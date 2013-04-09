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
        public User User { get; set; }
        public DateTime Created { get; set; }
        public bool Public { get; set; }

        // metadata
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string[] Tags { get; set; }
        public DateTime Taken { get; set; }

        public string VideoUrl { get; set; }
        public string SmallImageUrl { get; set; }
        public string MediumImageUrl { get; set; }
        public string LargeImageUrl { get; set; }

    }
}
