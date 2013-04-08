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
using Archive.DataModel;
using Archive.API_Helpers;
using Archive.Pages;      // VideoModel

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace Archive
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class ItemDetailPage : Archive.Common.LayoutAwarePage
    {
        int selectedVideoID = -1; 
        public ItemDetailPage()
        {
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
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            

            try
            {
                // Allow saved page state to override the initial item to display
                if (pageState != null && pageState.ContainsKey("SelectedItem"))
                {
                    navigationParameter = pageState["SelectedItem"];
                }
                // TODO: Create an appropriate data model for your problem domain to replace the sample data
                //var item = SampleDataSource.GetItem((String)navigationParameter);
                //this.pageTitle.Text = (App.ArchiveVideos.GetVideo(Convert.ToInt32(navigationParameter))).Title; 
                var item = VideosDataSource.GetVideo(Convert.ToInt32(navigationParameter.ToString()));
                this.DefaultViewModel["Group"] = "AllVideosGroup";      // HARDCODE!
                this.DefaultViewModel["Items"] = App.ArchiveVideos.AllVideosGroup.Items;

                var profileRequest = new ApiRequest("user/profile");
                profileRequest.Authenticated = true;
                profileRequest.Parameters.Add("userId", item.UserId.ToString());
                profileRequest.Parameters.Add("currentUserId", App.LoggedInUser.UserId.ToString()); 
                Profile responseProfile = await profileRequest.ExecuteAsync<Profile>();
                this.authorDisplayControl.DataContext = responseProfile.User;


                this.flipView.SelectedItem = item;
                selectedVideoID = item.VideoId;
                
            }
            catch
            {
                this.Frame.Navigate(typeof(GroupedItemsPage)); 
            }
            

        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            var selectedItem = (VideoModel)this.flipView.SelectedItem;
            pageState["SelectedItem"] = selectedItem.VideoId;
        }

        private void playBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedVideoID >= 0)
            {
                this.Frame.Navigate(typeof(StreamVideoPage), selectedVideoID);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void flipView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (VideoModel)this.flipView.SelectedItem;
            selectedVideoID = selectedItem.VideoId;

            try
            {
                var profileRequest = new ApiRequest("user/profile");
                profileRequest.Authenticated = true;
                profileRequest.Parameters.Add("userId", selectedItem.UserId.ToString());
                profileRequest.Parameters.Add("currentUserId", App.LoggedInUser.UserId.ToString());
                Profile responseProfile = await profileRequest.ExecuteAsync<Profile>();
                this.authorDisplayControl.DataContext = responseProfile.User;
            }
            catch (Exception ex)
            {

            }
        }

        private void authorDisplayControl_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ProfilePage), (authorDisplayControl.DataContext as User).UserId);
        }

        
    }
}
