using Archive.API_Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization.DateTimeFormatting;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Archive.Pages
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ProfilePage : Archive.Common.LayoutAwarePage
    {
        public int userID; 

        public ProfilePage()
        {
            if (App.LoggedInUser == null)
                return;

            this.InitializeComponent();
            
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

        private void followBtn_Click_1(object sender, RoutedEventArgs e)
        {

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            WebResponse response;                   // Response from createvideo URL 
            Stream responseStream;                  // Stream data from responding URL
            StreamReader reader;                    // Read data in stream 
            string responseJSON;                    // The JSON string returned to us by the Archive API 
            Profile responseProfile;                 
            // Get the videoID of the video you want to stream 
            userID = Convert.ToInt32(e.Parameter);

            string profileURL = "http://trout.wadec.com/api/user/profile?userId=" + userID.ToString();

            // Initiate HttpWebRequest with Archive API
            HttpWebRequest request = HttpWebRequest.CreateHttp(profileURL);

            // Set the method to POST
            request.Method = "GET";

            // Add headers 
            request.Headers["X-ApiKey"] = "123456";
            request.Headers["X-AccessToken"] = "ix/S6We+A5GVOFRoEPdKxLquqOM= ";          // HARDCODED!

            // Set the ContentType property of the WebRequest
            request.ContentType = "application/json";

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
                    responseProfile = JsonConvert.DeserializeObject<Profile>(responseJSON);

                    DateTimeFormatter dtFormatter = new DateTimeFormatter("shortdate");
                    var signedUpDateShort = dtFormatter.Format(responseProfile.User.Created);

                    profilePicture.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(responseProfile.User.Avatar, UriKind.Absolute));
                    usernameTxtBlock.Text = responseProfile.User.Username;
                    emailTxtBlock.Text = responseProfile.User.Email;
                    dateJoinedTxtBlock.Text = "Date joined: " + signedUpDateShort;
                    numberOfVideosTxtBox.Text = responseProfile.Videos.Count.ToString();
                    numberOfFollowersTxtBox.Text = responseProfile.Followers.Count.ToString();
                    numberOfFollowingTxtBox.Text = responseProfile.Following.Count.ToString();


                  

                    foreach (VideoModel video in responseProfile.Videos)
                    {
                        // Convert string from API (i.e., Upload/25.jpg) to url path (i.e., http://trout.wadec.com/upload/videoimage/25.jpg)
                        var imageURLfromAPI = video.VideoImage;
                        video.VideoImage = "http://trout.wadec.com/" + imageURLfromAPI;
                    }

                    videosGridView.ItemsSource = responseProfile.Videos;
                    followersGridView.ItemsSource = responseProfile.Followers;
                    followingGridView.ItemsSource = responseProfile.Following;
                }
            }
            catch (Exception ex)
            {
                // Do something here!!
            }
            base.OnNavigatedTo(e);
        }

        private void videosGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(ItemDetailPage), (e.ClickedItem as VideoModel).VideoId); 
        }

        private void followersGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(ProfilePage), (e.ClickedItem as User).UserId); 
        }

        private void followingGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(ProfilePage), (e.ClickedItem as User).UserId);
        }
    }
}
