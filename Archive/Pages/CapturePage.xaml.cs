using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI;
using Windows.UI.Popups; 
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
using Archive.DataModel;
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
        public StorageFile videoFile; 
        #endregion 

        #region Constructor
        public CapturePage()
        {
            this.InitializeComponent();

        }
        #endregion 

        #region 
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                // Using Windows.Media.Capture.CameraCaptureUI API to capture a photo
                CameraCaptureUI dialog = new CameraCaptureUI();
                dialog.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;

                StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Video);

                if (file != null)
                {
                    video_metadataPopup.IsOpen = true;
                    ShowMetaDataPopUp();  // Should I await here? 
                    videoFile = file;


                }
            }
            catch (Exception ex)
            {
            }
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
                    video_metadataPopup.IsOpen = true;
                    ShowMetaDataPopUp();
                    videoFile = file;


                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion 

        #region Submit video button click
        private async void submit_videoBtn_Click_1(object sender, RoutedEventArgs e)
        {
            video_metadataPopup.IsOpen = false;

            if (App.SynchronizeVideosToSkydrive)
            {
                #region Adjust some controls while the video is being uploaded
                uploadingPopUp.Visibility = Visibility.Visible;
                uploadingPopUp.IsOpen = true;
                backButton.Visibility = Visibility.Collapsed;
                ButtonsPanel.Visibility = Visibility.Collapsed;
                #endregion 

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
                    using (IRandomAccessStream fileStream = await videoFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (IOutputStream outStream = fileStream.GetOutputStreamAt(0))
                        {
                            DataWriter dw = new DataWriter(outStream);
                            await dw.StoreAsync();
                            dw.DetachStream();
                            await outStream.FlushAsync();
                        }
                    }

                    
                    try
                    {

                        SkyDriveFile skyDriveFile = await subFolder.UploadFileAsync(videoFile, videoFile.Name);
                    }
                    catch
                    {
                        ShowUploadErrorMessage();
                    }
                    

                    #region Upload complete, put the controls to normal 
                    uploadingPopUp.Visibility = Visibility.Collapsed;
                    uploadingPopUp.IsOpen = false;
                    backButton.Visibility = Visibility.Visible; ;
                    ButtonsPanel.Visibility = Visibility.Visible;
                    #endregion 

                    // Show progress ring here
                }
            }
            try
            {
                ShowUploadCompleteMessage();
                

                //var scopes = new string[] { "wl.signin", "wl.skydrive", "wl.skydrive_update" };
                //LiveAuthClient authClient = new LiveAuthClient();
                //LiveLoginResult result = await authClient.LoginAsync(scopes);

                //if (result.Status == LiveConnectSessionStatus.Connected)
                //{
                //    LiveConnectClient cxnClient = new LiveConnectClient(authClient.Session);
                //    SkyDriveFolder subFolder = null;
                //    // Get hold of the root folder from SkyDrive. 
                //    // NB: this does not traverse the network and get the full folder details.
                //    SkyDriveFolder root = new SkyDriveFolder(
                //      cxnClient, SkyDriveWellKnownFolder.Root);

                //    // This *does* traverse the network and get those details.
                //    await root.LoadAsync();
                //    try
                //    {
                //        subFolder = await root.GetFolderAsync("Archive");
                //    }
                //    catch { }

                //    if (subFolder != null)
                //    {
                        
                //        var file = await subFolder.GetFileAsync("video000.mp4");
                //        //CapturedVideo.SetSource(file as StorageFile, "video/mp4"); 
                        
                //    }
                //}
            }
            catch (HttpRequestException hre)
            {
            }
            catch (TaskCanceledException)
            {
            }
        }
        #endregion

        public async Task ShowMetaDataPopUp()
        {
            video_metadataPopup.IsOpen = true; 
        }

        private async void ShowUploadErrorMessage()
        {
            var errorMessage = string.Format("There was an error uploading your video.\nPlease use the upload button on the bottom of the screen to try again");
            Windows.UI.Popups.MessageDialog errorDialog = new Windows.UI.Popups.MessageDialog(errorMessage);
            await errorDialog.ShowAsync();
        }

        private async void ShowUploadCompleteMessage()
        {
            // This async call keeps causing a bug! 
            //var output = string.Format("Your video was sent successfully!\nView it online at momento.wadec.com");
            //output += "\nShare your video:\n\tTwitter\n\tFacebook\n\tYouTube";
            //Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(output);
            //await dialog.ShowAsync(); 
            //var task = dialog.ShowAsync().AsTask();
            //await task; 
        }

        private void discard_videoBtn_Click_1(object sender, RoutedEventArgs e)
        {
            video_metadataPopup.IsOpen = false;
            videoFile = null; 
        }
    }
}
