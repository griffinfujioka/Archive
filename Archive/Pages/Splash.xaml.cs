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
using Windows.ApplicationModel.Activation; 

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Archive.Pages
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Splash : Archive.Common.LayoutAwarePage
    {
        private Rect splashImageCoordinates; // Rect to store splash screen image coordinates.
        private SplashScreen splash; // Variable to hold the splash screen object.
        private static SearchActivatedEventArgs SearchArgs = null;

        public Splash()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Splash Screen for Basic Application launch
        /// </summary>
        /// <param name="splashScreen">SplashScreen from IActivatedEventArgs</param>
        public Splash(SplashScreen splashScreen)
        {
            Init(splashScreen);
        }

        /// <summary>
        /// Splash Screen for Search Application launch
        /// </summary>
        /// <param name="splashScreen">SplashScreen from IActivatedEventArgs</param>
        /// <param name="searchArgs">SearchActivatedEventArgs</param>
        public Splash(SplashScreen splashScreen, SearchActivatedEventArgs searchArgs)
        {
            SearchArgs = searchArgs;
            Init(splashScreen);
        }


        /// <summary>
        /// Splash Screen for Basic launch + Tile launch
        /// If args contains Note Unique Id as Argument then perform redirect to that specific note.
        /// </summary>
        /// <param name="splashScreen">SplashScreen from IActivatedEventArgs</param>
        /// <param name="args">LaunchActivatedEventArgs</param>
        public Splash(SplashScreen splashScreen, LaunchActivatedEventArgs args)
        {
            Init(splashScreen);
        }


        /// <summary>
        /// Place Splash Screen at the center of the page and register to the following events:
        /// DataTransferManager -> DataRequested
        /// SearchPane -> SuggestionsRequested, SearchPaneQuerySubmitted
        /// SettingsPane -> CommandsRequested
        /// NotesDataSource -> DataCompleted
        /// </summary>
        /// <param name="splashScreen">SplashScreen from IActivatedEventArgs</param>
        async private void Init(SplashScreen splashScreen)
        {
            //if (!SampleDataSource.DataLoaded)
            //{
            //    this.InitializeComponent();
            //    this.splashImageCoordinates = splashScreen.ImageLocation;
            //    this.splash = splashScreen;

            //    // Position the extended splash screen image in the same location as the splash screen image.
            //    this.loader.SetValue(Canvas.LeftProperty, this.splashImageCoordinates.X);
            //    this.loader.SetValue(Canvas.TopProperty, this.splashImageCoordinates.Y);
            //    this.loader.Height = this.splashImageCoordinates.Height;
            //    this.loader.Width = this.splashImageCoordinates.Width;

            //    DataTransferManager datatransferManager;
            //    datatransferManager = DataTransferManager.GetForCurrentView();
            //    datatransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.DataRequested);

            //    Window.Current.SizeChanged += new WindowSizeChangedEventHandler(ExtendedSplash_OnResize);
            //    SearchPane.GetForCurrentView().SuggestionsRequested += SearchPaneSuggestionsRequested;
            //    SearchPane.GetForCurrentView().QuerySubmitted += SearchPaneQuerySubmitted;

            //    SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;

            //    NotesDataSource data = new NotesDataSource();
            //    data.Completed += Data_Completed;
            //    await data.Load();
            //}
            //else
            //{
            //    Data_Completed(this, null);
            //}
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
    }
}
