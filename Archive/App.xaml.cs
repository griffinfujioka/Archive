using Archive.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;
using Windows.UI;
using Callisto.Controls;
using Archive.Pages;
using Microsoft.Live;
using SkyDriveHelper;
using Archive.DataModel;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using Windows.Networking.Connectivity;      // Check for internet connectivity 
using Archive.Common;
using Archive.API_Helpers; 

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace Archive
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        public static bool SynchronizeVideosToSkydrive = true;
        public static bool API_Authenticated = false;
        public static bool HasNetworkConnection = false;

        private static User _loggedinuser;      // Maintain information about a User if one is logged in 
        public static User LoggedInUser
        {
            get { return _loggedinuser; }
            set { _loggedinuser = value; }
        }
        

        private static ObservableCollection<VideoDataCommon> _skydriveVideos;
        public static ObservableCollection<VideoDataCommon> SkyDriveVideos
        {
            get { return _skydriveVideos; }
            set { _skydriveVideos = value; }
        }
 

        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;



            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }



                //var VideosDataSource = new VideosDataSource();
                //await VideosDataSource.Load(); 
                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(GroupedItemsPage), "AllGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
                //SplashScreen splashScreen = args.SplashScreen;
                //Splash eSplash = new Splash(splashScreen, args);
                //Window.Current.Content = eSplash;
            }
            // Ensure the current window is active
            Window.Current.Activate();

            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            var interfaceType = connectionProfile.NetworkAdapter.IanaInterfaceType;
            // 71 is WiFi & 6 is Ethernet (Ethernet is throwing a false-positive)
            if (interfaceType == 71 || interfaceType == 6)
            {
                App.HasNetworkConnection = true;
            }
            else
            {
                App.HasNetworkConnection = false; 
            }


            // Settings
            SettingsPane.GetForCurrentView().CommandsRequested += Settings_CommandsRequested;

            #region Synchronize with Skydrive
            if (SynchronizeVideosToSkydrive)
            {
                var scopes = new string[] { "wl.signin", "wl.skydrive", "wl.skydrive_update" };

                if (App.HasNetworkConnection)
                {
                    LiveAuthClient authClient = new LiveAuthClient();
                    LiveLoginResult result = await authClient.LoginAsync(scopes);

                    if (result.Status == LiveConnectSessionStatus.Connected)
                    {
                        LiveConnectClient cxnClient = new LiveConnectClient(authClient.Session);

                        // Get hold of the root folder from SkyDrive. 
                        // NB: this does not traverse the network and get the full folder details.
                        SkyDriveFolder root = new SkyDriveFolder(
                          cxnClient, SkyDriveWellKnownFolder.Root);

                        // This *does* traverse the network and get those details.
                        await root.LoadAsync();

                        string id = root.Name;
                        string desc = root.Description;
                        DateTimeOffset update = root.UpdatedTime;
                        uint count = root.Count;
                        Uri linkLocation = root.LinkLocation;
                        Uri uploadLocation = root.UploadLocation;
                        SkyDriveFolder subFolder = null;

                        try
                        {
                            subFolder = await root.GetFolderAsync("Archive");
                        }
                        catch { }


                        if (subFolder == null)
                            subFolder = await root.CreateFolderAsync("Archive");

                        VideosDataSource data = new VideosDataSource();
                        //data.Completed += Data_Completed;
                        await data.Load();

                    }
                }


            }
            #endregion

        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        /// <summary>
        /// Define Settings Pages for the application once the OnCommandsRequested event is raised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Settings_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            Color _background = Color.FromArgb(255, 178, 34, 34);

            // Add an About command
            var About = new SettingsCommand("About", "About", (handler) =>
            {
                var settings = new SettingsFlyout();
                settings.Content = new AboutPage();
                settings.HeaderBrush = new SolidColorBrush(_background);
                settings.Background = new SolidColorBrush(_background);
                settings.HeaderText = "About";
                settings.IsOpen = true;
            });

            var Settings = new SettingsCommand("Settings", "Settings", (handler) =>
            {
                var settings = new SettingsFlyout();
                settings.Content = new SettingsPage();
                settings.HeaderBrush = new SolidColorBrush(_background);
                settings.Background = new SolidColorBrush(_background);
                settings.HeaderText = "Settings";
                settings.IsOpen = true;
                settings.ContentBackgroundBrush = new SolidColorBrush(_background); 
            });



            args.Request.ApplicationCommands.Add(About);
            args.Request.ApplicationCommands.Add(Settings);
        }
    }
}
