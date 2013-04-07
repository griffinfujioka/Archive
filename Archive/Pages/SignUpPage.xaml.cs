using Archive.API_Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Windows.Storage;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Archive.Pages
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SignUpPage : Archive.Common.LayoutAwarePage
    {
        #region Variable declarations
        private Windows.Foundation.Collections.IPropertySet appSettings;
        private const String usernameKey = "Username";
        private const String passwordKey = "Password";
        public const String User = "User";
        #endregion

        #region Constructor
        public SignUpPage()
        {
            this.InitializeComponent();
            appSettings = ApplicationData.Current.LocalSettings.Values;
        }
        #endregion

        #region LoadState
        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }
        #endregion

        #region Save state
        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }
        #endregion

        #region OnNavigatedTo
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            emailAddressTxtBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
            base.OnNavigatedTo(e);
        }
        #endregion

        #region Submit sign up info
        private async void submitInfoBtn_Click_1(object sender, RoutedEventArgs e)
        {
            User responseUser = null;                     // The user which is returned if successful 
            int userID = -1;                             // userID of returned User

            // Get all of the information from the text fields
            var emailAddress = emailAddressTxtBox.Text;
            var username = usernameTxtBox.Text;
            var password = passwordTxtBox.Password;
            var confirmPassword = confirmPasswordTxtBox.Password;

            if (emailAddress == "" || username == "" || password == "" || confirmPassword == "")
            {
                var output = string.Format("Please fill out all fields.");
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(output);
                await dialog.ShowAsync();
                return; 
            }

            // Make sure the passwords match 
            if (password != confirmPassword)
            {
                var output = string.Format("Passwords must match.");
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(output);
                await dialog.ShowAsync();
                return; 
            }


            try
            {
                var signupRequest = new ApiRequest("signup");
                signupRequest.Authenticated = true;
                signupRequest.AddJsonContent(new { Email = emailAddress, Username = username, Password = password });
                responseUser = await signupRequest.ExecuteAsync<User>();

                if (responseUser != null)
                    userID = responseUser.UserId;
            }
            catch
            {
            }

            string successOutput;
            if (userID > 0)
            {
                successOutput = string.Format("Welcome to Archive, " + responseUser.Username + ".");
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(successOutput);
                await dialog.ShowAsync();
                App.LoggedInUser = responseUser;
                appSettings[usernameKey] = responseUser.Username;
                appSettings[passwordKey] = password;
                appSettings[User] = JsonConvert.SerializeObject(responseUser);
                this.Frame.Navigate(typeof(GroupedItemsPage)); 
            }
            else
            {
                successOutput = string.Format("Failed to create account. Please try again.");
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(successOutput);
                await dialog.ShowAsync();
            }


        }
        #endregion

        private void confirmPasswordTxtBox_KeyUp_1(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {

                submitInfoBtn_Click_1(sender, e); 
            }
        }
    }
}
