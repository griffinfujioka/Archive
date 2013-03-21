using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    public sealed partial class StreamVideoPage : Archive.Common.LayoutAwarePage
    {
        public static int streamVideoID;
        public const string videoStreamURL = "http://trout.wadec.com/api/videos/view?videoId="; 
        public StreamVideoPage()
        {
            this.InitializeComponent();
            var bounds = Window.Current.Bounds;
            var height = bounds.Height;
            var width = bounds.Width-140;
            MainWebView.Height = height;
            MainWebView.Width = width;
            Loaded += OnLoaded;
            


        }


        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            HttpClient http = new HttpClient();
            HttpResponseMessage response = await http.GetAsync(videoStreamURL + streamVideoID.ToString());
            
            var content = await response.Content.ReadAsStringAsync();
            var desContent = JsonConvert.DeserializeObject<string>(content);
            MainWebView.NavigateToString(desContent);
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Get the videoID of the video you want to stream 
            streamVideoID = Convert.ToInt32(e.Parameter);
            base.OnNavigatedTo(e);
        }
    }
}
