using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Xml.Dom;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using SQLite;

namespace Carpooling
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignIn : Page
    {
        private SQLiteAsyncConnection conn;
        public static int loginID = -1;
        public SignIn()
        {
            this.InitializeComponent();
            ConnData();
            InitData();
        }

        private void ConnData()
        {
            var dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLiteAsyncConnection(dbPath);
        }

        private async void InitData()
        {
            var result = await conn.CreateTablesAsync<Profile, Route, Hitch>();
        }


        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (User.Text == string.Empty || Pass.Password == string.Empty)
            {
                Message1.Visibility = Visibility.Visible;
                Message2.Visibility = Visibility.Collapsed;
                Message1.Text = "Morate popuniti oba polja!";
                return;
            }
            List<Profile> results = await conn.QueryAsync<Profile>("SELECT * FROM Profile WHERE Profile.Username='" + User.Text + "' AND Profile.Password='" + Pass.Password + "'");
            if (results.Count > 0)
            {
                loginID = results[0].ID_P;
                Frame.Navigate(typeof (RoutView));
            }
            else
            {
                Message1.Visibility = Visibility.Visible;
                Message2.Visibility = Visibility.Collapsed;
                Message1.Text = "Pogrešno korisničko ime i/ili lozinka!";
            }
        }

        private async void SignUp_Click(object sender, RoutedEventArgs e)
        {
            if (Username.Text == string.Empty || Password.Password == string.Empty || Name.Text == string.Empty ||
                Surname.Text == string.Empty || JMBG.Text == string.Empty || Phone.Text == string.Empty ||
                Mail.Text == string.Empty)
            {
                Message2.Visibility = Visibility.Visible;
                Message1.Visibility = Visibility.Collapsed;
                Message2.Text = "Morate popuniti sva polja.";
                return;
            }

            List<Result> results = await conn.QueryAsync<Result>("SELECT Profile.Username, Profile.Password FROM Profile WHERE Profile.JMBG='" + JMBG.Text + "' OR Profile.Email='" + Mail.Text + "'");
            if (results.Count > 0)
            {
                Message2.Visibility = Visibility.Visible;
                Message1.Visibility = Visibility.Collapsed;
                Message2.Text = "Korisnik sa datim podacima već postoji.";
            }
            else
            {
                Profile p = new Profile()
                {
                    Ime = Name.Text,
                    Prezime = Surname.Text,
                    Jmbg = JMBG.Text,
                    Tel = Phone.Text,
                    Email = Mail.Text,
                    Password = Password.Password,
                    Username = Username.Text
                };
                await conn.InsertAsync(p);
                List<Profile> res = await conn.QueryAsync<Profile>("SELECT * FROM Profile WHERE Profile.Username='" + Username.Text + "' AND Profile.Password='" + Password.Password + "'");
                if (res.Count > 0)
                {
                    loginID = res[0].ID_P;
                    Frame.Navigate(typeof(RoutView));
                }
            }
        }

        private void Pass_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if( e.Key == Windows.System.VirtualKey.Enter )
                SignIn_Click(sender, e);
        }

        private void User_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SignIn_Click(sender, e);
        }

        private void Name_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SignUp_Click(sender, e);
        }

        private void Surname_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SignUp_Click(sender, e);
        }

        private void JMBG_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SignUp_Click(sender, e);
        }

        private void Phone_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SignUp_Click(sender, e);
        }

        private void Mail_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SignUp_Click(sender, e);
        }

        private void Username_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SignUp_Click(sender, e);
        }

        private void Password_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SignUp_Click(sender, e);
        }
    }
}
