using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text; 
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
using Windows.Storage;          // ApplicationData
using Windows.Storage.Streams;
using System.Net.Http;          // For http handlers
using System.Net.Http.Headers;  // For ProductInfoHeaderValue class
using Archive.Common;
using Archive.Data;
using Archive.DataModel;
using Archive.API_Helpers; 
using Microsoft.Live;
using SkyDriveHelper;           // Wrapper for accessing SkyDrive 
using System.Runtime.Serialization.Json;    // JSON Serialization
using Newtonsoft.Json;              // JSONConvert
using Archive.JSON;
using Archive.Pages;
using Windows.UI.Popups; 



// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace Archive
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class GroupedItemsPage : Archive.Common.LayoutAwarePage
    {
        #region Variable declarations
        private Windows.Foundation.Collections.IPropertySet appSettings;
        private const String usernameKey = "Username";
        private const String passwordKey = "Password";
        public const String User = "User"; 
        public String ArchiveAPIuri = "http://trout.wadec.com/API";
        public String API_Key = "";
        public String API_Secret = "";
        #endregion 

        #region Constructor
        /// <summary>
        /// Page constructor
        /// </summary>
        public GroupedItemsPage()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;

            appSettings = ApplicationData.Current.LocalSettings.Values;
           
            var bounds = Window.Current.Bounds;
            var height = bounds.Height;
            var width = bounds.Width;

            if(App.HasNetworkConnection)
                AuthenticateArchiveAPI();       // Authenticate client app with Archive API

            // If the user is not logged in 
            if (!appSettings.ContainsKey(usernameKey) || !appSettings.ContainsKey(passwordKey))
            {
         
                usernameTxtBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
                loginPopUp.Visibility = Visibility.Collapsed; 
                logoutBtn.Visibility = Visibility.Collapsed;
                profileBtn.Visibility = Visibility.Collapsed; 
                loginBtn.Visibility = Visibility.Visible;
                signUpBtn.Visibility = Visibility.Visible;
                lowerButtonsStackPanel.Visibility = Visibility.Collapsed;
               
            }
            else
            {
                loginPopUp.Visibility = Visibility.Collapsed; 
                logoutBtn.Visibility = Visibility.Visible;
                profileBtn.Visibility = Visibility.Visible; 
                loginBtn.Visibility = Visibility.Collapsed;
                signUpBtn.Visibility = Visibility.Collapsed;
                lowerButtonsStackPanel.Visibility = Visibility.Visible;
                try
                {

                    var userJSON = appSettings[User];
                    App.LoggedInUser = JsonConvert.DeserializeObject<User>(appSettings[User].ToString());
                   
                }
                catch 
                {
                    // Do something here!
                }
            }
            
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
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            
            // Load user's video from Archive API
            if (App.LoggedInUser != null)
            {
                try
                {

                    await App.LoadUsersVideos();
                    IEnumerable<VideoDataGroup> ArchiveGroup = App.ArchiveVideos.AllGroups;
                    if (ArchiveGroup != null)
                        this.DefaultViewModel["Groups"] = ArchiveGroup;
                }
                catch
                {
                    // Do something here
                }
            }
            else
            {
                this.DefaultViewModel["Groups"] = null; 
            }
        }
        #endregion 

        #region Header_Click : Invoked when a group header is clicked.
        /// <summary>
        /// Invoked when a group header is clicked.
        /// </summary>
        /// <param name="sender">The Button used as a group header for the selected group.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Header_Click(object sender, RoutedEventArgs e)
        {
            // Determine what group the Button instance represents
            var group = (sender as FrameworkElement).DataContext;

            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            this.Frame.Navigate(typeof(GroupDetailPage), ((VideoDataGroup)group).VideoId);
        }
        #endregion 

        #region ItemView_ItemClick : Invoked when an item within a group is clicked.
        /// <summary>
        /// Invoked when an item within a group is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((VideoModel)e.ClickedItem).VideoId;
            this.Frame.Navigate(typeof(ItemDetailPage), itemId);
        }
        #endregion 

        #region Capture new video 
        private void newvideoBtn_Click_1(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CapturePage));
        }
        #endregion 

        #region Submit login credentials button click
        private void submitLoginBtn_Click_1(object sender, RoutedEventArgs e)
        {
            var username = usernameTxtBox.Text;
            var password = passwordTxtBox.Password;
            Authenticate_User(username, password); 
        }
        #endregion

        #region Logout button clicked 
        private async void signOutButton_Click_1(object sender, RoutedEventArgs e)
        {
            appSettings.Remove(usernameKey);    // Clear the username key from appSettings
            appSettings.Remove(passwordKey);    // Clear the password key from appSettings
            appSettings.Remove(User);           // Clear the User key from appSettings
            Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("You are now logged out.");
            await dialog.ShowAsync();
            loginBtn.Visibility = Visibility.Visible;
            signUpBtn.Visibility = Visibility.Visible;
            logoutBtn.Visibility = Visibility.Collapsed;
            profileBtn.Visibility = Visibility.Collapsed; 
            lowerButtonsStackPanel.Visibility = Visibility.Collapsed;

            // Clear the username and password textboxes
            usernameTxtBox.Text = "";
            passwordTxtBox.Password = "";

            this.DefaultViewModel["Groups"] = null; 
            App.ArchiveVideos = null; 

        }
        #endregion 

        #region Login button clicked
        private async void loginBtn_Click_1(object sender, RoutedEventArgs e)
        {
            if (!App.HasNetworkConnection)
            {

                var output = string.Format("Archive has detected that network connectivity is not available.");
                output += "\nSome features may be unavailable until network connectivity is restored.";
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(output);
                await dialog.ShowAsync();
                return;

            }
            loginPopUp.Visibility = Visibility.Visible;
            loginBtn.Visibility = Visibility.Collapsed;
            signUpBtn.Visibility = Visibility.Visible; 
            usernameTxtBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
             
        }
        #endregion 

        #region AuthenticateArchiveAPI
        /// <summary>
        ///  Sends http request to Archive API to authenticate app as an Archive client app
        /// </summary>
        /// <returns></returns>
        public async void AuthenticateArchiveAPI()
        {
            // Testing some functionality here: trying to contact Archive API
            HttpClient httpClient;
            HttpMessageHandler handler = new HttpClientHandler();
            handler = new PlugInHandler(handler); // Adds a custom header to every request and response message.            
            httpClient = new HttpClient(handler);


            var AuthenticationURI = (ArchiveAPIuri + "/REQUEST_token");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, AuthenticationURI);
            httpRequestMessage.Headers.Add("X-ApiKey", "123456");
            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            // Use an enum like that: System.Net.HttpStatusCode.Accepted;

            switch ((int)response.StatusCode)
            {
                case 200:       // Everything is good! 
                    App.API_Authenticated = true; 
                    // Parse some JSON to receive RequestTokenResponse: (string token, string method)
                    break; 
                default:
                    break; 
            }
        }
        #endregion 

        #region Sync button 
        private async void syncButton_Click_1(object sender, RoutedEventArgs e)
        {
            // Load user's video from Archive API
            await App.LoadUsersVideos();
            IEnumerable<VideoDataGroup> ArchiveGroup = App.ArchiveVideos.AllGroups;
            if (ArchiveGroup != null)
                this.DefaultViewModel["Groups"] = ArchiveGroup;
        }
        #endregion 

        #region Authenticate user 
        public async void Authenticate_User(string username, string password)
        {

            if (!App.HasNetworkConnection)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("An network connection is needed to login.");
                await dialog.ShowAsync();
                return;
            }

            

            try
            {
                var loginRequest = new ApiRequest("login");
                loginRequest.Authenticated = true;
                loginRequest.AddJsonContent(new { Username = username, Password = password});
                App.LoggedInUser = await loginRequest.ExecuteAsync<User>();

    
                appSettings[usernameKey] = username;
                appSettings[passwordKey] = password;
                appSettings[User] = JsonConvert.SerializeObject(App.LoggedInUser);

                #region Adjust UI controls 
                loginPopUp.Visibility = Visibility.Collapsed; 
                logoutBtn.Visibility = Visibility.Visible;
                profileBtn.Visibility = Visibility.Visible;
                loginBtn.Visibility = Visibility.Collapsed;
                signUpBtn.Visibility = Visibility.Collapsed; 
                usernameTxtBlock.Focus(Windows.UI.Xaml.FocusState.Pointer);
                #endregion

                
                await App.LoadUsersVideos();
                IEnumerable<VideoDataGroup> ArchiveGroup = App.ArchiveVideos.AllGroups;
                if (ArchiveGroup != null)
                    this.DefaultViewModel["Groups"] = ArchiveGroup;
                
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Welcome back " + username + "!");
                await dialog.ShowAsync();
                lowerButtonsStackPanel.Visibility = Visibility.Visible;
                 
            }
            catch (ApiException ex)
            {
                // api returned something other than 200
                InvalidLoginCredentials();
            }
            catch (Exception ex)
            {
                // If user credentials are invalid, we will catch it here 
                InvalidLoginCredentials();
            }

            

        }
        #endregion 

        #region InvalidLoginCredentials
        private async void InvalidLoginCredentials()
        {
            var output = "Invalid login credentials. Please try again.";
            Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(output);
            await dialog.ShowAsync();
            profileBtn.Visibility = Visibility.Collapsed; 
            loginPopUp.Visibility = Visibility.Visible;
            loginBtn.Visibility = Visibility.Collapsed;
            signUpBtn.Visibility = Visibility.Visible;
            lowerButtonsStackPanel.Visibility = Visibility.Collapsed;
        }
        #endregion 

        #region OnLoaded
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!App.HasNetworkConnection)
            {
                var output = string.Format("Archive has detected that network connectivity is not available.");
                output += "\nSome features may be unavailable until network connectivity is restored.";
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(output);
                await dialog.ShowAsync();
            }
        }
        #endregion

        #region Sign up button clicked
        private async void signUpBtn_Click_1(object sender, RoutedEventArgs e)
        {
            if (!App.HasNetworkConnection)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("An network connection is needed to sign up.");
                await dialog.ShowAsync();
                return;
            }

            this.Frame.Navigate(typeof(SignUpPage)); 
        }
        #endregion

        #region Profile button clicked
        private void profileBtn_Click_1(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ProfilePage),App.LoggedInUser.UserId); 
        }
        #endregion

        private void findFriendsBtn_Click_1(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FindFriends)); 
        }

        private void passwordTxtBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string username = usernameTxtBox.Text;
                string password = passwordTxtBox.Password;

                Authenticate_User(username, password);
            }
        }


    }
}
