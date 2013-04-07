using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Archive.API_Helpers
{
    [DataContract]
    public class UserProfile : User
    {
        [DataMember]
        public bool Following { get; set; }

    }
}
