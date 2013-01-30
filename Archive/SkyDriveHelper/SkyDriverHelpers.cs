

namespace SkyDriveHelper
{
   
    using System;
    using System.Collections.Generic; 
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Live;
    using SkyDriveBag = System.Collections.Generic.IDictionary<string, object>;
    using Windows.Storage;


    #region SkyDriveWellKnownFolder
    public enum SkyDriveWellKnownFolder
    {
        Root,
        CameraRoll,
        Documents,
        Photos,
        Public,
        RecentDocuments
    }
    #endregion

    #region SkyDriveItem
    public class SkyDriveItem
    {
        public async Task LoadAsync()
        {
            // We load whether already loaded or not.
            LiveOperationResult result = await this.CxnClient.GetAsync(this.Id);

            this.ItemDetails = result.Result;

            this.ItemState = ItemStateType.Loaded;
        }
        public async Task SaveAsync()
        {
            CheckItemState();

            LiveOperationResult result = await this.CxnClient.PutAsync(
              this.Id, this.ItemDetails);

            this.ItemDetails = result.Result;

            this.ItemState = ItemStateType.Loaded;
        }
        public string Name
        {
            get
            {
                CheckItemState();
                return ((string)this.ItemDetails[NAME_KEY]);
            }
            set
            {
                CheckItemState();
                this.ItemDetails[NAME_KEY] = value;
            }
        }
        public string Description
        {
            get
            {
                CheckItemState();
                return ((string)this.ItemDetails[DESCRIPTION_KEY]);
            }
            set
            {
                this.ItemDetails[DESCRIPTION_KEY] = value;
            }
        }
        public Uri LinkLocation
        {
            get
            {
                CheckItemState();
                return (new Uri((string)this.ItemDetails[LINK_KEY]));
            }
        }
        public Uri UploadLocation
        {
            get
            {
                CheckItemState();
                return (new Uri((string)this.ItemDetails[UPLOAD_KEY]));
            }
        }
        public DateTimeOffset CreatedTime
        {
            get
            {
                CheckItemState();
                return (DateTimeOffset.Parse(
                  (string)this.ItemDetails[CREATED_KEY]));
            }
        }
        public DateTimeOffset UpdatedTime
        {
            get
            {
                CheckItemState();
                return (DateTimeOffset.Parse(
                  (string)this.ItemDetails[UPDATED_KEY]));
            }
        }
        public async Task DeleteAsync()
        {
            CheckItemState();
            await this.CxnClient.DeleteAsync(this.Id);
            MarkStale();
        }
        protected enum SkyDriveItemType
        {
            folder,
            album
        }
        protected SkyDriveItem(LiveConnectClient cxnClient)
        {
            this.CxnClient = cxnClient;
        }
        virtual internal string Id
        {
            get
            {
                return ((string)this.ItemDetails[ID_KEY]);
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        internal void MarkStale()
        {
            if (this.ItemState == ItemStateType.Loaded)
            {
                this.ItemState = ItemStateType.Stale;
            }
        }
        protected LiveConnectClient CxnClient
        {
            get;
            private set;
        }
        protected SkyDriveBag ItemDetails
        {
            get;
            set;
        }
        protected enum ItemStateType
        {
            Unloaded,
            Loaded,
            Stale
        }
        SkyDriveItem()
        {
        }
        protected ItemStateType ItemState
        {
            get;
            set;
        }
        protected void CheckItemState()
        {
            if (this.ItemState != ItemStateType.Loaded)
            {
                throw new InvalidOperationException("Unexpected item state - refresh the item with LoadAsync?");
            }
        }
        static protected bool IsAnyType(SkyDriveBag item, params SkyDriveItemType[] types)
        {
            return (types.Any(t => IsType(item, t)));
        }
        static protected bool IsType(SkyDriveBag item, SkyDriveItemType type)
        {
            return ((string)item[TYPE_KEY] == Enum.GetName(typeof(SkyDriveItemType), type));
        }

        protected static readonly string ID_KEY = "id";
        protected static readonly string TYPE_KEY = "type";
        protected static readonly string NAME_KEY = "name";
        protected static readonly string DESCRIPTION_KEY = "description";
        protected static readonly string UPLOAD_KEY = "upload_location";
        protected static readonly string LINK_KEY = "link";
        protected static readonly string CREATED_KEY = "created_time";
        protected static readonly string UPDATED_KEY = "updated_time";
    }
    #endregion

    #region SkyDriveFile
    public class SkyDriveFile : SkyDriveItem
    {
        internal SkyDriveFile(LiveConnectClient cxnClient, SkyDriveBag item)
            : base(cxnClient)
        {
            this.ItemDetails = item;
            this.ItemState = ItemStateType.Loaded;
        }
        public async Task<IStorageFile> DownloadAsync()
        {
            LiveDownloadOperationResult result = await this.CxnClient.BackgroundDownloadAsync(this.Id);
            return (result.File);
        }
        public async Task DownloadAsync(IStorageFile file)
        {
            await this.CxnClient.BackgroundDownloadAsync(
              this.Id + CONTENT_SPECIFIER, file);
        }
        public async Task<SkyDriveFile> CopyAsync(SkyDriveFolder destination)
        {
            LiveOperationResult result = await this.CxnClient.CopyAsync(this.Id, destination.Id);
            this.MarkStale();
            destination.MarkStale();
            return (new SkyDriveFile(this.CxnClient, result.Result));
        }
        public async Task<SkyDriveFile> MoveAsync(SkyDriveFolder destination)
        {
            LiveOperationResult result = await this.CxnClient.MoveAsync(this.Id, destination.Id);
            this.MarkStale();
            destination.MarkStale();
            return (new SkyDriveFile(this.CxnClient, result.Result));
        }
        static readonly string CONTENT_SPECIFIER = "/content";
    }
    #endregion

    #region SkyDriveFolder
    public class SkyDriveFolder : SkyDriveItem
    {
        static SkyDriveFolder()
        {
            _foldersLookup = new Dictionary<SkyDriveWellKnownFolder, string>()
      {
        { SkyDriveWellKnownFolder.Root,             ROOT_FOLDER },
        { SkyDriveWellKnownFolder.CameraRoll,       ROOT_FOLDER + "/camera_roll" },
        { SkyDriveWellKnownFolder.Documents,        ROOT_FOLDER + "/my_documents" },
        { SkyDriveWellKnownFolder.Photos,           ROOT_FOLDER + "/my_photos" },
        { SkyDriveWellKnownFolder.Public,           ROOT_FOLDER + "/public_documents" },
        { SkyDriveWellKnownFolder.RecentDocuments,  ROOT_FOLDER + "/recent_docs" }
      };
        }
        public SkyDriveFolder(LiveConnectClient cxnClient, SkyDriveWellKnownFolder wellKnownFolder) :
            base(cxnClient)
        {
            this.Id = _foldersLookup[wellKnownFolder];
        }
        public uint Count
        {
            get
            {
                CheckItemState();
                return ((uint)(int)(this.ItemDetails[COUNT_KEY]));
            }
        }
        public async Task<IEnumerable<SkyDriveFile>> GetFilesAsync()
        {
            List<SkyDriveFile> files = new List<SkyDriveFile>();

            await this.EnumerateFolder(
              bag => !IsAnyType(bag, SkyDriveItemType.album, SkyDriveItemType.folder),
              bag =>
              {
                  files.Add(new SkyDriveFile(this.CxnClient, bag));
              }
            );
            return (files);
        }
        public async Task<IEnumerable<SkyDriveFolder>> GetFoldersAsync()
        {
            List<SkyDriveFolder> folders = new List<SkyDriveFolder>();

            await this.EnumerateFolder(
              bag => IsAnyType(bag, SkyDriveItemType.folder, SkyDriveItemType.album),
              bag =>
              {
                  folders.Add(new SkyDriveFolder(this.CxnClient, bag));
              }
            );
            return (folders);
        }
        public async Task<SkyDriveFolder> MoveAsync(SkyDriveFolder destination)
        {
            LiveOperationResult result = await this.CxnClient.MoveAsync(this.Id, destination.Id);
            this.MarkStale();
            destination.MarkStale();
            return (new SkyDriveFolder(this.CxnClient, result.Result));
        }
        // TODO: must be a better way of doing this than getting all the files
        // and then removing the ones we don't want :-)
        public async Task<SkyDriveFile> GetFileAsync(string fileName)
        {
            var files = await GetFilesAsync();
            return (files.FirstOrDefault(
              f => string.Compare(f.Name, fileName, StringComparison.OrdinalIgnoreCase) == 0));
        }
        public async Task<SkyDriveFolder> GetFolderAsync(string fileName)
        {
            var folders = await GetFoldersAsync();
            return (folders.FirstOrDefault(
              f => string.Compare(f.Name, fileName, StringComparison.OrdinalIgnoreCase) == 0));
        }
        public async Task<SkyDriveFolder> CreateFolderAsync(string folderName)
        {
            SkyDriveBag bag = new Dictionary<string, object>();
            bag.Add(NAME_KEY, folderName);
            LiveOperationResult result = await this.CxnClient.PostAsync(this.Id, bag);

            this.MarkStale();

            return (new SkyDriveFolder(this.CxnClient, result.Result));
        }
        public async Task<SkyDriveFile> UploadFileAsync(StorageFile file,
          string skyDriveFileName = null,
          OverwriteOption overwrite = OverwriteOption.DoNotOverwrite)
        {
            LiveOperationResult result = await this.CxnClient.BackgroundUploadAsync(this.Id,
              string.IsNullOrEmpty(skyDriveFileName) ? file.Name : skyDriveFileName,
              file,
              overwrite);

            this.MarkStale();

            return (new SkyDriveFile(this.CxnClient, result.Result));
        }
        internal override string Id
        {
            get
            {
                string id = null;
                if (base.ItemState == ItemStateType.Unloaded)
                {
                    return (this._id);
                }
                else
                {
                    id = base.Id;
                }
                return (id);
            }
            set
            {
                if (base.ItemState != ItemStateType.Unloaded)
                {
                    throw new InvalidOperationException("Folder populated");
                }
                this._id = value;
            }
        }
        async Task EnumerateFolder(
          Predicate<SkyDriveBag> selector,
          Action<SkyDriveBag> callback)
        {
            var result = await this.CxnClient.GetAsync(this.Id + FILES_SPECIFIER);

            var resultDictionary = (SkyDriveBag)result.Result;
            var resultData = (List<object>)resultDictionary[DATA_KEY];

            foreach (SkyDriveBag item in resultData)
            {
                if (selector(item))
                {
                    callback(item);
                }
            }
        }

        SkyDriveFolder(LiveConnectClient cxnClient, SkyDriveBag item) :
            base(cxnClient)
        {
            this.ItemDetails = item;
            this.ItemState = ItemStateType.Loaded;
        }

        string _id;

        static Dictionary<SkyDriveWellKnownFolder, string> _foldersLookup;
        static readonly string ROOT_FOLDER = "me/skydrive";
        static readonly string FILES_SPECIFIER = "/files";
        static readonly string DATA_KEY = "data";
        static readonly string COUNT_KEY = "count";
    }
    #endregion
}