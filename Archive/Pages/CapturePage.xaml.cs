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
using System.Net; 
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
using Archive.Common;
using Archive.Data;
using Archive.DataModel;
using Archive.API_Helpers;
using System.Text;  // Encoding 
using Newtonsoft.Json;


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

        #region OnNavigatedTo
        // Instead of using the capture button on this page, 
        // we're assuming that this page is only navigated to from 
        // the main page, whereby the user would be expecting to record a video of themselves. 
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
            #region Variable declarations
            WebResponse response;                   // Response from createvideo URL 
            Stream responseStream;                  // Stream data from responding URL
            StreamReader reader;                    // Read data in stream 
            string responseJSON;                    // The JSON string returned to us by the Archive API 
            CreateVideoResponse API_response;       // A simple object with only one attribute: VideoId 
            HttpClient httpClient = new HttpClient();
            HttpMessageHandler handler = new HttpClientHandler();
            int VideoId = -1;
            #endregion 

            // Close the metadata popup 
            video_metadataPopup.IsOpen = false;

            #region Send out CreateVideo request to Archive API, which will return a new VideoId
            // Get VideoId from API first 
            // Initiate HttpWebRequest with Archive API
            var VideoUploadURI = "http://trout.wadec.com/API/createvideo";  
            HttpWebRequest request = HttpWebRequest.CreateHttp(VideoUploadURI);


            string UserID_JSON = JsonConvert.SerializeObject(new { UserId = 24 });

            // Set the method to POST
            request.Method = "POST";

            // Add headers 
            request.Headers["X-ApiKey"] = "123456";
            request.Headers["X-AccessToken"] = "UqYONgdB/aCCtF855bp8CSxmuHo=";

            // Set the ContentType property of the WebRequest
            request.ContentType = "application/json";

            // Create POST data and convert it to a byte array
            byte[] byteArray = Encoding.UTF8.GetBytes(UserID_JSON);


            // Create a stream request
            Stream dataStream = await request.GetRequestStreamAsync();

            // Write the data to the stream
            dataStream.Write(byteArray, 0, byteArray.Length);



            try
            {
                // Get response from URL
                response = await request.GetResponseAsync();

                using (responseStream = response.GetResponseStream())
                {
                    reader = new StreamReader(responseStream);

                    // Read a string of JSON into responseJSON
                    responseJSON = reader.ReadToEnd();

                    // Deserialize the JSON into a User object (using JSON.NET third party library)
                    API_response = JsonConvert.DeserializeObject<CreateVideoResponse>(responseJSON);

                    // Get the VideoId
                    VideoId = API_response.VideoId;
                }

            }
            catch (Exception ex)
            { }

            #endregion 

            #region Extract video metadata from metadata pop up 
            string videoName = null;
            string videoDescription = descriptionTxtBox.Text;
            string tags = tagTxtBox.Text;
            DateTime dateCreated = DateTime.Now;

            if (titleTxtBox.Text != "")
                videoName = titleTxtBox.Text + ".mp4";
            #endregion 
           
            #region Send metadata 
            // Send metadata first 
            var VideoMetadataURI = "http://trout.wadec.com/API/uploadvideometadata";
            HttpWebRequest metadata_request = HttpWebRequest.CreateHttp(VideoMetadataURI);

            // Create a VideoMetadata object 
            VideoMetadata md = new VideoMetadata(VideoId, "Test name", videoDescription, "ACM HQ", dateCreated.ToUniversalTime());

            // Serialize the VideoMetadata object into JSON string
            string video_metadata_JSON = JsonConvert.SerializeObject(md);

            // Set the method to POST
            metadata_request.Method = "POST";

            // Add headers 
            metadata_request.Headers["X-ApiKey"] = "123456";
            metadata_request.Headers["X-AccessToken"] = "UqYONgdB/aCCtF855bp8CSxmuHo=";

            // Set the ContentType property of the WebRequest
            metadata_request.ContentType = "application/json";

            // Create POST data and convert it to a byte array
            byteArray = Encoding.UTF8.GetBytes(video_metadata_JSON);

            // Create a stream request
            dataStream = await metadata_request.GetRequestStreamAsync();

            // Write the data to the stream
            dataStream.Write(byteArray, 0, byteArray.Length);



            try
            {
                // Get response from URL
                response = await metadata_request.GetResponseAsync();

                using (responseStream = response.GetResponseStream())
                {
                    reader = new StreamReader(responseStream);

                    // Read a string of JSON into responseJSON
                    responseJSON = reader.ReadToEnd();



                    // Deserialize the JSON into a User object (using JSON.NET third party library)
                    API_response = JsonConvert.DeserializeObject<CreateVideoResponse>(responseJSON);
                }

            }
            catch (WebException ex)
            {
                using (responseStream = ex.Response.GetResponseStream())
                {

                }
            }
            #endregion 

            #region Upload video to Archive API
            HttpClient client = new HttpClient(); 
            MultipartFormDataContent form = new MultipartFormDataContent();
            StorageFile file = await StorageFile.GetFileFromPathAsync(videoFile.Path);
            var stream = await file.OpenReadAsync();
            StreamContent streamContent = new StreamContent(stream.AsStream(), 1024);
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            streamContent.Headers.ContentDisposition.Name = "\"file\"";
            streamContent.Headers.ContentDisposition.FileName = "\"" + Path.GetFileName(videoFile.Path) + "\"";
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            form.Add(new StringContent(VideoId.ToString()), "\"videoId\"");
            form.Add(streamContent, "file");

            string address = "http://trout.wadec.com/API/uploadvideofile"; 
            HttpContent response_content = client.PostAsync(address, form).Result.Content;


            #endregion 

            #region Upload video to SkyDrive
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

                        SkyDriveFile skyDriveFile = await subFolder.UploadFileAsync(videoFile, videoName == null ? videoName : videoFile.DateCreated.ToString(), OverwriteOption.Rename); 
                    }
                    catch
                    {
                        
                    }
                   

                    #region Upload complete, put the controls to normal 
                    uploadingPopUp.Visibility = Visibility.Collapsed;
                    uploadingPopUp.IsOpen = false;
                    backButton.Visibility = Visibility.Visible; ;
                    ButtonsPanel.Visibility = Visibility.Visible;
                    #endregion 
                }
            }
            #endregion 

            #region Show success message
            var output = string.Format("Your video was sent successfully!\nView it online at momento.wadec.com");
            output += "\nShare your video:\n\tTwitter\n\tFacebook\n\tYouTube";
            Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(output);
            await dialog.ShowAsync();
            #endregion 
        }
        #endregion

        #region ShowMetaDataPopUp
        public async Task ShowMetaDataPopUp()
        {
            video_metadataPopup.IsOpen = true; 
        }
        #endregion

        #region Show Upload Error Message
        private async void ShowUploadErrorMessage()
        {
            var errorMessage = string.Format("There was an error uploading your video.");
            Windows.UI.Popups.MessageDialog errorDialog = new Windows.UI.Popups.MessageDialog(errorMessage);
            await errorDialog.ShowAsync();
        }
        #endregion

        #region Cancel upload clicked
        private void cancelUploadButton_Click(object sender, RoutedEventArgs e)
        {
            video_metadataPopup.IsOpen = false;
            videoFile = null;
        }
        #endregion 
    }
}
