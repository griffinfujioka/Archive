﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;            // ImageSource
using Windows.UI.Xaml.Media.Imaging;    // BitmapImage
using System.Collections.Specialized;   // NotifyCollectionChangedAction
using System.Collections.ObjectModel;   // ObservableCollection

namespace Archive.DataModel
{
    #region VideoDataCommmon
    /// <summary>
    /// Base class for <see cref="VideoDataItem"/> and <see cref="VideosDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public class VideoDataCommon : Archive.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");   // Might not be correct for our purposes 

        public VideoDataCommon(String videoId, String title, String subtitle, String imagePath, String description)
        {
            this._videoId = videoId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _videoId = string.Empty;
        public string VideoId
        {
            get { return this._videoId; }
            set { this.SetProperty(ref this._videoId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(VideoDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }
    #endregion 

    #region VideoDataItem
    public abstract class VideoDataItem : VideoDataCommon
    {
        public VideoDataItem(String videoId, String title, String subtitle, String imagePath, String description, String content, VideoDataGroup group)
            : base(videoId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private VideoDataGroup _group;
        public VideoDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }
    #endregion 

    #region VideoDataGroup
    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class VideoDataGroup : VideoDataCommon
    {
     
        public VideoDataGroup(String videoId, String title, String subtitle, String imagePath, String description)
            : base(videoId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<VideoDataItem> _items = new ObservableCollection<VideoDataItem>();
        public ObservableCollection<VideoDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<VideoDataItem> _topItem = new ObservableCollection<VideoDataItem>();
        public ObservableCollection<VideoDataItem> TopItems
        {
            get { return this._topItem; }
        }
    }
    #endregion 

    #region VideosDataSource
    public sealed class VideosDataSource
    {
        public static bool DataLoaded;
        private static VideosDataSource _videosDataSource = new VideosDataSource();

        private ObservableCollection<VideoDataGroup> _allGroups = new ObservableCollection<VideoDataGroup>();
        public ObservableCollection<VideoDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<VideoDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _videosDataSource.AllGroups;
        }

        public static VideoDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _videosDataSource.AllGroups.Where((group) => group.VideoId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static VideoDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _videosDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.VideoId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public VideosDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat");

            //// Define a number of different groups (i.e., categories) and put individual items (i.e., videos) 
            //var group1 = new VideoDataGroup("Group-1",
            //        "Recent",
            //        "",
            //        "Assets/Person1.jpg",
            //        "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            //group1.Items.Add(new VideoDataItem("Group-1-Item-1",
            //        "Here is my first video",
            //        "Just trying this new service out",
            //        "Assets/Person1.jpg",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group1));
            //group1.Items.Add(new VideoDataItem("Group-1-Item-2",
            //        "I'm having so much fun!",
            //        "This new service is actually prety sweet. Whoever made it must have known what they were doing.",
            //        "Assets/Person2.jpg",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group1));
            //group1.Items.Add(new VideoDataItem("Group-1-Item-3",
            //        "This is title 3",
            //        "Item Subtitle: 3",
            //        "Assets/Person3.jpg",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group1));
            //group1.Items.Add(new VideoDataItem("Group-1-Item-4",
            //        "Item Title: 4",
            //        "Item Subtitle: 4",
            //        "Assets/Person4.jpg",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group1));
            //group1.Items.Add(new VideoDataItem("Group-1-Item-5",
            //        "Item Title: 5",
            //        "Item Subtitle: 5",
            //        "Assets/Person5.jpg",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group1));
            //this.AllGroups.Add(group1);

            //var group2 = new VideoDataGroup("Group-2",
            //        "Favorites",
            //        "Group Subtitle: 2",
            //        "Assets/LightGray.png",
            //        "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            //group2.Items.Add(new VideoDataItem("Group-2-Item-1",
            //        "Item Title: 1",
            //        "Item Subtitle: 1",
            //        "Assets/DarkGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group2));
            //group2.Items.Add(new VideoDataItem("Group-2-Item-2",
            //        "Item Title: 2",
            //        "Item Subtitle: 2",
            //        "Assets/MediumGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group2));
            //group2.Items.Add(new VideoDataItem("Group-2-Item-3",
            //        "Item Title: 3",
            //        "Item Subtitle: 3",
            //        "Assets/LightGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group2));
            //this.AllGroups.Add(group2);

            //var group3 = new VideoDataGroup("Group-3",
            //        "Personal",
            //        "Group Subtitle: 3",
            //        "Assets/MediumGray.png",
            //        "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            //group3.Items.Add(new VideoDataItem("Group-3-Item-1",
            //        "Item Title: 1",
            //        "Item Subtitle: 1",
            //        "Assets/MediumGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group3));
            //group3.Items.Add(new VideoDataItem("Group-3-Item-2",
            //        "Item Title: 2",
            //        "Item Subtitle: 2",
            //        "Assets/LightGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group3));
            //group3.Items.Add(new VideoDataItem("Group-3-Item-3",
            //        "Item Title: 3",
            //        "Item Subtitle: 3",
            //        "Assets/DarkGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group3));
            //group3.Items.Add(new VideoDataItem("Group-3-Item-4",
            //        "Item Title: 4",
            //        "Item Subtitle: 4",
            //        "Assets/LightGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group3));
            //group3.Items.Add(new VideoDataItem("Group-3-Item-5",
            //        "Item Title: 5",
            //        "Item Subtitle: 5",
            //        "Assets/MediumGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group3));
            //group3.Items.Add(new VideoDataItem("Group-3-Item-6",
            //        "Item Title: 6",
            //        "Item Subtitle: 6",
            //        "Assets/DarkGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group3));
            //group3.Items.Add(new VideoDataItem("Group-3-Item-7",
            //        "Item Title: 7",
            //        "Item Subtitle: 7",
            //        "Assets/MediumGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group3));
            //this.AllGroups.Add(group3);

            //var group4 = new VideoDataGroup("Group-4",
            //        "Group Title: 4",
            //        "Group Subtitle: 4",
            //        "Assets/LightGray.png",
            //        "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            //group4.Items.Add(new VideoDataItem("Group-4-Item-1",
            //        "Item Title: 1",
            //        "Item Subtitle: 1",
            //        "Assets/DarkGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group4));
            //group4.Items.Add(new VideoDataItem("Group-4-Item-2",
            //        "Item Title: 2",
            //        "Item Subtitle: 2",
            //        "Assets/LightGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group4));
            //group4.Items.Add(new VideoDataItem("Group-4-Item-3",
            //        "Item Title: 3",
            //        "Item Subtitle: 3",
            //        "Assets/DarkGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group4));
            //group4.Items.Add(new VideoDataItem("Group-4-Item-4",
            //        "Item Title: 4",
            //        "Item Subtitle: 4",
            //        "Assets/LightGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group4));
            //group4.Items.Add(new VideoDataItem("Group-4-Item-5",
            //        "Item Title: 5",
            //        "Item Subtitle: 5",
            //        "Assets/MediumGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group4));
            //group4.Items.Add(new VideoDataItem("Group-4-Item-6",
            //        "Item Title: 6",
            //        "Item Subtitle: 6",
            //        "Assets/LightGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group4));
            //this.AllGroups.Add(group4);

            //var group5 = new VideoDataGroup("Group-5",
            //        "Group Title: 5",
            //        "Group Subtitle: 5",
            //        "Assets/MediumGray.png",
            //        "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            //group5.Items.Add(new VideoDataItem("Group-5-Item-1",
            //        "Item Title: 1",
            //        "Item Subtitle: 1",
            //        "Assets/LightGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group5));
            //group5.Items.Add(new VideoDataItem("Group-5-Item-2",
            //        "Item Title: 2",
            //        "Item Subtitle: 2",
            //        "Assets/DarkGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group5));
            //group5.Items.Add(new VideoDataItem("Group-5-Item-3",
            //        "Item Title: 3",
            //        "Item Subtitle: 3",
            //        "Assets/LightGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group5));
            //group5.Items.Add(new VideoDataItem("Group-5-Item-4",
            //        "Item Title: 4",
            //        "Item Subtitle: 4",
            //        "Assets/MediumGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group5));
            //this.AllGroups.Add(group5);

            //var group6 = new VideoDataGroup("Group-6",
            //        "Group Title: 6",
            //        "Group Subtitle: 6",
            //        "Assets/DarkGray.png",
            //        "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            //group6.Items.Add(new VideoDataItem("Group-6-Item-1",
            //        "Item Title: 1",
            //        "Item Subtitle: 1",
            //        "Assets/LightGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group6));
            //group6.Items.Add(new VideoDataItem("Group-6-Item-2",
            //        "Item Title: 2",
            //        "Item Subtitle: 2",
            //        "Assets/DarkGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group6));
            //group6.Items.Add(new VideoDataItem("Group-6-Item-3",
            //        "Item Title: 3",
            //        "Item Subtitle: 3",
            //        "Assets/MediumGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group6));
            //group6.Items.Add(new VideoDataItem("Group-6-Item-4",
            //        "Item Title: 4",
            //        "Item Subtitle: 4",
            //        "Assets/DarkGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group6));
            //group6.Items.Add(new VideoDataItem("Group-6-Item-5",
            //        "Item Title: 5",
            //        "Item Subtitle: 5",
            //        "Assets/LightGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group6));
            //group6.Items.Add(new VideoDataItem("Group-6-Item-6",
            //        "Item Title: 6",
            //        "Item Subtitle: 6",
            //        "Assets/MediumGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group6));
            //group6.Items.Add(new VideoDataItem("Group-6-Item-7",
            //        "Item Title: 7",
            //        "Item Subtitle: 7",
            //        "Assets/DarkGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group6));
            //group6.Items.Add(new VideoDataItem("Group-6-Item-8",
            //        "Item Title: 8",
            //        "Item Subtitle: 8",
            //        "Assets/LightGray.png",
            //        "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            //        ITEM_CONTENT,
            //        group6));
            //this.AllGroups.Add(group6);
        }

        /// <summary>
        /// Clean AllGroups collection
        /// </summary>
        public static void Unload()
        {
            _videosDataSource._allGroups.Clear();
        }

        async public Task Load()
        {
            try
            {
                await DataManager.LoadAsyncFromSkydrive();
                DataLoaded = true;
            }
            catch (Exception ex)
            {
                DataLoaded = false;
            }
        }

    }
    #endregion 
}
