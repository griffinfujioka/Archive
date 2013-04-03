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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Archive.Pages
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class FindFriends : Archive.Common.LayoutAwarePage
    {
        public IList<User> results; 
        public FindFriends()
        {
            this.InitializeComponent();
            results = new List<User>(); 
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

        private async void searchBtn_Click_1(object sender, RoutedEventArgs e)
        {
            try
                {
                    var searchRequest = new ApiRequest("user/search");
                    searchRequest.Authenticated = true;
                    searchRequest.Parameters.Add("search", queryTxtBox.Text);
                    results = await searchRequest.ExecuteAsync<IList<User>>();
                    itemGridView.ItemsSource = results;
                    itemListView.ItemsSource = results;
                }
                catch (Exception ex)
                {

                }
        }

        private void queryTxtBox_KeyUp_1(object sender, KeyRoutedEventArgs e)
        {
            
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                searchBtn_Click_1(sender, e);
            }

        }

        private void itemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(ProfilePage), (e.ClickedItem as User).UserId);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                queryTxtBox.Text = e.Parameter.ToString(); 
                try
                {
                    var searchRequest = new ApiRequest("user/search");
                    searchRequest.Authenticated = true;
                    searchRequest.Parameters.Add("search", queryTxtBox.Text);
                    results = await searchRequest.ExecuteAsync<IList<User>>();
                    itemGridView.ItemsSource = results;
                    itemListView.ItemsSource = results;
                }
                catch (Exception ex)
                {

                }
            }
            base.OnNavigatedTo(e);
        }
    }
}
