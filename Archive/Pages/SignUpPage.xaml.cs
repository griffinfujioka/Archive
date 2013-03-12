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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Archive.Pages
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SignUpPage : Archive.Common.LayoutAwarePage
    {
        public SignUpPage()
        {
            this.InitializeComponent();
            emailAddressTxtBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
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
            var accountCreator = new AccountCreationObject() { EmailAddress = emailAddress, Username = username, Password = password };

            // Serialize the AccountCreationObject into a JSON string 
            var JSONaccountCreator = JsonConvert.SerializeObject(accountCreator); 

            // Send JSON string to Archive API
            
           
        }
    }
}
