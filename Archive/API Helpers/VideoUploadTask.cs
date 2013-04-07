using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.Threading;

namespace Archive.API_Helpers
{
    public sealed class VideoUploadTask : IBackgroundTask
    {
        private const string TaskName = "VideoUploadTask";

        private static ConcurrentQueue<VideoUploadInBackgroundModel> uploadQueue = new ConcurrentQueue<VideoUploadInBackgroundModel>();

        public static void RegisterVideoUpload(VideoUploadInBackgroundModel o)
        {
            VideoUploadTask.uploadQueue.Enqueue(o);

            bool taskRegistered = false;
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == TaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();
                builder.Name = TaskName;
                builder.TaskEntryPoint = "Archive.API_Helpers.VideoUploadTask";
                builder.SetTrigger(new TimeTrigger(15, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            }
        }

        volatile bool cancelRequested = false;
        BackgroundTaskDeferral deferral = null;
        IBackgroundTaskInstance taskInstance = null;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            this.deferral = taskInstance.GetDeferral();
            this.taskInstance = taskInstance;

            VideoUploadInBackgroundModel model;
            if (uploadQueue.TryDequeue(out model))
            {
                await this.UploadVideo(model);
            }
            else
            {
                this.deferral.Complete();
            }
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            this.cancelRequested = true;
        }

        private async Task UploadVideo(VideoUploadInBackgroundModel model)
        {
            int savedVideoId;


            // try to create the video in the system
            try
            {
                var createVideoRequest = new ApiRequest("createvideo");
                createVideoRequest.Authenticated = true;
                await createVideoRequest.AddJsonContentAsync(new { UserId = model.LoggedInUserId });
                var videoId = await createVideoRequest.ExecuteAsync<VideoIdModel>();
                savedVideoId = videoId.VideoId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                this.UploadFailed(model);
                return;
            }

            // upload metadata, don't take no for an answer
            var metadataUploaded = false;
            while (!metadataUploaded)
            {
                try
                {
                    var videoMetadataRequest = new ApiRequest("uploadvideometadata");
                    videoMetadataRequest.Authenticated = true;
                    videoMetadataRequest.Parameters.Add("VideoId", savedVideoId.ToString());
                    videoMetadataRequest.AddJsonContent(JsonConvert.SerializeObject(model.Metadata));
                    metadataUploaded = await videoMetadataRequest.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            // upload thumbnail, don't take no for an answer
            var thumbnailUploaded = false;
            while (!thumbnailUploaded)
            {
                try
                {
                    var thumbnailUploadRequest = new ApiRequest("uploadvideoimage");
                    thumbnailUploadRequest.Authenticated = true;
                    thumbnailUploadRequest.Parameters.Add("VideoId", savedVideoId.ToString());
                    await thumbnailUploadRequest.AddFileContentAsync(model.ImagePath);

                    thumbnailUploaded = await thumbnailUploadRequest.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            var videoUploaded = false;
            int[] missingChunks = null;
            while (!videoUploaded)
            {
                try
                {
                    var videoUpload = new ApiChunkedVideoUpload(savedVideoId, model.VideoPath);
                    videoUpload.MissingChunks = missingChunks;
                    await videoUpload.Execute();

                    var videoCompleteRequest = new ApiRequest("video/uploadchunked/iscomplete");
                    videoCompleteRequest.Authenticated = true;
                    videoCompleteRequest.Parameters.Add("VideoId", savedVideoId.ToString());
                    missingChunks = await videoCompleteRequest.ExecuteAsync<int[]>();

                    if (missingChunks.Length == 0)
                        videoUploaded = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            this.deferral.Complete();
        }

        private void UploadFailed(VideoUploadInBackgroundModel model)
        {
            uploadQueue.Enqueue(model);
            this.deferral.Complete();
        }
    }
}
