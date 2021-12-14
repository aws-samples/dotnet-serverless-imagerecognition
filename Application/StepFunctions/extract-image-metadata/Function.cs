using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;
using Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace extract_image_metadata
{
    public class Function
    {
        public Function()
        {
            S3Client = new AmazonS3Client();
        }

        private IAmazonS3 S3Client { get; }

        /// <summary>
        ///     A simple function that takes a s3 bucket input and extract metadata of Image.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<ImageMetadata> FunctionHandler(ExecutionInput state, ILambdaContext context)
        {
            var srcKey = WebUtility.UrlDecode(state.SourceKey);
            
            var metadata = new ImageMetadata();
            using (var response = await S3Client.GetObjectAsync(state.Bucket, srcKey))
            {
                using (var sourceImage = Image.Load(response.ResponseStream, out var format))
                {
                    metadata.OriginalImagePixelCount = sourceImage.Width * sourceImage.Height;

                    metadata.Width = sourceImage.Width;

                    metadata.Height = sourceImage.Height;

                    metadata.ExifProfile = sourceImage.Metadata.ExifProfile;

                    metadata.Format = format.Name;
                }
            }

            context.Logger.LogLine("Photo metadata extracted succesfully");

            return metadata;
        }
    }
}