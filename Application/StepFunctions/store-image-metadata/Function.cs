using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Util;
using Common;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace store_image_metadata
{
    public class Function
    {
        private const string PHOTO_TABLE = "PHOTO_TABLE";
        private static readonly IAmazonDynamoDB _ddbClient = new AmazonDynamoDBClient();
        private readonly DynamoDBContext _ddbContext;

        public Function()
        {
            AWSConfigsDynamoDB.Context
                .AddMapping(new TypeMapping(typeof(Photo), Environment.GetEnvironmentVariable(PHOTO_TABLE)));

            _ddbContext = new DynamoDBContext(_ddbClient);
        }

        /// <summary>
        ///     A simple function that takes a string and returns both the upper and lower case version of the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(InputEvent input, ILambdaContext context)
        {
            var thumbnail = JsonSerializer.Deserialize<Thumbnail>(JsonSerializer.Serialize(input.ParallelResults[1]));

            var labels = JsonSerializer.Deserialize<List<Label>>(JsonSerializer.Serialize(input.ParallelResults[0]));

            var photoUpdate = new Photo
            {
                PhotoId = WebUtility.UrlDecode(input.PhotoId),
                ProcessingStatus = ProcessingStatus.Succeeded,
                FullSize = new PhotoImage
                {
                    Key = WebUtility.UrlDecode(input.SourceKey),
                    Width = input.ExtractedMetadata?.Dimensions?.Width,
                    Height = input.ExtractedMetadata?.Dimensions?.Height
                },
                Format = input.ExtractedMetadata?.Format,
                ExifMake = input.ExtractedMetadata?.ExifMake,
                ExifModel = input.ExtractedMetadata?.ExifModel,
                Thumbnail = new PhotoImage
                {
                    Key = WebUtility.UrlDecode(thumbnail?.s3key),
                    Width = thumbnail?.width,
                    Height = thumbnail?.height
                },
                ObjectDetected = labels.Select(l => l.Name).ToArray(),
                GeoLocation = input.ExtractedMetadata?.Geo,
                UpdatedDate = DateTime.UtcNow
            };

            // update photo table.
            await _ddbContext.SaveAsync(photoUpdate).ConfigureAwait(false);

            var data = JsonSerializer.Serialize(photoUpdate);

            context.Logger.LogLine("Photo recognition metadata stored succesfully");

            Console.WriteLine(data);
        }
    }
}