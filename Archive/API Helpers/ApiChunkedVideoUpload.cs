using Archive.API_Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Archive
{
	public class ApiChunkedVideoUpload
	{
		private const int CHUNK_SIZE = 1024 * 512;

		public event EventHandler<ApiChunkedVideoUploadProgressEventArgs> Progess;
		public event EventHandler UploadComplete;
		public event EventHandler UploadFailed;

		private int videoId;
		private string filePath;

		public ApiChunkedVideoUpload(int videoId, string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			this.videoId = videoId;
			this.filePath = filePath;
		}

		public async Task Execute()
		{
			// hack for testing from a project file
			//var videoFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(this.filePath));
			var videoFile = await StorageFile.GetFileFromPathAsync(this.filePath);
			var videoFileProperties = await videoFile.GetBasicPropertiesAsync();

			// determine number of chunks
			var fileSize = videoFileProperties.Size;
			var totalChunks = (long)((fileSize + CHUNK_SIZE) / CHUNK_SIZE);

			// send chunked upload request
			var chunkedUploadRequest = new ApiRequest("video/uploadchunked/request");
			chunkedUploadRequest.Authenticated = true;
			chunkedUploadRequest.AddJsonContent(new { VideoId = this.videoId, TotalChunks = totalChunks });
			var chunkedUploadRequestResponse = await chunkedUploadRequest.ExecuteAsync();

			// check response
			if (!chunkedUploadRequestResponse)
			{
				if (this.UploadFailed != null)
					this.UploadFailed.Invoke(this, new EventArgs());

				return;
			}

			// open file for reading
			var newFileStream = await videoFile.OpenReadAsync();
			var videoFileStream = newFileStream.AsStream();
			byte[] buffer = new byte[CHUNK_SIZE];

			// iterate 
			for (int i = 0; i < totalChunks; i++)
			{
				int readCount = await videoFileStream.ReadAsync(buffer, 0, CHUNK_SIZE);

				if (readCount > 0)
				{
					var uploadChunkRequest = new ApiRequest("video/uploadchunked/upload");
					uploadChunkRequest.Authenticated = true;
					uploadChunkRequest.Parameters.Add("VideoId", videoId.ToString());
					uploadChunkRequest.Parameters.Add("ChunkId", i.ToString());
					uploadChunkRequest.AddByteContent(buffer, 0, readCount);
					var uploadChunkRequestResponse = await uploadChunkRequest.ExecuteAsync();

					if(this.Progess != null)
						this.Progess.Invoke(this, new ApiChunkedVideoUploadProgressEventArgs(i + 1, (int)totalChunks));
				}
			}

			// fire upload complete event
			if (this.UploadComplete != null)
				this.UploadComplete.Invoke(this, new EventArgs());
		}

		public class ApiChunkedVideoUploadProgressEventArgs : EventArgs
		{
			public int CompletedChunks { get; set; }
			public int TotalChunks { get; set; }

			public ApiChunkedVideoUploadProgressEventArgs(int completedChunks, int totalChunks)
			{
				this.CompletedChunks = completedChunks;
				this.TotalChunks = totalChunks;
			}
		}
	}
}
