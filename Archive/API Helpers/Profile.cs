using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Archive.API_Helpers
{
    public class Profile
    {
        [DataMember(Name = "User")]
         public UserProfile User { get; set; }

        [DataMember(Name = "Videos")]
         public List<VideoModel> Videos { get; set; }

        [DataMember(Name = "Followers")]
         public List<User> Followers { get; set; }

        [DataMember(Name = "Following")]
         public List<User> Following { get; set; }
    }
}
