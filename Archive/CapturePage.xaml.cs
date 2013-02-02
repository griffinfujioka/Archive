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
using System.Net.Http;          /* For http handlers */
using System.Net.Http.Headers;  /* For ProductInfoHeaderValue class */
using Windows.Storage.Streams;  /* Used to store a video stream to a file */
using System.Threading.Tasks;   /* Tasks */
using Windows.Storage;
using Windows.Media.Capture;    /* Camera */ 
using Microsoft.Live; 
using SkyDriveHelper;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers; 
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Archive
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class CapturePage : Archive.Common.LayoutAwarePage
    {

        #region Variable declarations
        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser()
        private Windows.Foundation.Collections.IPropertySet appSettings;
        private const String videoKey = "capturedVideo";
        private const String fileKey = "filePath";
        private const String usernameKey = "Username";
        private const String passwordKey = "Password";
        public static string filePath;
        HttpClient httpClient;
        #endregion 

        #region Constructor
        public CapturePage()
        {
            this.InitializeComponent();
        }
        #endregion 



        #region Upload button click
        private async void uploadBtn_Click_1(object sender, RoutedEventArgs e)
        {
            /* video_metadataPopup.IsOpen = true;
             while (video_metadataPopup.IsOpen) ; */
            titleTxtBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);

        }
        #endregion

        #region Discard button click
        private async void discardButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (appSettings.ContainsKey(videoKey))
            {
                appSettings.Remove(videoKey);
                CapturedVideo.Source = null;
            }
            else
            {
                //Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("There is no video file to discard.");
                //await dialog.ShowAsync();
            }
        }
        #endregion 

        #region ReloadVideo
        /// <summary>
        /// Loads the video from file path
        /// </summary>
        /// <param name="filePath">The path to load the video from</param>
        private async Task ReloadVideo(String filePath)
        {
            try
            {
                
                StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
                var stream = await file.OpenReadAsync();
                StreamContent streamContent = new StreamContent(stream.AsStream(), 1024);
                //form.Add(streamContent, "video", file.Path);
                IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                // TODO: Figure out how to resume a paused video
                CapturedVideo.SetSource(fileStream, "video/mp4");

            }
            catch (Exception ex)
            {
                //appSettings.Remove(videoKey);
            }
        }
        #endregion

        #region Play button click
        private async void playBtn_Click_1(object sender, RoutedEventArgs e)
        {

            if (appSettings.ContainsKey(videoKey))
            {
                object filePath;
                if (appSettings.TryGetValue(videoKey, out filePath) && filePath.ToString() != "")
                {

                    await ReloadVideo(filePath.ToString());

                }
            }
            else
            {
                //Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("There is no video file to play.");
                //await dialog.ShowAsync();
            }

        }
        #endregion 

        #region Stop button click
        private void stopBtn_Click_1(object sender, RoutedEventArgs e)
        {
            CapturedVideo.Stop();
        }
        #endregion 

        #region Capture Button click
        private async void Capture_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Using Windows.Media.Capture.CameraCaptureUI API to capture a photo
                CameraCaptureUI dialog = new CameraCaptureUI();
                dialog.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;

                StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Video);

                if (file != null)
                {
                    if (App.SynchronizeVideosToSkydrive)
                    {
                        var scopes = new string[] { "wl.signin", "wl.skydrive", "wl.skydrive_update" };

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

                            SkyDriveFolder subFolder = null;

                            try
                            {
                                subFolder = await root.GetFolderAsync("Archive");
                            }
                            catch { }


                            if (subFolder == null)
                                subFolder = await root.CreateFolderAsync("Archive");


                            //StorageFile newVideo = await ApplicationData.Current.LocalFolder.CreateFileAsync(file.Name); 
                            using(IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                using(IOutputStream outStream = fileStream.GetOutputStreamAt(0))
                                {
                                        DataWriter dw = new DataWriter(outStream);
                                        await dw.StoreAsync();
                                        dw.DetachStream(); 
                                        await outStream.FlushAsync(); 
                                }
                            }


                            SkyDriveFile skyDriveFile = await subFolder.UploadFileAsync(file, file.Name);
                            //var stream = await file.OpenAsync(FileAccessMode.Read);

                            //using (var outputStream = stream.GetOutputStreamAt(0))
                            //{
                            //    DataWriter dw = new DataWriter(outputStream);
                            //    await dw.StoreAsync();
                            //    await outputStream.FlushAsync();

                            //}

                            //FileOpenPicker picker = new FileOpenPicker();
                            //picker.FileTypeFilter.Add(".jpg");
                            //StorageFile file1 = await picker.PickSingleFileAsync();

                            //SkyDriveFile skyDriveFile1 = await subFolder.UploadFileAsync(file1);

                        }
                    }
                    //IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                    //CapturedVideo.SetSource(fileStream, "video/mp4");

                    //// Store the file path in Application Data
                    //// Each time you Capture a video file.Path is a different, randomly generated path. 
                    //appSettings[videoKey] = file.Path;
                    //appSettings[fileKey] = file.Path;
                    //filePath = file.Path;       // Set the global variable so when you record a video, that's that video that will send 


                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion 

    }
}
