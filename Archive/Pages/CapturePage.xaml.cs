﻿using System;
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
using Windows.Media;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media.MediaProperties;        // ImageProperties
using Windows.Devices.Enumeration; 


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Archive
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class CapturePage : Archive.Common.LayoutAwarePage
    {

        // NOTE: Maybe instead of trying to extract frames from a video, I can simply 
        // capture an image of the person at the beginning of each video, save it temporarily, 
        // then upload it with the video metadata if the video gets uploaded. 
        #region Variable declarations
        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser()
        private Windows.Foundation.Collections.IPropertySet appSettings;
        private const String videoKey = "capturedVideo";
        private const String fileKey = "filePath";
        private const String usernameKey = "Username";
        private const String passwordKey = "Password";
        public BitmapImage videoImage;
        public static string filePath;
        HttpClient httpClient;
        public StorageFile videoFile;
        private Windows.Media.Capture.MediaCapture m_mediaCaptureMgr;
        private readonly String TEMP_PHOTO_FILE_NAME = "photoTmp.jpg";
        private bool m_bRotateVideoOnOrientationChange;
        private bool m_bReversePreviewRotation;
        private readonly String PHOTO_FILE_NAME = "photo.jpg";
        private DeviceInformationCollection m_devInfoCollection;
        string[] scopes = new string[] { "wl.signin", "wl.skydrive", "wl.skydrive_update" };
        #endregion 

        #region Constructor
        public CapturePage()
        {
            this.InitializeComponent();
            videoImage = new BitmapImage(); 
            

        }
        #endregion 

        #region OnNavigatedTo
        /// <summary>
        /// Instead of using the capture button on this page, 
        /// we're assuming that this page is only navigated to from 
        /// the main page, whereby the user would be expecting to record a video of themselves. 
        /// </summary>
        /// <param name="e"></param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                // Open up the camera 
                CameraCaptureUI dialog = new CameraCaptureUI();
                dialog.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;
                
                // Capture a video file 
                StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Video);

                // If the video file isn't null: 
                if (file != null)
                {
                    video_metadataPopup.IsOpen = true;
                    videoFile = file;
                }
            }
            catch (Exception ex)
            {
                // Do something here!!!
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
                // Do something here!!!
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

            #region Adjust some controls while the video is being uploaded
            uploadingPopUp.Visibility = Visibility.Visible;
            uploadingPopUp.IsOpen = true;
            backButton.Visibility = Visibility.Collapsed;
            ButtonsPanel.Visibility = Visibility.Collapsed;
            #endregion 

            #region Send out CreateVideo request to Archive API, which will return a new VideoId
            // Get VideoId from API first 
            var VideoUploadURI = "http://trout.wadec.com/API/createvideo";
            // Initiate HttpWebRequest with Archive API
            HttpWebRequest request = HttpWebRequest.CreateHttp(VideoUploadURI);

            // Serialize a simple UserId object and send it to the Archive API
            if (App.LoggedInUser == null)
            {
                var error_output = string.Format("You are not currently logged in.");
                Windows.UI.Popups.MessageDialog error_dialog = new Windows.UI.Popups.MessageDialog(error_output);
                await error_dialog.ShowAsync();
                return; 
            }
            string UserID_JSON = JsonConvert.SerializeObject(new { UserId = App.LoggedInUser.UserId });
            //string UserID_JSON = JsonConvert.SerializeObject(new { UserId = 1 });

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
            {
                // Do something here!!!
            }

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
                // Do something here!!!
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
            try
            {
                HttpContent response_content = client.PostAsync(address, form).Result.Content;
            }
            catch
            {
                // Do something here!!!
            }


            #endregion 

            // Get thumbnail of the video file 
            var thumb = await videoFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView, 1000, Windows.Storage.FileProperties.ThumbnailOptions.UseCurrentScale);
            
            var bmpimg = new BitmapImage();
            bmpimg.SetSource(thumb);
            videoImage.SetSource(thumb); 
            PreviewImage.Source = bmpimg;

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

     //           videoImage.UriSource = new Uri(new Uri(
     //Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" +
     //Windows.Storage.ApplicationData.Current.LocalFolder.Name),
     //"videoImage.jpg");
                
     //           Windows.Storage.StorageFile imageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(videoImage.UriSource);
     //           SkyDriveFile skyDriveFile = await subFolder.UploadFileAsync(imageFile, "videoImage.jpg", OverwriteOption.Overwrite);
            }
            UploadVideoToSkyDrive();
            //CaptureImage();
            //Extract_Image_From_Video(videoFile);

            #region Upload complete, put the controls to normal
            uploadingPopUp.Visibility = Visibility.Collapsed;
            uploadingPopUp.IsOpen = false;
            backButton.Visibility = Visibility.Visible; ;
            ButtonsPanel.Visibility = Visibility.Visible;
            #endregion 

            
            #region Show success message
            var output = string.Format("Your video was sent successfully!\nView it online at momento.wadec.com");
            output += "\nShare your video:\n\tTwitter\n\tFacebook\n\tYouTube";
            Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(output);
            await dialog.ShowAsync();
            #endregion 
        }
        #endregion

        #region GetCurrentPhotoRotation
        private Windows.Storage.FileProperties.PhotoOrientation GetCurrentPhotoRotation()
        {
            bool counterclockwiseRotation = m_bReversePreviewRotation;

            if (m_bRotateVideoOnOrientationChange)
            {
                return PhotoRotationLookup(Windows.Graphics.Display.DisplayProperties.CurrentOrientation, counterclockwiseRotation);
            }
            else
            {
                return Windows.Storage.FileProperties.PhotoOrientation.Normal;
            }
        }
        #endregion

        #region Re-encode Photo 
        private async Task<Windows.Storage.StorageFile> ReencodePhotoAsync(
            Windows.Storage.StorageFile tempStorageFile,
            Windows.Storage.FileProperties.PhotoOrientation photoRotation)
        {
            Windows.Storage.Streams.IRandomAccessStream inputStream = null;
            Windows.Storage.Streams.IRandomAccessStream outputStream = null;
            Windows.Storage.StorageFile photoStorage = null;

            try
            {
                inputStream = await tempStorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);

                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(inputStream);

                photoStorage = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync(PHOTO_FILE_NAME, Windows.Storage.CreationCollisionOption.GenerateUniqueName);

                outputStream = await photoStorage.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

                outputStream.Size = 0;

                var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                var properties = new Windows.Graphics.Imaging.BitmapPropertySet();
                properties.Add("System.Photo.Orientation",
                    new Windows.Graphics.Imaging.BitmapTypedValue(photoRotation, Windows.Foundation.PropertyType.UInt16));

                await encoder.BitmapProperties.SetPropertiesAsync(properties);

                await encoder.FlushAsync();
            }
            finally
            {
                if (inputStream != null)
                {
                    inputStream.Dispose();
                }

                if (outputStream != null)
                {
                    outputStream.Dispose();
                }

                var asyncAction = tempStorageFile.DeleteAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
            }

            return photoStorage;
        }
        #endregion

        #region Photo Rotation Lookup
        private Windows.Storage.FileProperties.PhotoOrientation PhotoRotationLookup(
            Windows.Graphics.Display.DisplayOrientations displayOrientation,
            bool counterclockwise)
        {
            switch (displayOrientation)
            {
                case Windows.Graphics.Display.DisplayOrientations.Landscape:
                    return Windows.Storage.FileProperties.PhotoOrientation.Normal;

                case Windows.Graphics.Display.DisplayOrientations.Portrait:
                    return (counterclockwise) ? Windows.Storage.FileProperties.PhotoOrientation.Rotate270 :
                        Windows.Storage.FileProperties.PhotoOrientation.Rotate90;

                case Windows.Graphics.Display.DisplayOrientations.LandscapeFlipped:
                    return Windows.Storage.FileProperties.PhotoOrientation.Rotate180;

                case Windows.Graphics.Display.DisplayOrientations.PortraitFlipped:
                    return (counterclockwise) ? Windows.Storage.FileProperties.PhotoOrientation.Rotate90 :
                        Windows.Storage.FileProperties.PhotoOrientation.Rotate270;

                default:
                    return Windows.Storage.FileProperties.PhotoOrientation.Unspecified;
            }
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

        #region Extract image from video 
        /// <summary>
        /// Extract the first frame of the video as a BitmapImage. 
        /// This image will be the face of the video.
        /// </summary>
        /// <param name="video_file"></param>
        /// <returns></returns>
        public async void Extract_Image_From_Video(StorageFile video_file)
        {
   
            BitmapImage image = new BitmapImage();

            // Open the video file as a stream
            IRandomAccessStream readStream = await video_file.OpenAsync(FileAccessMode.Read);
            // Breaks here 
            BitmapDecoder bmpDecoder = await BitmapDecoder.CreateAsync(readStream);
            BitmapFrame frame = await bmpDecoder.GetFrameAsync(0);
            BitmapTransform bmpTrans = new BitmapTransform();
            bmpTrans.InterpolationMode = BitmapInterpolationMode.Cubic;
            PixelDataProvider pixelDataProvider = await frame.GetPixelDataAsync(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, bmpTrans, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
            byte[] pixelData = pixelDataProvider.DetachPixelData();
            InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();
            BitmapEncoder enc = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, ras);
            // write the pixel data to our stream
            enc.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, 200, 200, bmpDecoder.DpiX, bmpDecoder.DpiY, pixelData);
            await enc.FlushAsync();

            // this is critical and below does not work without it!
            ras.Seek(0);

            // Set to the image
            BitmapImage gridImage = new BitmapImage();
            gridImage.SetSource(ras);
            PreviewImage.Source = gridImage;
        }
        #endregion 

        #region Upload video to SkyDrive
        public async void UploadVideoToSkyDrive()
        {

            #region Extract video metadata from metadata pop up
            string videoName = null;
            string videoDescription = descriptionTxtBox.Text;
            string tags = tagTxtBox.Text;
            DateTime dateCreated = DateTime.Now;

            if (titleTxtBox.Text != "")
                videoName = titleTxtBox.Text + ".mp4";
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


            }

        }
        #endregion

        #region Capture an image
        public async void CaptureImage()
        {
            try
            {
                //This is kind of a hack... 
                //Using Windows.Media.Capture.CameraCaptureUI API to capture a photo
                //CameraCaptureUI dialog = new CameraCaptureUI();
                //dialog.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;
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

                    // Get media capture settings
                    var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
                    m_mediaCaptureMgr = new Windows.Media.Capture.MediaCapture();

                    // Initialize media capture manager 
                    await m_mediaCaptureMgr.InitializeAsync(settings);

                    // Get the current rotation 
                    var currentRotation = GetCurrentPhotoRotation();

                    // Set the image properties to create a .jpeg image
                    var imageProperties = ImageEncodingProperties.CreateJpeg();

                    // Create a temporary storage file in the user's picture library 
                    var tempPhotoStorageFile = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync(TEMP_PHOTO_FILE_NAME, Windows.Storage.CreationCollisionOption.GenerateUniqueName);

                    await m_mediaCaptureMgr.CapturePhotoToStorageFileAsync(imageProperties, tempPhotoStorageFile);
                    var photoStorageFile = await ReencodePhotoAsync(tempPhotoStorageFile, currentRotation);
                    await subFolder.UploadFileAsync(photoStorageFile);
                    var photoStream = await photoStorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    var bmpimg = new BitmapImage();
                    bmpimg.SetSource(photoStream);
                    PreviewImage.Source = bmpimg;
                    await tempPhotoStorageFile.DeleteAsync();

                    // Set the videoImage variable on this page to contain the image! 

                }
            }
            catch
            {

            }
        }
        #endregion

        private async void addTagsBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
