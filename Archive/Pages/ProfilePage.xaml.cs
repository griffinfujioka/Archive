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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Profile responseProfile;          
       
            // Get the videoID of the video you want to stream 
            userID = Convert.ToInt32(e.Parameter);


            try
            {
                var profileRequest = new ApiRequest("user/profile");
                profileRequest.Authenticated = true;
                profileRequest.Parameters.Add("userId", userID.ToString()); 
                responseProfile = await profileRequest.ExecuteAsync<Profile>();

                DateTimeFormatter dtFormatter = new DateTimeFormatter("shortdate");
                var signedUpDateShort = dtFormatter.Format(responseProfile.User.Created);

                profilePicture.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(responseProfile.User.Avatar, UriKind.Absolute));
                usernameTxtBlock.Text = responseProfile.User.Username;
                emailTxtBlock.Text = responseProfile.User.Email;
                dateJoinedTxtBlock.Text = "Date joined: " + signedUpDateShort;
                numberOfVideosTxtBox.Text = responseProfile.Videos.Count.ToString();
                numberOfFollowersTxtBox.Text = responseProfile.Followers.Count.ToString();
                numberOfFollowingTxtBox.Text = responseProfile.Following.Count.ToString();

                videosGridView.ItemsSource = responseProfile.Videos;
                followersGridView.ItemsSource = responseProfile.Followers;
                followingGridView.ItemsSource = responseProfile.Following;

            }
            catch(Exception ex)
            {
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

        private void followButton_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
