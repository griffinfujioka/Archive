using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;    // JSON Serialization

namespace Archive.API_Helpers
{
    [DataContract]
    public class VideoMetadata
    {

        [DataMember(Name = "VideoId")]
        public int VideoId { get; set;}

        [DataMember(Name = "Title")]
        public string Title { get; set; }

        [DataMember(Name = "Description")]
        public string Description { get; set; }

        [DataMember(Name = "Location")]
        public string Location { get; set; }

        [DataMember(Name = "Taken")]
        public DateTime DateCreated { get; set; }

        [DataMember(Name = "Public")]
        public bool Public { get; set; }

        [DataMember(Name = "Tags")]
        public string[] Tags { get; set; }

        public VideoMetadata()
        {
            this.Title = "";
            this.Description = "";
            this.DateCreated = DateTime.Now; 
        }

        public VideoMetadata(int VideoId, string Title, string Description, string Location, DateTime DateCreated, bool Public, string[] tags)
        {
            this.VideoId = VideoId; 
            this.Title = Title;
            this.Description = Description;
            this.Location = Location; 
            this.DateCreated = DateCreated;
            this.Public = Public;
            this.Tags = tags;
        }

    }
}
