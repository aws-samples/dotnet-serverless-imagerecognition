using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.S3.Model;
using Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace thumbnail
{
    public class Function
    {
        private const int MAX_WIDTH = 250;
        private const int MAX_HEIGHT = 250;

        public Function()
        {
            S3Client = new AmazonS3Client();
        }

        private IAmazonS3 S3Client { get; }

        /// <summary>
        ///     A simple function that takes a string and returns both the upper and lower case version of the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<ThumbnailInfo> FunctionHandler(Input input, ILambdaContext context)
        {
            var size = input.ExtractedMetadata.Dimensions;

            var scalingFactor = Math.Min(
                MAX_WIDTH / size.Width,
                MAX_HEIGHT / size.Height
            );

            var width = Convert.ToInt32(scalingFactor * size.Width);
            var height = Convert.ToInt32(scalingFactor * size.Height);

            var image = await GenerateThumbnail(input.Bucket, input.SourceKey, width, height);

            var destinationKey = input.SourceKey.Replace("uploads", "resized");

            using (var stream = new MemoryStream())
            {
                await image.thumbnailImage.SaveAsync(stream, image.format);
                stream.Position = 0;

                context.Logger.LogLine($"Saving thumbnail to {destinationKey} with size {stream.Length}");
                await S3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = input.Bucket,
                    Key = destinationKey,
                    InputStream = stream
                });

                context.Logger.LogLine("Photo thumbnail created");

                return new ThumbnailInfo(width, height, destinationKey, input.Bucket);
            }
        }

        private async Task<ThumbnailImage> GenerateThumbnail(string s3Bucket, string srcKey, int width, int height)
        {
            srcKey = WebUtility.UrlDecode(srcKey.Replace("+", " "));
            using (var response = await S3Client.GetObjectAsync(s3Bucket, srcKey))
            {
                var image = Image.Load(response.ResponseStream, out var format);
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size {Width = width, Height = height},
                    Mode = ResizeMode.Stretch
                }));

                return new ThumbnailImage(image, format);
            }
        }
    }

    public record ThumbnailImage(Image thumbnailImage, IImageFormat format);

    public record ThumbnailInfo(int width, int height, string s3key, string s3Bucket);
}