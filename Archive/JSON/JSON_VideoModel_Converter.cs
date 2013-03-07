using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;     // JObject  
using Archive.API_Helpers;

namespace Archive.JSON
{
    class JSON_VideoModel_Converter : JsonCreationConverter<VideoModel>
    {
        protected override VideoModel Create(Type objectType, JObject jsonObject)
        {
            return new VideoModel(); 
        }
    }
}
