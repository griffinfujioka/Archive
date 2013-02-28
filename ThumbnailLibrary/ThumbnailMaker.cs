using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace ThumbnailLibrary
{
    /// <summary>
    /// Generates a thumbnnail from an image
    /// </summary>
    public sealed class ThumbnailMaker
    {
        // URL format for referencing local storage
        private const string URI_LOCAL = "ms-appdata:///Local/{0}";

        /// <summary>
        /// Creates a thumbnail image in local storage based on the source file, then returns the URI to the thumbnail
        /// </summary>
        /// <param name="file">The source image file to generate the thumbnail from</param>
        /// <returns>The URI to the generated thumbnail in local storage</returns>
        public IAsyncOperation<Uri> GenerateThumbnailAsync(IStorageFile file)
        {
            return GenerateThumbnail(file).AsAsyncOperation();
        }

        private static async Task<Uri> GenerateThumbnail(IStorageFile file)
        {
            using (var fileStream = await file.OpenReadAsync())
            {
                // decode the file using the built-in image decoder
                var decoder = await BitmapDecoder.CreateAsync(fileStream);

                // create the output file for the thumbnail
                var thumbFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    string.Format("thumbnail{0}", file.FileType),
                    CreationCollisionOption.GenerateUniqueName);

                // create a stream for the output file
                using (var outputStream = await thumbFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // create an encoder from the existing decoder and set the scaled height
                    // and width 
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(
                        outputStream,
                        decoder);
                    encoder.BitmapTransform.ScaledHeight = 100;
                    encoder.BitmapTransform.ScaledWidth = 100;
                    await encoder.FlushAsync();
                }

                // create the URL
                var storageUrl = string.Format(URI_LOCAL, thumbFile.Name);

                // return it
                return new Uri(storageUrl, UriKind.Absolute);
            }
        }
    }
}