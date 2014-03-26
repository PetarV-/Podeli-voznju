using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NotificationsExtensions.ToastContent;
using SQLite;
using Bing.Maps;
using Bing.Maps.Search;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Carpooling
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SQLiteAsyncConnection conn;
        DateTime chosenDate, chosenTime;
        private int last_id = -1;
        public static Location Start = null;
        public static Location End = null;
        public static Pushpin start = null;
        public static Pushpin end = null;
        public static Bing.Maps.Directions.Route route;
        string comment;
        int availableSeats;
        public MainPage()
        {
            this.InitializeComponent();
            ConnData();

            DateTimeOffset offsetMin = new DateTimeOffset(DateTime.Now);
            DateTimeOffset offsetMax = new DateTimeOffset(new DateTime(2014, 12, 30));
            ChooseDate.Date = offsetMin;
            ChooseDate.MaxYear = offsetMax;

            //this.Submit.Click += (sender, e ) => { DisplayTextToast(); };
        }

        private void ConnData()
        {
            var dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLiteAsyncConnection(dbPath);
        }
        void DisplayTextToast()
        {
            IToastNotificationContent toastContent = null;
            IToastText04 templateContent = ToastContentFactory.CreateToastText04();
            templateContent.TextHeading.Text = "Čestitamo!";
            templateContent.TextBody1.Text = "Uspešno ste kreirali rutu.";
            templateContent.TextBody2.Text = "Vaš jedinstveni identifikator rute je: " + last_id.ToString();
            toastContent = templateContent;
            NotifyUser(toastContent.GetContent(), NotifyType.StatusMessage);

            // Create a toast, then create a ToastNotifier object to show
            // the toast
            ToastNotification toast = toastContent.CreateNotification();

            // If you have other applications in your package, you can specify the AppId of
            // the app to create a ToastNotifier for that application
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    //StatusBlock.Style = Resources["StatusStyle"] as Style;
                    break;
                case NotifyType.ErrorMessage:
                    //StatusBlock.Style = Resources["ErrorStyle"] as Style;
                    break;
            }
            //StatusBlock.Text = strMessage;
        }
        

        private void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            chosenDate = new DateTime(e.NewDate.Year, e.NewDate.Month, e.NewDate.Day);
        }

        private void TimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            DateTime chosenTime = new DateTime(chosenDate.Year, chosenDate.Month, chosenDate.Day, e.NewTime.Hours, e.NewTime.Minutes, 0);
        }

        private void Comment_LostFocus(object sender, RoutedEventArgs e)
        {
            comment = e.OriginalSource.ToString();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // h4ckz0r
            availableSeats = Convert.ToInt32(FreeSpots.SelectedIndex.ToString()) + 1;
            //Comment.Text = availableSeats.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // TODO id, route u bazu
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(RoutView));
            }
        }

        private void goToProfile(object sender, RoutedEventArgs e)
        {
            if(this.Frame != null )
            {
                this.Frame.Navigate(typeof(BasicPage1));
            }
        }

        private void SignOut(object sender, RoutedEventArgs e)
        {
            if (SignIn.loginID != -1)
            {
                SignIn.loginID = -1;
                Frame.Navigate(typeof(SignIn));
            }
        }
        
        private void goToRouteView(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(RoutView));
            }
        }
        public async void PerformSearch(string Address, string from)
        {
            // Set the address string to geocode
            Bing.Maps.Search.GeocodeRequestOptions requestOptions = new Bing.Maps.Search.GeocodeRequestOptions(Address);
            // Make the geocode request 
            Bing.Maps.Search.SearchManager searchManager = myMap.SearchManager;
            Bing.Maps.Search.LocationDataResponse response = await searchManager.GeocodeAsync(requestOptions);
            if (response.LocationData.Count > 0)
            {
                if (from == "start")
                {
                    Start = response.LocationData[0].Location;
                    Bing.Maps.Pushpin pushpin = new Bing.Maps.Pushpin();
                    pushpin.SetValue(Bing.Maps.MapLayer.PositionProperty, Start);
                    if (start != null)
                        myMap.Children.Remove(start);
                    start = pushpin;
                    this.myMap.Children.Add(pushpin);
                    if (Start != null && End != null)
                        GetDirections(Start, End);
                }
                else
                {
                    End = response.LocationData[0].Location;
                    Bing.Maps.Pushpin pushpin = new Bing.Maps.Pushpin();
                    pushpin.SetValue(Bing.Maps.MapLayer.PositionProperty, End);
                    if (end != null)
                        myMap.Children.Remove(end);
                    end = pushpin;
                    this.myMap.Children.Add(pushpin);
                }
                if (Start != null && End != null)
                    GetDirections(Start, End);

            }
        }
        private void LostFocusStart(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text != string.Empty)
                PerformSearch((sender as TextBox).Text, "start");
        }

        private void LostFocusEnd(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text != string.Empty)
                PerformSearch((sender as TextBox).Text, "end");
        }
        public async void GetDirections(Location c, Location l)
        {
            // Set the start and end waypoints
            Bing.Maps.Directions.Waypoint startWaypoint = new Bing.Maps.Directions.Waypoint(c);
            Bing.Maps.Directions.Waypoint endWaypoint = new Bing.Maps.Directions.Waypoint(l);

            Bing.Maps.Directions.WaypointCollection waypoints = new Bing.Maps.Directions.WaypointCollection();
            waypoints.Add(startWaypoint);
            waypoints.Add(endWaypoint);

            Bing.Maps.Directions.DirectionsManager directionsManager = myMap.DirectionsManager;
            directionsManager.Waypoints = waypoints;

            // Calculate route directions
            Bing.Maps.Directions.RouteResponse response = await directionsManager.CalculateDirectionsAsync();

            // Display the route on the map
            if (response.Routes.Count > 0)
            {
                if (route != null)
                    directionsManager.HideRoutePath(route);
                route = response.Routes[0];
                directionsManager.ShowRoutePath(response.Routes[0]);
            }

        }

        private async void RightClick(object sender, RightTappedRoutedEventArgs e)
        {
            this.myMap.TryPixelToLocation(new Point(e.GetPosition(this.myMap).X, e.GetPosition(this.myMap).Y), out End);
            Bing.Maps.Pushpin pushpin = new Bing.Maps.Pushpin();
            pushpin.SetValue(Bing.Maps.MapLayer.PositionProperty, End);
            if (end != null)
                myMap.Children.Remove(end);
            end = pushpin;
            this.myMap.Children.Add(pushpin);

            Bing.Maps.Search.ReverseGeocodeRequestOptions requestOptions = new ReverseGeocodeRequestOptions(End);
            Bing.Maps.Search.SearchManager searchManager = myMap.SearchManager;
            Bing.Maps.Search.LocationDataResponse response = await searchManager.ReverseGeocodeAsync(requestOptions);
            if (response.LocationData.Count > 0)
            {
                RouteTo.Text = response.LocationData[0].Address.FormattedAddress;
            }

            if (Start != null && End != null)
                GetDirections(Start, End);
        }

        private async void LeftClick(object sender, TappedRoutedEventArgs e)
        {
            this.myMap.TryPixelToLocation(new Point(e.GetPosition(this.myMap).X, e.GetPosition(this.myMap).Y), out Start);
            Bing.Maps.Pushpin pushpin = new Bing.Maps.Pushpin();
            pushpin.SetValue(Bing.Maps.MapLayer.PositionProperty, Start);
            if (start != null)
                myMap.Children.Remove(start);
            start = pushpin;
            this.myMap.Children.Add(pushpin);

            Bing.Maps.Search.ReverseGeocodeRequestOptions requestOptions = new ReverseGeocodeRequestOptions(Start);
            Bing.Maps.Search.SearchManager searchManager = myMap.SearchManager;
            Bing.Maps.Search.LocationDataResponse response = await searchManager.ReverseGeocodeAsync(requestOptions);
            if (response.LocationData.Count > 0)
            {
                RouteFrom.Text = response.LocationData[0].Address.FormattedAddress;
            }

            if (Start != null && End != null)
                GetDirections(Start, End);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (RouteFrom.Text.Length > 3 && RouteTo.Text.Length > 3)
            {
                AddRoute();
                this.Frame.Navigate(typeof(RoutView));
            }
        }

        private async void AddRoute()
        {

            Route r = new Route()
            {
                Br_mesta = Convert.ToInt32(FreeSpots.SelectedIndex.ToString()) + 1,
                Caption = Comment.Text,
                Termin =
                    new DateTime(ChooseDate.Date.Year, chosenDate.Date.Month, chosenDate.Date.Day, ChooseTime.Time.Hours,
                        ChooseTime.Time.Minutes, 0),
                ID_DRIVER = SignIn.loginID,
                Latitude_F = End.Latitude,
                Latitude_S = Start.Latitude,
                Longitude_F = End.Longitude,
                Longitude_S = Start.Longitude
            };
            await conn.InsertAsync(r);
            last_id = r.ID_R;
            DisplayTextToast();
        }
    }
}
