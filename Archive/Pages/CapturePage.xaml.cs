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
using Windows.Media;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media.MediaProperties;        // ImageProperties
using Windows.Devices.Enumeration;
using Windows.Storage.FileProperties;
using System.Threading;
using Windows.Devices.Geolocation;
using Bing.Maps;
using Windows.UI.Notifications;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Archive
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class CapturePage : Archive.Common.LayoutAwarePage
    {


        #region Variable declarations
        private const String videoKey = "capturedVideo";        
        private const String fileKey = "filePath";
        private const String usernameKey = "Username";  
        private const String passwordKey = "Password";
        public BitmapImage videoImage;
        public static string filePath;
        public StorageFile videoFile;
        private readonly String PHOTO_FILE_NAME = "Archive Temp Photo.jpg";
        string[] scopes = new string[] { "wl.signin", "wl.skydrive", "wl.skydrive_update" };
        LiveAuthClient authClient;
        LiveConnectClient cxnClient;
        SkyDriveFolder root;                        // Root of SkyDrive directory 
        SkyDriveFolder archiveSkyDriveFolder;       // Archive folder in SkyDrive directory 
        private bool Is_SkyDrive_Enabled; 
        int VideoId;

        public int numberOfTags;
        public string[] tags;
        public string location_string;
        public List<string> tagsList; 
        #endregion 

        #region Constructor
        /// <summary>
        ///  Initialize some required variables. 
        /// </summary>
        public CapturePage()
        {
            this.InitializeComponent();
            videoImage = new BitmapImage();
            VideoId = -1;
            authClient = new LiveAuthClient();
            Is_SkyDrive_Enabled = false;
            Loaded += OnLoaded;             // Subscribe to the page Loaded event, run code in OnLoaded 
            privacyComboBox.SelectedIndex = 0;
            numberOfTags = 0;
            tagsList = new List<string>(); 

        }
        #endregion 

        #region OnNavigatedTo
        /// <summary>
        /// Instead of using the capture button on this page, 
        /// we're assuming that this page is only navigated to from 
        /// the main page, whereby the user would be expecting to record a video of themselves. 
        /// 
        /// 
        /// Open camera dialog so user is immediately able to record video. 
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

        #region OnLoaded 
        /// <summary>
        /// This function takes care of a number of things including: 
        ///  - Check if we've been given SkyDrive access
        ///  - If yes, setup Archive folder for storing videos 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // See if we can connect to SkyDrive 
            // If the user has given us permission to use SkyDrive... 
            //LiveLoginResult result = await authClient.LoginAsync(scopes);

            //if (result.Status == LiveConnectSessionStatus.Connected)
            //{
            //    Is_SkyDrive_Enabled = true;
            //    cxnClient = new LiveConnectClient(authClient.Session);

            //    // Get hold of the root folder from SkyDrive. 
            //    // NB: this does not traverse the network and get the full folder details.
            //    root = new SkyDriveFolder(
            //      cxnClient, SkyDriveWellKnownFolder.Root);

            //    // This *does* traverse the network and get those details.
            //    await root.LoadAsync();


            //    // Try to open the Archive folder 
            //    try
            //    {
            //        archiveSkyDriveFolder = await root.GetFolderAsync("Archive");
            //    }
            //    catch { }


            //    if (archiveSkyDriveFolder == null)
            //        archiveSkyDriveFolder = await root.CreateFolderAsync("Archive");
            //}


            location_string = "";
            try
            {
                string bing_maps_key = "AsU97otKt6mDgr4kQR8HxTUiHzzzxy08NBR1iLqssnnzllYMxT4zQQ84J5Rbr9fh";
                Geolocator gl = new Geolocator();
                gl.PositionChanged += (s, args) => { /* empty */ };

                Geoposition gp = await gl.GetGeopositionAsync();
                var latitude = gp.Coordinate.Latitude;
                var longitude = gp.Coordinate.Longitude;
                var helper = new MapHelper(bing_maps_key);
                var location = await helper.FindLocationByPointAsync(latitude, longitude);
                var my_address = location.First().address;

                location_string = string.Format("{0}, {1}",
                   my_address.locality, my_address.adminDistrict);

                locationTxtBlock.Text = location_string;
            }
            catch (Exception ex)
            {
                // Do something here... 
            }



        }
        #endregion 

        #region Discard button click
        private async void discardButton_Click_1(object sender, RoutedEventArgs e)
        {
           
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

                    #region Get current location and reverse geocode coordinates into city name
                //    string bing_maps_key = "AsU97otKt6mDgr4kQR8HxTUiHzzzxy08NBR1iLqssnnzllYMxT4zQQ84J5Rbr9fh";
                //    Geolocator gl = new Geolocator();
                //    gl.PositionChanged += (s, args) => { /* empty */ };

                //    Geoposition gp = await gl.GetGeopositionAsync();
                //    var latitude = gp.Coordinate.Latitude;
                //    var longitude = gp.Coordinate.Longitude;
                //    var helper = new MapHelper(bing_maps_key);
                //    var location = await helper.FindLocationByPointAsync(latitude, longitude);
                //    var address = location.First().address;

                //    var location_string = string.Format("{0}, {1}",
                //address.locality, address.adminDistrict);
                //    locationTxtBlock.Text = location_string;
                    // Here I've got the coordinates, but can't figure out the city name


                    #endregion 

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
            int SavedVideoId = -1;
            VideoMetadata md = null;
            #endregion 

            this.Frame.Navigate(typeof(GroupedItemsPage)); 


            #region Adjust some controls while the video is being uploaded
            //// Close the metadata popup 
            video_metadataPopup.IsOpen = false;
            //uploadingPopUp.Visibility = Visibility.Visible;
            //uploadingPopUp.IsOpen = true;
            //backButton.Visibility = Visibility.Collapsed;
            //ButtonsPanel.Visibility = Visibility.Collapsed;
            #endregion 

            #region Make sure the user is logged in
            // Serialize a simple UserId object and send it to the Archive API
            if (App.LoggedInUser == null)
            {
                var error_output = string.Format("You are not currently logged in.");
                Windows.UI.Popups.MessageDialog error_dialog = new Windows.UI.Popups.MessageDialog(error_output);
                await error_dialog.ShowAsync();
                return;
            }
            #endregion

            #region Upload video
            string UserID_JSON = JsonConvert.SerializeObject(new { UserId = App.LoggedInUser.UserId });

            progressTxtBlock.Text = "Uploading video..."; 

            try
            {

                var createVideoRequest = new ApiRequest("createvideo");
                createVideoRequest.Authenticated = true;
                await createVideoRequest.AddJsonContentAsync(new { UserId = App.LoggedInUser.UserId });
                var videoId = await createVideoRequest.ExecuteAsync<VideoIdModel>();
                SavedVideoId = videoId.VideoId;

                var videoUpload = new ApiChunkedVideoUpload(SavedVideoId, videoFile.Path);
                await videoUpload.Execute();

                var isVideoComplete = new ApiRequest("video/uploadchunked/iscomplete");
                isVideoComplete.Parameters.Add("videoId", SavedVideoId.ToString());
                var array = await isVideoComplete.ExecuteAsync(); 

                
            }
            catch (ApiException ex)
            {
                // api returned something other than 200
            }
            catch (Exception ex)
            {
                // something bad happened, hopefully not api related
            }
            #endregion

            #region Get current location and reverse geocode coordinates into city name
            progressTxtBlock.Text = "Getting current location...";
            //string location_string = "";
            //try
            //{
            //    string bing_maps_key = "AsU97otKt6mDgr4kQR8HxTUiHzzzxy08NBR1iLqssnnzllYMxT4zQQ84J5Rbr9fh";
            //    Geolocator gl = new Geolocator();
            //    gl.PositionChanged += (s, args) => { /* empty */ };

            //    Geoposition gp = await gl.GetGeopositionAsync();
            //    var latitude = gp.Coordinate.Latitude;
            //    var longitude = gp.Coordinate.Longitude;
            //    var helper = new MapHelper(bing_maps_key);
            //    var location = await helper.FindLocationByPointAsync(latitude, longitude);
            //    var my_address = location.First().address;

            //    location_string = string.Format("{0}, {1}",
            //       my_address.locality, my_address.adminDistrict);
            //}
            //catch (Exception ex)
            //{
            //    // Do something here... 
            //}


            #endregion 

            #region Extract video metadata from metadata pop up 
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    string videoName = "Untitled";
                    string archive_videoName = videoName;
                    string videoDescription = descriptionTxtBox.Text;
                    DateTime dateCreated = DateTime.Now;
                    bool isPublic = false;

                    if (privacyComboBox.SelectedIndex == 0)
                        isPublic = false;
                    else if (privacyComboBox.SelectedIndex == 1)
                        isPublic = true;

                    if (titleTxtBox.Text != "")
                    {
                        archive_videoName = titleTxtBox.Text;
                        videoName = titleTxtBox.Text + ".mp4";
                    }

                    tags = new string[tagsList.Count];
                    int i = 0;

                    foreach (string t in tagsList)
                    {
                        tags[i] = t;
                        i++; 
                    }
            // Create a VideoMetadata object 
            md = new VideoMetadata(SavedVideoId, archive_videoName, videoDescription, location_string, dateCreated.ToUniversalTime(), isPublic, tags);

            // Serialize the VideoMetadata object into JSON string
            string video_metadata_JSON = JsonConvert.SerializeObject(md);
                });
            #endregion  

            #region Send metadata
            progressTxtBlock.Text = "Uploading video metadata..."; 
            try
            {
                var videoMetadataRequest = new ApiRequest("uploadvideometadata");
                videoMetadataRequest.Authenticated = true;
                videoMetadataRequest.Parameters.Add("VideoId", SavedVideoId.ToString());
                videoMetadataRequest.AddJsonContent(md);

                var result = await videoMetadataRequest.ExecuteAsync(); 

            }
            catch (ApiException ex)
            {
                // api returned something other than 200
            }
            catch (Exception ex)
            {
                // something bad happened, hopefully not api related
            }
            #endregion
            
            #region Get a thumbnail image from the video file and upload it
            // Get thumbnail of the video file 
            var thumb = await videoFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView, 1000, Windows.Storage.FileProperties.ThumbnailOptions.UseCurrentScale);

            // Create a Buffer object to hold raw picture data
            var buffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(thumb.Size));

            // Read the raw picture data into the Buffer object 
            await thumb.ReadAsync(buffer, Convert.ToUInt32(thumb.Size), InputStreamOptions.None);

            // Open LocalFolder
            var folder = ApplicationData.Current.LocalFolder;

            // Create (or open if one exists) a folder called temp images
            folder = await folder.CreateFolderAsync("temp images", CreationCollisionOption.OpenIfExists);

            // Create a StorageFile 
            var thumbFile = await folder.CreateFileAsync(PHOTO_FILE_NAME, CreationCollisionOption.ReplaceExisting);

            // Write picture data to the file 
            await FileIO.WriteBufferAsync(thumbFile, buffer);

            // Preview the image
            var bmpimg = new BitmapImage();
            bmpimg.SetSource(thumb);
            PreviewImage.Source = bmpimg;

            try
            {

                var thumbnailUploadRequest = new ApiRequest("uploadvideoimage");
                thumbnailUploadRequest.Authenticated = true;
                thumbnailUploadRequest.Parameters.Add("VideoId", SavedVideoId.ToString());
                await thumbnailUploadRequest.AddFileContentAsync(thumbFile.Path);

                var result = await thumbnailUploadRequest.ExecuteAsync();

            }
            catch (ApiException ex)
            {
                // api returned something other than 200

            }
            catch (Exception ex)
            {
                // something bad happened, hopefully not api related
            }
            #endregion

            #region Upload complete, put the controls to normal
            uploadingPopUp.Visibility = Visibility.Collapsed;
            uploadingPopUp.IsOpen = false;
            //backButton.Visibility = Visibility.Visible; ;
            //ButtonsPanel.Visibility = Visibility.Visible;
            #endregion 

            #region Show success toast notification 
            //var notifier = ToastNotificationManager.CreateToastNotifier();
            //if (notifier.Setting == NotificationSetting.Enabled)
            //{
                
            //    var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            //    var title = template.GetElementsByTagName("text")[0];
            //    title.AppendChild(template.CreateTextNode("Upload successful.")); 
            //    var message = template.GetElementsByTagName("text")[1];
            //    message.AppendChild(template.CreateTextNode("Your video " + archive_videoName + " has been uploaded successfully."));
                

            //    var toast = new ToastNotification(template);
            //    notifier.Show(toast);
            //}
            #endregion

            // Background task stuff
            //var VideoUploadObject = new VideoUploadInBackgroundModel(App.LoggedInUser.UserId, md, thumbFile.Path, videoFile.Path);

            //VideoUploadTask.RegisterVideoUpload(VideoUploadObject); 
            

            
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

        #region Upload video to SkyDrive
        public async Task UploadVideoToSkyDrive()
        {
            #region Extract video metadata from metadata pop up
            string videoName = null;
            string videoDescription = descriptionTxtBox.Text;
            string tags = tagTxtBox.Text;
            DateTime dateCreated = DateTime.Now;

            if (titleTxtBox.Text != "")
                videoName = titleTxtBox.Text + ".mp4";
            #endregion 

            if (authClient == null)
                authClient = new LiveAuthClient(); 

            LiveLoginResult result = await authClient.LoginAsync(scopes);

            if (Is_SkyDrive_Enabled)
            {
                
                // This *does* traverse the network and get details of files held in root directory. 
                await root.LoadAsync();

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
                    SkyDriveFile skyDriveFile = await archiveSkyDriveFolder.UploadFileAsync(videoFile, videoName == null ? videoName : videoFile.DateCreated.ToString(), OverwriteOption.Rename);
                }
                catch
                {
                    // File upload failed... Do something!
                }


            }

        }
        #endregion

        #region Add tag button clicked
        private void addTagBtn_Click(object sender, RoutedEventArgs e)
        {
            tagsList.Add(tagTxtBox.Text);
            tagsTxtBlock.Text += "#" + tagTxtBox.Text + " ";
            tagTxtBox.Text = "";
            tagTxtBox.Focus(Windows.UI.Xaml.FocusState.Keyboard); 
            
        }
        #endregion

        #region Get thumbnail from video file, save the thumbnail as a .jpg image in the Local folder
        /// <summary>
        /// Get the thumbnail of videoFile (the StorageFile defined within the scope of this page), 
        /// save it as a .jpg image in the Local folder
        /// then upload the file to the Archive API
        /// </summary>
        public async Task GetThumbnail()
        {
            //// Get thumbnail of the video file 
            //var thumb = await videoFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView, 1000, Windows.Storage.FileProperties.ThumbnailOptions.UseCurrentScale);
            
            //// Create a Buffer object to hold raw picture data
            //var buffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(thumb.Size));

            //// Read the raw picture data into the Buffer object 
            //await thumb.ReadAsync(buffer, Convert.ToUInt32(thumb.Size), InputStreamOptions.None);

            //// Open LocalFolder
            //var folder = ApplicationData.Current.LocalFolder;

            //// Create (or open if one exists) a folder called temp images
            //folder = await folder.CreateFolderAsync("temp images", CreationCollisionOption.OpenIfExists);

            //// Create a StorageFile 
            //var thumbFile = await folder.CreateFileAsync(PHOTO_FILE_NAME, CreationCollisionOption.ReplaceExisting);

            //// Write picture data to the file 
            //await FileIO.WriteBufferAsync(thumbFile, buffer);

            //// Preview the image
            //var bmpimg = new BitmapImage();
            //bmpimg.SetSource(thumb);
            //PreviewImage.Source = bmpimg;

            //// Upload the image to the Archive API
            //await UploadImageToAPI(thumbFile);
  
        }
        #endregion 

        #region Upload video image to Archive URL
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageFile">The .jpg imageFile stored in Local folder to be uploaded</param>
        public async Task UploadImageToAPI(StorageFile imageFile)
        {
           
            HttpClient client = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();
            var stream = await imageFile.OpenReadAsync();
            StreamContent streamContent = new StreamContent(stream.AsStream(), 1024);
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            streamContent.Headers.ContentDisposition.Name = "\"File\"";
            streamContent.Headers.ContentDisposition.FileName = "\"" + Path.GetFileName(imageFile.Path) + "\"";
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            form.Add(new StringContent(VideoId.ToString()), "\"VideoId\"");
            form.Add(streamContent, "File");

            string address = "http://trout.wadec.com/API/uploadvideoimage";

            await client.PostAsync(address, form); 

            //try
            //{
            //    //var response = await client.PostAsync(address, form);
            //    await client.PostAsync(address, form);
            //    //var responsecontent = response.Result.Content; 
            //    //var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            //    //HttpContent response_content = client.PostAsync(address, form, timeout.Token).Result.Content;
            //}
            //catch (AggregateException e)
            //{
            //    // Do something here!!!

            //}

            
        }
        #endregion 

        private void privacyComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void tagTxtBox_KeyUp_1(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                addTagBtn_Click(sender, e); 
            }
        }
    }
}
