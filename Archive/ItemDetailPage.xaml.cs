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
using Archive.Pages;
using System.Net.Http;
using Newtonsoft.Json;      // VideoModel

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
        public const string videoStreamURL = "http://trout.wadec.com/api/videos/view?videoId=";
        public List<string> tagsList; 

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

                if (selectedVideoID >= 0)
                {
                    HttpClient http = new HttpClient();
                    HttpResponseMessage response = await http.GetAsync(videoStreamURL + selectedVideoID.ToString());

                    var content = await response.Content.ReadAsStringAsync();
                    var desContent = JsonConvert.DeserializeObject<string>(content);
                   
                    //MainWebView.NavigateToString(desContent);
                }

                // TODO: Create an appropriate data model for your problem domain to replace the sample data
                //var item = SampleDataSource.GetItem((String)navigationParameter);
                //this.pageTitle.Text = (App.ArchiveVideos.GetVideo(Convert.ToInt32(navigationParameter))).Title; 
                var item = VideosDataSource.GetVideo(Convert.ToInt32(navigationParameter.ToString()));
                this.DefaultViewModel["Group"] = "AllVideosGroup";      // HARDCODE!
                this.DefaultViewModel["Items"] = App.ArchiveVideos.AllVideosGroup.Items;


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
            tagsListBox.Items.Clear(); 
            // HACK - Come back and fix this shit
            if (this.flipView.SelectedItem == null)
                return; 

            // There is a bug which should be addressed here, but I'm not sure how to solve it. 
            // In the case that the flip view is changed, the video should stop playing. 
            
            var selectedItem = (VideoModel)this.flipView.SelectedItem;
            
            selectedVideoID = selectedItem.VideoId;
            pageTitle.Text = selectedItem.Title; 
            

            //tagsListBox.ItemsSource = selectedItem.Tags;
            foreach (string tag in selectedItem.Tags)
            {
                var lbi = new ListBoxItem();
                lbi.Content = "#" + tag;
                tagsListBox.Items.Add(lbi); 
            }

            try
            {
                var profileRequest = new ApiRequest("user/profile");
                profileRequest.Authenticated = true;
                profileRequest.Parameters.Add("userId", selectedItem.User.UserId.ToString());
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

        private void MediaPlayer_IsFullScreenChanged_1(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            var bounds = Window.Current.Bounds;
            var height = bounds.Height;
            var width = bounds.Width;


            (sender as Microsoft.PlayerFramework.MediaPlayer).IsFullScreen = true; 

        }

        private void playerCanvas_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
    
        }

        private void editBtn_Click_1(object sender, RoutedEventArgs e)
        {
            var selectedItem = (VideoModel)this.flipView.SelectedItem;
            titleTxtBox.Text = selectedItem.Title;
            descriptionTxtBox.Text = selectedItem.Description;
            var completeTagsString = "";
            foreach (string tag in selectedItem.Tags)
            {
                completeTagsString += "#" + tag + " "; 
            }
            tagsTxtBlock.Text = completeTagsString;
            locationTxtBlock.Text = selectedItem.Location;

            if (selectedItem.IsPublic)
                privacyComboBox.SelectedIndex = 1; 

            video_metadataPopup.IsOpen = true; 
        }

        private void tagTxtBox_KeyUp_1(object sender, KeyRoutedEventArgs e)
        {

        }

        private void addTagBtn_Click_1(object sender, RoutedEventArgs e)
        {
            tagsList.Add(tagTxtBox.Text);
            tagsTxtBlock.Text += "#" + tagTxtBox.Text + " ";
            tagTxtBox.Text = "";
            tagTxtBox.Focus(Windows.UI.Xaml.FocusState.Keyboard); 
        }

        private void privacyComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void submit_videoBtn_Click_1(object sender, RoutedEventArgs e)
        {
            // Send the new info up to the server


            video_metadataPopup.IsOpen = false; 
        }

        private void cancelUploadButton_Click_1(object sender, RoutedEventArgs e)
        {
            video_metadataPopup.IsOpen = false; 
        }

        
    }
}
