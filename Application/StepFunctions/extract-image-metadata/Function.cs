using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Common;
using SixLabors.ImageSharp;

namespace extract_image_metadata
{
    public class Function
    {
        private static IAmazonS3 S3Client { get; }

        static Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            S3Client = new AmazonS3Client();
        }

        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
        /// <param name="args"></param>
        private static async Task Main()
        {
            Func<ExecutionInput, ILambdaContext, Task<ImageMetadata>> handler = FunctionHandler;
            await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<CustomJsonSerializerContext>(options => {
                options.PropertyNameCaseInsensitive = true;
            }))
                .Build()
                .RunAsync();
        }

        /// <summary>
        ///     A simple function that takes a s3 bucket input and extract metadata of Image.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<ImageMetadata> FunctionHandler(ExecutionInput state, ILambdaContext context)
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