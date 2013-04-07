using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archive.API_Helpers
{
    public class VideoUploadInBackgroundModel
    {
        public int LoggedInUserId { get; set; }
        public VideoMetadata Metadata { get; set; }
        public string ImagePath { get; set; }
        public string VideoPath { get; set; }

        public VideoUploadInBackgroundModel(int userId, VideoMetadata md, string imagePath, string videoPath)
        {
            this.LoggedInUserId = userId;
            this.Metadata = md;
            this.ImagePath = imagePath;
            this.VideoPath = videoPath; 
        }
    }
}
