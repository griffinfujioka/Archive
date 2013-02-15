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
    class LoginRequest
    {
        [DataMember(Name = "Username")]
        public string Username { get; set; }

        [DataMember(Name = "Password")]
        public string Password { get; set; }

        public LoginRequest()
        {
            Username = "";
            Password = "";
        }

        public LoginRequest(string username, string password)
        {
            this.Username = username;
            this.Password = password; 
        }
    }
}
