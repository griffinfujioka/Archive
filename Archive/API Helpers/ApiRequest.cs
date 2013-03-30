using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Archive.API_Helpers;

namespace Archive
{
	public class ApiRequest
	{
		public static long GetCurrentUnixTimestamp()
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			TimeSpan span = (DateTime.Now - epoch);
			return (long)span.TotalSeconds;
		}

		public const string APIKEY = "windows8";
		public const string APISECRET = "griffin";

		public string RequestPath { get; protected set; }

		private IDictionary<string, string> parameters = new Dictionary<string, string>();
		public IDictionary<string, string> Parameters { get { return this.parameters; } }

		private bool authenticated = false;
		public bool Authenticated { get { return this.authenticated; } set { this.authenticated = value; } }

		private HttpContent content = null;

		public ApiRequest(string requestPath)
		{
			if (string.IsNullOrWhiteSpace(requestPath))
				throw new ArgumentException("requestPath cannot be null or empty.");

			this.RequestPath = requestPath;
		}

		public void AddJsonContent(object content)
		{
			var jsonString = JsonConvert.SerializeObject(content);

			this.content = new StringContent(jsonString);
			this.content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
		}

		public async Task AddJsonContentAsync(object obj)
		{
			var jsonString = await JsonConvert.SerializeObjectAsync(obj);

			this.content = new StringContent(jsonString);
			this.content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
		}

		public async Task AddFileContentAsync(string filename)
		{
			// hack for testing from a project file
			//var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(filename));
			var file = await StorageFile.GetFileFromPathAsync(filename);
			var fileStream = await file.OpenReadAsync();
			this.content = new StreamContent(fileStream.AsStream());
			this.content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType.ToString());
		}
		
		public void AddByteContent(byte[] data)
		{
			this.content = new ByteArrayContent(data);
			this.content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
		}
		
		public void AddByteContent(byte[] data, int start, int count)
		{
			this.content = new ByteArrayContent(data, start, count);
			this.content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
		}

		public async Task<bool> ExecuteAsync()
		{
			var httpClient = new HttpClient();
			var request = this.BuildRequest();

			try
			{
				var response = await httpClient.SendAsync(request);
				if (response.StatusCode == HttpStatusCode.OK)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception e)
			{
				throw new Exception("Non Api Error", e);
			}
		}

		public async Task<TType> ExecuteAsync<TType>()
		{
			var httpClient = new HttpClient();
			var request = this.BuildRequest();

			try
			{
				var response = await httpClient.SendAsync(request);
				var responseJson = await response.Content.ReadAsStringAsync();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					return await JsonConvert.DeserializeObjectAsync<TType>(responseJson);
				}
				else
				{
					throw new ApiException(response.StatusCode, responseJson);
				}
			}
			catch (ApiException e)
			{
				throw e;
			}
			catch (Exception e)
			{
				throw new Exception("Non Api Error", e);
			}
		}

		public HttpMethod Method { get { return this.content == null ? HttpMethod.Get : HttpMethod.Post; } }

		protected HttpRequestMessage BuildRequest()
		{
			if (String.IsNullOrWhiteSpace(this.RequestPath))
				throw new InvalidOperationException("RequestPath must be set before executing a request.");

			var uri = this.CreateUri();
			var request = new HttpRequestMessage(this.Method, uri);
			//request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.Add("X-ApiKey", ApiRequest.APIKEY);

			if (this.Authenticated)
			{
				var timestamp = ApiRequest.GetCurrentUnixTimestamp();
				var accessToken = this.ComputeAccessToken(
					request.Method,
					uri.AbsolutePath.ToLower(),
					timestamp,
					this.Parameters
					);

                
				request.Headers.Add("X-AccessToken", WebUtility.UrlEncode(accessToken));
				request.Headers.Add("X-Timestamp", timestamp.ToString());
			}

			// check for content
			if (this.content != null)
				request.Content = content;

			return request;
		}

		protected Uri CreateUri()
		{
			var query = string.Join("&", this.Parameters.Select(x => String.Format("{0}={1}", System.Net.WebUtility.UrlEncode(x.Key), System.Net.WebUtility.UrlDecode(x.Value))).ToArray());
			var requestUrl = String.Format("http://trout.wadec.com/api/{0}?{1}", this.RequestPath, query);
			return new Uri(requestUrl);
		}

		protected string ComputeAccessToken(HttpMethod method, string uri, long unixTimestamp, IDictionary<string, string> parameters)
		{
			try
			{
				var mesageStringBuilder = new StringBuilder();
				var messageWriter = new StringWriter(mesageStringBuilder);

				messageWriter.WriteLine(APISECRET);
				messageWriter.WriteLine(method.Method);
				messageWriter.WriteLine(uri);
				messageWriter.WriteLine(unixTimestamp);
				messageWriter.WriteLine(String.Join("&", parameters.Select(x => string.Join("=", x.Key, x.Value))));

				var algorithmProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
				BinaryStringEncoding encoding = BinaryStringEncoding.Utf8;

				IBuffer messageBuffer = CryptographicBuffer.ConvertStringToBinary(mesageStringBuilder.ToString(), encoding);
				IBuffer signedMessage = algorithmProvider.HashData(messageBuffer);

				return CryptographicBuffer.EncodeToBase64String(signedMessage);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}
}
