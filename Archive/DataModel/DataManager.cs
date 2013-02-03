using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyDriveHelper;
using System.Collections.ObjectModel;   // Observable Collection
using Microsoft.Live;
using Archive; 

namespace Archive.DataModel
{
    class DataManager
    {
        /// <summary>
        /// Load videos from SkyDrive
        /// </summary>
        /// <returns></returns>
        static async public Task LoadAsyncFromSkydrive()
        {
            var scopes = new string[] { "wl.signin", "wl.skydrive", "wl.skydrive_update" };
            LiveAuthClient authClient = new LiveAuthClient();
            LiveLoginResult result = await authClient.LoginAsync(scopes);

            VideosDataSource.Unload();
            ObservableCollection<VideoDataCommon> videos = new ObservableCollection<VideoDataCommon>();

            if (result.Status == LiveConnectSessionStatus.Connected)
            {
                LiveConnectClient cxnClient = new LiveConnectClient(authClient.Session);

                // Get hold of the root folder from SkyDrive. 
                // NB: this does not traverse the network and get the full folder details.
                SkyDriveFolder root = new SkyDriveFolder(
                  cxnClient, SkyDriveWellKnownFolder.Root);
                
                var ArchiveFolder = await root.GetFolderAsync("Archive");
                if (ArchiveFolder == null)      // There is no Archive folder in the SkyDrive 
                    return;

                var files = await ArchiveFolder.GetFilesAsync();     // Get all files from SkyDrive 

                foreach (var file in files)
                {
                    try
                    {
                        
                        // Stream?
                        if(file.Name.Contains(".mp4"))
                        {
                            var video = new VideoDataCommon(file.Id, file.Name, file.Description, file.LinkLocation.ToString(), file.Description); 
                            videos.Add(video);

                            
                        }
                    }
                    catch { }
                }
            }

            var debugCollection = new ObservableCollection<VideoDataCommon>(videos);
            
            App.SkyDriveVideos = new ObservableCollection<VideoDataCommon>(videos); 
            //DataContractJsonSerializer des = new DataContractJsonSerializer(typeof(NoteDataCommon), types);
        }
    }
}
