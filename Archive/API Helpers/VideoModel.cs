using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archive.API_Helpers
{
    public class VideoModel
    {
        // header
        public int VideoId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public bool Public { get; set; }

        // metadata
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string[] Tags { get; set; }
        public DateTime Taken { get; set; }


        // preview
        public string VideoImage { get; set; }
        public int ImageSize { get; set; }
        public string ImageMimeType { get; set; }


        // file
        public string VideoFile { get; set; }
        public int VideoSize { get; set; }
        public string VideoMimeType { get; set; }

    }
}
