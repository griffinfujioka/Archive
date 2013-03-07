using Archive.API_Helpers;
using Archive.Common;
using Archive.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Archive.JSON;
using System; 

namespace Archive.API_Helpers
{
    public class VideoFeedFunctions
    {
        public async void GetMyVideoFeed(int UserId)
        {
            if (App.LoggedInUser == null)
                return;

            WebResponse response;                   // Response from createvideo URL 
            Stream responseStream;                  // Stream data from responding URL
            StreamReader reader;                    // Read data in stream 
            string responseJSON;                    // The JSON string returned to us by the Archive API 
            List<VideoModel> myVideos = new List<VideoModel>();
            var url = "http://trout.wadec.com/API/videos/feed?" + App.LoggedInUser.UserId;
            string UserID_JSON = JsonConvert.SerializeObject(new { UserId = App.LoggedInUser.UserId });
            // Initiate HttpWebRequest with Archive API
            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            // Set the method to POST
            request.Method = "GET";

            // Add headers 
            request.Headers["X-ApiKey"] = "123456";
            request.Headers["X-AccessToken"] = "UqYONgdB/aCCtF855bp8CSxmuHo=";

            // Set the ContentType property of the WebRequest
            request.ContentType = "application/json";
            // Create POST data and convert it to a byte array
            byte[] byteArray = Encoding.UTF8.GetBytes(UserID_JSON);


            // Create a stream request
            Stream dataStream = await request.GetRequestStreamAsync();

            // Write the data to the stream
            dataStream.Write(byteArray, 0, byteArray.Length);



            try
            {
                // Get response from URL
                response = await request.GetResponseAsync();

                using (responseStream = response.GetResponseStream())
                {
                    reader = new StreamReader(responseStream);

                    // Read a string of JSON into responseJSON
                    responseJSON = reader.ReadToEnd();


                    // Deserialize all of the ideas in the file into a list of ideas
                    List<VideoModel> deserialized = JsonConvert.DeserializeObject<List<VideoModel>>(responseJSON,
      new JSON_VideoModel_Converter());

                    myVideos = deserialized; 

                }

            }
            catch (Exception ex)
            {
                // Do something here!!!
            }


        }
    }
}