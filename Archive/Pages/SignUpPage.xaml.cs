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
        private Windows.Foundation.Collections.IPropertySet appSettings;
        private const String usernameKey = "Username";
        private const String passwordKey = "Password";
        public const String User = "User"; 

        public SignUpPage()
        {
            this.InitializeComponent();
            emailAddressTxtBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
            appSettings = ApplicationData.Current.LocalSettings.Values;
        }

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
            emailAddressTxtBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            emailAddressTxtBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
            base.OnNavigatedTo(e);
        }

        private async void submitInfoBtn_Click_1(object sender, RoutedEventArgs e)
        {
            WebResponse response;                   // Response from createvideo URL 
            Stream responseStream;                  // Stream data from responding URL
            StreamReader reader;                    // Read data in stream 
            string responseJSON;                    // The JSON string returned to us by the Archive API 
            User responseUser = null;                     // The user which is returned if successful 
            int userID = -1;                             // userID of returned User

            // Get all of the information from the text fields
            var emailAddress = emailAddressTxtBox.Text;
            var username = usernameTxtBox.Text;
            var password = passwordTxtBox.Password;
            var confirmPassword = confirmPasswordTxtBox.Password;

            // Make sure the passwords match 
            if (password != confirmPassword)
            {
                var output = string.Format("Passwords must match.");
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(output);
                await dialog.ShowAsync();
                return; 
            }

            // Create an AccountCreationObject out of the information 
            var accountCreator = new AccountCreationObject() { Email = emailAddress, Username = username, Password = password };

            // Serialize the AccountCreationObject into a JSON string 
            var accountCreatorJSON = JsonConvert.SerializeObject(accountCreator); 

            // Send JSON string to Archive API
            string signUpURL = "http://trout.wadec.com/api/signup";

            // Initiate HttpWebRequest with Archive API
            HttpWebRequest request = HttpWebRequest.CreateHttp(signUpURL);

            // Set the method to POST
            request.Method = "POST";

            // Add headers 
            request.Headers["X-ApiKey"] = "123456";
            request.Headers["X-AccessToken"] = "ix/S6We+A5GVOFRoEPdKxLquqOM= ";          // HARDCODED!

            // Set the ContentType property of the WebRequest
            request.ContentType = "application/json";

            // Create POST data and convert it to a byte array
            byte[] byteArray = Encoding.UTF8.GetBytes(accountCreatorJSON);

             // Create a stream request
            Stream dataStream = await request.GetRequestStreamAsync();

            // Write the data to the stream
            dataStream.Write(byteArray, 0, byteArray.Length);



            try
            {
                // Get response from URL
                response = await request.GetResponseAsync();

                using (responseStream = response.GetResponseStream())
                {
                    reader = new StreamReader(responseStream);

                    // Read a string of JSON into responseJSON
                    responseJSON = reader.ReadToEnd();

                    // Deserialize the JSON into a User object (using JSON.NET third party library)
                    responseUser = JsonConvert.DeserializeObject<User>(responseJSON);

                    // Get the VideoId
                    userID = responseUser.UserId;
                }
            }
            catch(Exception ex)
            {

            }

            string successOutput;
            if (userID > 0)
            {
                successOutput = string.Format("Account created successfully. Welcome to Archive " + responseUser.Username + ".");
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
    }
}
