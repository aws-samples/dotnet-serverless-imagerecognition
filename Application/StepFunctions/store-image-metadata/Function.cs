using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Common;

namespace store_image_metadata
{
    public class Function
    {
        private static readonly string PHOTO_TABLE = Environment.GetEnvironmentVariable("PHOTO_TABLE") ?? string.Empty;
        private static readonly IAmazonDynamoDB _ddbClient = new AmazonDynamoDBClient();

        static Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
        }

        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
        /// <param name="args"></param>
        private static async Task Main()
        {
            Func<InputEvent, ILambdaContext, Task> handler = FunctionHandler;
            await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<CustomJsonSerializerContext>(options =>
            {
                options.PropertyNameCaseInsensitive = true;
            }))
                .Build()
                .RunAsync();
        }


        /// <summary>
        ///     A simple function that takes a string and returns both the upper and lower case version of the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task FunctionHandler(InputEvent input, ILambdaContext context)
        {
            var logger = new ImageRecognitionLogger(input, context);

            var thumbnail = JsonSerializer.Deserialize(JsonSerializer.Serialize(input.ParallelResults[1]),
                CustomJsonSerializerContext.Default.Thumbnail);

            var labels = JsonSerializer.Deserialize(JsonSerializer.Serialize(input.ParallelResults[0]),
                CustomJsonSerializerContext.Default.ListLabel);


            var photo = new Photo
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
                ExifMake = input.ExtractedMetadata?.ExifMake ?? string.Empty,
                ExifModel = input.ExtractedMetadata?.ExifModel ?? string.Empty,
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
            int status = (int)photo.ProcessingStatus;

            var request = new UpdateItemRequest
            {
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "PhotoId", new AttributeValue { S = photo.PhotoId } }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":status",new AttributeValue { S = status.ToString() }},
                    {":date",new AttributeValue { S = DateTime.UtcNow.ToString()}},
                    {":objects",new AttributeValue { SS = photo.ObjectDetected.ToList() }},
                    {":thumb",new AttributeValue { M = ToDynamoAttributes(photo.Thumbnail) } },
                    {":full",new AttributeValue { M = ToDynamoAttributes(photo.FullSize) }},
                    {":make",new AttributeValue { S = photo.ExifMake}},
                    {":model",new AttributeValue { S = photo.ExifModel }},
                },

                UpdateExpression = "SET ProcessingStatus = :status, UpdatedDate = :date, ObjectDetected = :objects, Thumbnail = :thumb, FullSize = :full, ExifMake = :make, ExifModel = :model",

                TableName = PHOTO_TABLE
            };

            Console.WriteLine(request.UpdateExpression);

            var data = JsonSerializer.Serialize(photo, CustomJsonSerializerContext.Default.Photo);

            Console.WriteLine(data);

            await _ddbClient.UpdateItemAsync(request);

            await logger.WriteMessageAsync(
                new MessageEvent
                { Message = "Photo recognition metadata stored succesfully", Data = data, CompleteEvent = true },
                ImageRecognitionLogger.Target.All);
        }

        private static Dictionary<string, AttributeValue> ToDynamoAttributes(PhotoImage photoImage)
        {
            Dictionary<String, AttributeValue> item = new Dictionary<string, AttributeValue>();
            item.Add("Key", new AttributeValue { S = photoImage.Key });
            if (photoImage.Width != null)
            {
                item.Add("Width", new AttributeValue { N = photoImage.Width.ToString() });
            }
            if (photoImage.Height != null)
            {
                item.Add("Height", new AttributeValue { N = photoImage.Height.ToString() });
            }
            return item;
        }
    }
}