using Archive.Data;

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
using Windows.Storage;  // ApplicationData
using System.Net.Http;          // For http handlers
using System.Net.Http.Headers;  // For ProductInfoHeaderValue class
using Archive.Common; 

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
        public String ArchiveAPIuri = "http://trout.wadec.com/API";
        public String API_Key = "";
        public String API_Secret = "";
        #endregion 

        #region Constructor
        public GroupedItemsPage()
        {
            this.InitializeComponent();

            appSettings = ApplicationData.Current.LocalSettings.Values;

            var bounds = Window.Current.Bounds;
            var height = bounds.Height;
            var width = bounds.Width;

            AuthenticateArchiveAPI();       // Authenticate client app with Archive API

            // If the user is not logged in 
            if (!appSettings.ContainsKey(usernameKey) || !appSettings.ContainsKey(passwordKey))
            {
                loginPopUp.IsOpen = true;
                usernameTxtBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
                logoutBtn.Visibility = Visibility.Collapsed;
                loginBtn.Visibility = Visibility.Visible; 
            }
            else
            {
                loginPopUp.IsOpen = false;
                logoutBtn.Visibility = Visibility.Visible;
                loginBtn.Visibility = Visibility.Collapsed; 
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
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = SampleDataSource.GetGroups((String)navigationParameter);
            this.DefaultViewModel["Groups"] = sampleDataGroups;
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
            this.Frame.Navigate(typeof(GroupDetailPage), ((SampleDataGroup)group).UniqueId);
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
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
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
        private async void submitLoginBtn_Click_1(object sender, RoutedEventArgs e)
        {
            // TODO: Verify login credentials against Archive API
            var username = usernameTxtBox.Text;
            var password = passwordTxtBox.Password;
            
           
            appSettings[usernameKey] = usernameTxtBox.Text;
            appSettings[passwordKey] = passwordTxtBox.Password;
            loginPopUp.IsOpen = false;
            logoutBtn.Visibility = Visibility.Visible;
            loginBtn.Visibility = Visibility.Collapsed;
            usernameTxtBlock.Focus(Windows.UI.Xaml.FocusState.Pointer); 
        }
        #endregion

        #region Logout button clicked 
        private async void signOutButton_Click_1(object sender, RoutedEventArgs e)
        {
            appSettings.Remove(usernameKey);    // Clear the username key from appSettings
            appSettings.Remove(passwordKey);    // Clear the password key from appSettings
            Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("You are now logged out.");
            await dialog.ShowAsync();
            loginBtn.Visibility = Visibility.Visible;
            logoutBtn.Visibility = Visibility.Collapsed; 
        }
        #endregion 

        #region Login button clicked
        private void loginBtn_Click_1(object sender, RoutedEventArgs e)
        {
            loginPopUp.IsOpen = true;
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
            MultipartFormDataContent form = new MultipartFormDataContent();


            ArchiveAPIuri = (ArchiveAPIuri + "/REQUEST_token");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, ArchiveAPIuri);
            httpRequestMessage.Headers.Add("X-ApiKey", "123456");
            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            // Use an enum like that: System.Net.HttpStatusCode.Accepted;

            switch ((int)response.StatusCode)
            {
                case 200:       // Everything is good! 
                    // Parse some JSON to receive RequestTokenResponse: (string token, string method)
                    break; 
                default:
                    break; 
            }
        }
        #endregion 


    }
}
