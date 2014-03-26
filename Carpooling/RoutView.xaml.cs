using Windows.UI.Xaml.Shapes;
using Bing.Maps;
using Bing.Maps.Search;
using Carpooling.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SQLite;
using Path = System.IO.Path;
using Windows.ApplicationModel.DataTransfer;
using NotificationsExtensions.ToastContent;
using Windows.UI.Notifications;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Carpooling
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class RoutView : Page
    {
        public double temp;
        public static Location Start = null;
        public static Location End = null;
        public static Pushpin start = null;
        public static Pushpin end = null;
        public static Bing.Maps.Directions.Route route;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        SQLiteAsyncConnection conn;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public RoutView()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            myMap.Culture = "sr-Latn-RS";
            myMap.Language = "en-US";

            this.joinBtn.Click += (sender, e) => { DisplayTextToast(); };

            RegisterForShare();
            ConnData();
            ReadFromDB();
        }

        private void ConnData()
        {
            var dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLiteAsyncConnection(dbPath);
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        private void ItemListViewOnItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            Bing.Maps.Pushpin pushpin = new Bing.Maps.Pushpin();
            pushpin.SetValue(Bing.Maps.MapLayer.PositionProperty, new Location(myMap.Center.Latitude, myMap.Center.Longitude));
            this.myMap.Children.Add(pushpin);
        }

        private async void ReadFromDB()
        {
            try
            {
                List<Route> results = await conn.QueryAsync<Route>("SELECT * FROM Route");

                if (results.Count > 0)
                {
                    foreach (var result in results)
                    {
                        string s = "";
                        List<Profile> res = await conn.QueryAsync<Profile>("SELECT * FROM Profile WHERE ID_P=" + result.ID_DRIVER);
                        if (res.Count > 0)
                            s += res[0].Ime + " " + res[0].Prezime + Environment.NewLine;
                        Bing.Maps.Search.ReverseGeocodeRequestOptions requestOptions = new ReverseGeocodeRequestOptions(new Location(result.Latitude_S, result.Longitude_S));
                        Bing.Maps.Search.SearchManager searchManager = myMap.SearchManager;
                        Bing.Maps.Search.LocationDataResponse response = await searchManager.ReverseGeocodeAsync(requestOptions);
                        if (response.LocationData.Count > 0)
                        {
                            s += response.LocationData[0].Address.FormattedAddress + " -> " + Environment.NewLine;
                        }
                        requestOptions = new ReverseGeocodeRequestOptions(new Location(result.Latitude_F, result.Longitude_F));
                        searchManager = myMap.SearchManager;
                        response = await searchManager.ReverseGeocodeAsync(requestOptions);
                        if (response.LocationData.Count > 0)
                        {
                            s += response.LocationData[0].Address.FormattedAddress + Environment.NewLine;
                        }
                        s += result.Termin + " - ";
                        s += result.Br_mesta + " mesta";
                        itemListView.Items.Add(s);
                    }
                }
            }
            catch (Exception e) { }
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

        private void RightClick(object sender, RightTappedRoutedEventArgs e)
        {
            this.myMap.TryPixelToLocation(new Point(e.GetPosition(this.myMap).X, e.GetPosition(this.myMap).Y), out End);
            Bing.Maps.Pushpin pushpin = new Bing.Maps.Pushpin();
            pushpin.SetValue(Bing.Maps.MapLayer.PositionProperty, End);
            if (end != null)
                myMap.Children.Remove(end);
            end = pushpin;
            this.myMap.Children.Add(pushpin);
            if (Start != null && End != null)
                GetDirections(Start, End);
        }

        private void LeftClick(object sender, TappedRoutedEventArgs e)
        {
            this.myMap.TryPixelToLocation(new Point(e.GetPosition(this.myMap).X, e.GetPosition(this.myMap).Y), out Start);
            Bing.Maps.Pushpin pushpin = new Bing.Maps.Pushpin();
            pushpin.SetValue(Bing.Maps.MapLayer.PositionProperty, Start);
            if (start != null)
                myMap.Children.Remove(start);
            start = pushpin;
            this.myMap.Children.Add(pushpin);
            if (Start != null && End != null)
                GetDirections(Start, End);
        }
        public async System.Threading.Tasks.Task<double> calculateDistance(Location c, Location l)
        {
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
                return temp = response.Routes[0].TravelDistance;
            }
            return temp = Double.MaxValue;
        }
        private async void filterButton_Click(object sender, RoutedEventArgs e)
        {
            // hide the flyout
            filterBtn.Flyout.Hide();

            // process the query
            // sample query: SELECT * FROM Route WHERE ...
            // Possible WHEREs:
            // if from-to tolerance exists.
            // if time tolerance exists.
            // if space setting exists.

            string baseQuery = "SELECT * FROM Route";
            bool where_has_been = false;

            if (tbSeatNo.Text != string.Empty)
            {
                int stNo;
                if (int.TryParse(tbSeatNo.Text, out stNo))
                {
                    if (!where_has_been) baseQuery += " WHERE";
                    else baseQuery += " AND";
                    where_has_been = true;
                    baseQuery += " Route.Br_mesta >= " + stNo.ToString();
                }
            }

            List<Route> retRoutes = await conn.QueryAsync<Route>(baseQuery);

            if (tbMinutes.Text != string.Empty)
            {
                int minNo;
                if (int.TryParse(tbMinutes.Text, out minNo))
                {
                    DateTime chosenTime = new DateTime(chooseDate.Date.Year, chooseDate.Date.Month, chooseDate.Date.Day,
                        chooseTime.Time.Hours, chooseTime.Time.Minutes, 0);
                    foreach (Route r in retRoutes.ToList<Route>())
                    {
                        if (r.Termin.Subtract(chosenTime).Duration().CompareTo(new TimeSpan(0, minNo, 0)) > 0)
                        {
                            retRoutes.Remove(r);
                        }
                    }
                }
            }

            if (tbMeters.Text != string.Empty)
            {
                int metNo;
                if (int.TryParse(tbMeters.Text, out metNo))
                {
                    Location userstart = new Location();
                    Location userend = new Location();
                    Bing.Maps.Search.GeocodeRequestOptions requestOptions = new Bing.Maps.Search.GeocodeRequestOptions(tbFrom.Text);
                    Bing.Maps.Search.SearchManager searchManager = myMap.SearchManager;
                    Bing.Maps.Search.LocationDataResponse response = await searchManager.GeocodeAsync(requestOptions);
                    if (response.LocationData.Count > 0)
                    {
                        userstart = response.LocationData[0].Location;
                    }
                    requestOptions = new Bing.Maps.Search.GeocodeRequestOptions(tbTo.Text);
                    searchManager = myMap.SearchManager;
                    response = await searchManager.GeocodeAsync(requestOptions);
                    if (response.LocationData.Count > 0)
                    {
                        userend = response.LocationData[0].Location;
                    }
 
                    foreach (Route r in retRoutes.ToList<Route>())
                    {
                        double dist = 0;
                        calculateDistance(new Location(r.Latitude_S, r.Longitude_S), userstart);
                        dist += temp;
                        calculateDistance(new Location(r.Latitude_F, r.Longitude_F), userend);
                        dist += temp;
 
                        if (dist > metNo)
                        {
                            retRoutes.Remove(r);
                            continue;
                        }
                        dist = 0;
                        calculateDistance(new Location(r.Latitude_S, r.Longitude_S), userend);
                        dist += temp;
                        calculateDistance(new Location(r.Latitude_F, r.Longitude_F), userstart);
                        dist += temp;
 
                        if (dist > metNo)
                        {
                            retRoutes.Remove(r);
                        }
                    }
                    
                }
            
            }

            // TODO PASS RETROUTES TO THE LISTVIEW
            itemListView.Items.Clear();
            foreach (var retRoute in retRoutes)
            {
                string s = "";
                List<Profile> res = await conn.QueryAsync<Profile>("SELECT * FROM Profile WHERE ID_P=" + retRoute.ID_DRIVER);
                if (res.Count > 0)
                    s += res[0].Ime + " " + res[0].Prezime + Environment.NewLine;
                Bing.Maps.Search.ReverseGeocodeRequestOptions requestOptions1 = new ReverseGeocodeRequestOptions(new Location(retRoute.Latitude_S, retRoute.Longitude_S));
                Bing.Maps.Search.SearchManager searchManager1 = myMap.SearchManager;
                Bing.Maps.Search.LocationDataResponse response1 = await searchManager1.ReverseGeocodeAsync(requestOptions1);
                if (response1.LocationData.Count > 0)
                {
                    s += response1.LocationData[0].Address.FormattedAddress + " -> " + Environment.NewLine;
                }
                requestOptions1 = new ReverseGeocodeRequestOptions(new Location(retRoute.Latitude_S, retRoute.Longitude_S));
                searchManager1 = myMap.SearchManager;
                response1 = await searchManager1.ReverseGeocodeAsync(requestOptions1);
                if (response1.LocationData.Count > 0)
                {
                    s += response1.LocationData[0].Address.FormattedAddress + Environment.NewLine;
                }
                s += retRoute.Termin + "     ";
                s += retRoute.Br_mesta + "mesta";
                itemListView.Items.Add(s);
            }

        }

        private void goToProfile(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
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
        private async void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Invalidate the view state when logical page navigation is in effect, as a change
            // in selection may cause a corresponding change in the current logical page.  When
            // an item is selected this has the effect of changing from displaying the item list
            // to showing the selected item's details.  When the selection is cleared this has the
            // opposite effect.
            if ((sender as ListView).SelectedItem == null)
            {
                myMap.Children.Clear();
                return;
            }
            string s = (sender as ListView).SelectedItem.ToString();
            s = s.Remove(0, s.IndexOf(Environment.NewLine) + 2);
            Bing.Maps.Search.GeocodeRequestOptions requestOptions = new Bing.Maps.Search.GeocodeRequestOptions(s.Substring(0, s.IndexOf("->")).Trim());
            Bing.Maps.Search.SearchManager searchManager = myMap.SearchManager;
            Bing.Maps.Search.LocationDataResponse response = await searchManager.GeocodeAsync(requestOptions);
            if (response.LocationData.Count > 0)
            {
                Start = response.LocationData[0].Location;
            }
            s = s.Remove(0, s.IndexOf(Environment.NewLine) + 2);
            requestOptions = new Bing.Maps.Search.GeocodeRequestOptions(s.Substring(0, s.IndexOf(Environment.NewLine)));
            searchManager = myMap.SearchManager;
            response = await searchManager.GeocodeAsync(requestOptions);
            if (response.LocationData.Count > 0)
            {
                End = response.LocationData[0].Location;
            }
            GetDirections(Start, End);

            //if (this.UsingLogicalPageNavigation()) this.InvalidateVisualState();
        }

        private void filterBtn_Click(object sender, RoutedEventArgs e)
        {

        }


        private void goToRoutePlanner(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(RoutePlanner));
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }
        private void RegisterForShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.ShareTextHandler);
        }

        private void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;

            // Now add the data you want to share.
            request.Data.Properties.Title = "Podeli rutu";
            String s = " ";
            if (itemListView.SelectedItem != null) s = itemListView.SelectedItem.ToString();
            request.Data.SetText("Video sam odličnu vožnju u aplikaciji \"Podeli vožnju\"! \n" + s);
        }

        private void ShareProgrammatically()
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
        }

        void DisplayTextToast()
        {
            IToastNotificationContent toastContent = null;
            IToastText04 templateContent = ToastContentFactory.CreateToastText04();
            templateContent.TextHeading.Text = "Čestitamo!";
            templateContent.TextBody1.Text = "Uspešno ste se priključili ruti!";
            templateContent.TextBody2.Text = "Vozač će vas uskoro kontaktirati.";
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

        private void joinBtn_Click(object sender, RoutedEventArgs e)
        {
            if (itemListView.SelectedItem != null)
            {

            }
        }
    }
}
