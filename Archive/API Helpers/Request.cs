using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archive.API_Helpers
{
    public enum RequestType
    {
        GET, POST
    };

    /// <summary>
    /// A class to abstract the many requests used throughout this project.
    /// </summary>
    public class Request
    {
        public RequestType type { get; set; }
    }
}
