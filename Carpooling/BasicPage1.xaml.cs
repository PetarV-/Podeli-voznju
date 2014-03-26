using Carpooling.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;

using SQLite;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Carpooling
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class BasicPage1 : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private SQLiteAsyncConnection conn;

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


        public BasicPage1()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            //// InputScope for mail
            //InputScope inputScopeMail = new InputScope();
            //InputScopeName inputScopeNameMail = new InputScopeName();
            //inputScopeNameMail.NameValue = InputScopeNameValue.EmailSmtpAddress;
            //inputScopeMail.Names.Add(inputScopeNameMail);
            //Email.InputScope = inputScopeMail;

            //// InputScore for phone number
            //InputScope inputScopePhone = new InputScope();
            //InputScopeName inputScopeNamePhone = new InputScopeName();
            //inputScopeNamePhone.NameValue = InputScopeNameValue.TelephoneNumber;
            //inputScopePhone.Names.Add(inputScopeNamePhone);
            //PhoneNumber.InputScope = inputScopePhone;

            ConnData();
            UpdateProfileName();
        }

        private void ConnData()
        {
            var dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLiteAsyncConnection(dbPath);
        }

        public async void UpdateProfileName()
        {
            List<Profile> queryList = await conn.QueryAsync<Profile>("Select * from Profile where ID_P=?", SignIn.loginID);

            if( queryList.Count > 0 )
                pageTitle.Text = "Profile: " + queryList[0].Ime + " " + queryList[0].Prezime;
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


        private void EmailClick(object sender, RoutedEventArgs e)
        {
            Email.IsEnabled = true;
        }

        private void Email_LostFocus(object sender, RoutedEventArgs e)
        {
            Email.IsEnabled = false;
        }

        private void Email_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if( e.Key == VirtualKey.Enter )
            {
                Email.IsEnabled = false;
            }
        }

        private void PhoneNumber_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                PhoneNumber.IsEnabled = false;
            }
        }

        private void PhoneNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            PhoneNumber.IsEnabled = false;
        }

        private void PhoneNumberClick(object sender, RoutedEventArgs e)
        {
            PhoneNumber.IsEnabled = true;
        }

        private void goToProfile(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(BasicPage1));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(MainPage));
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

        private void goToRoutePlanner(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(RoutePlanner));
            }
        }

    }
}
