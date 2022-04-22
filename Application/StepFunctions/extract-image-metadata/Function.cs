using System.IO;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
[assembly: LambdaSerializer(typeof(NewtonJsonSerializer))]

namespace extract_image_metadata
{

    [JsonSerializable(typeof(ExecutionInput))]
    [JsonSerializable(typeof(ImageMetadata))]
    public partial class JsonLambdaContext : JsonSerializerContext
    {

    }

    public class Function
    {
        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
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
            var logger = new ImageRecognitionLogger(state, context);

            var srcKey = WebUtility.UrlDecode(state.SourceKey);
            var tmpPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(srcKey));
            try
            {
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

                await logger.WriteMessageAsync(new MessageEvent {Message = "Photo metadata extracted succesfully"},
                    ImageRecognitionLogger.Target.All);

                return metadata;
            }
            finally
            {
                if (File.Exists(tmpPath)) File.Delete(tmpPath);
            }
        }
    }
}