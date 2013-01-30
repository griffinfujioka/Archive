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
using Microsoft.Live;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Archive.Pages
{
    public sealed partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void SyncSkydriveToggle_Toggled_1(object sender, RoutedEventArgs e)
        {
            if (SyncSkydriveToggle.IsOn)
            {
                var scopes = new string[] { "wl.signin", "wl.skydrive", "wl.skydrive_update" };

                LiveAuthClient authClient = new LiveAuthClient();
                LiveLoginResult result = await authClient.LoginAsync(scopes);

                if (result.Status == LiveConnectSessionStatus.Connected)
                {

                }
                // Create folder in Skydrive if it doesn't exist 
                // Save all videos into Skydrive folder 
            }
        }

        private void SignIntoSkydriveButton_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
