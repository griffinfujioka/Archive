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
    class VideoMetadata
    {
        [DataMember(Name = "Title")]
        public string Title { get; set; }

        [DataMember(Name = "Description")]
        public string Description { get; set; }

        [DataMember(Name = "DateCreated")]
        public DateTime DateCreated { get; set; }

        public VideoMetadata()
        {
            this.Title = "";
            this.Description = "";
            this.DateCreated = DateTime.Now; 
        }

        public VideoMetadata(string Title, string Description, DateTime DateCreated)
        {
            this.Title = Title;
            this.Description = Description;
            this.DateCreated = DateCreated; 
        }

    }
}
