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
    class User
    {
        [DataMember(Name="UserId")]
        public int UserId { get; set; }

        [DataMember(Name="Username")] 
        public string Username { get; set; }

        [DataMember(Name = "Email")] 
        public string Email { get; set; }

        [DataMember(Name = "Created")] 
        public DateTime Created { get; set; }
    }
}
