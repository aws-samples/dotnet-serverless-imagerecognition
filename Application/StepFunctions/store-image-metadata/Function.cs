using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Util;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Common;

namespace store_image_metadata
{
    public class Function
    {
        private const string PHOTO_TABLE = "PHOTO_TABLE";
        private static readonly IAmazonDynamoDB _ddbClient = new AmazonDynamoDBClient();
        private static DynamoDBContext _ddbContext;

        static Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();

            _ddbContext = new DynamoDBContext(_ddbClient);

            AWSConfigsDynamoDB.Context
                .AddMapping(new TypeMapping(typeof(Photo), Environment.GetEnvironmentVariable(PHOTO_TABLE)));
        }

        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
        /// <param name="args"></param>
        private static async Task Main()
        {
            Func<InputEvent, ILambdaContext, Task> handler = FunctionHandler;
            await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<CustomJsonSerializerContext>(options => {
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
            try
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

                await _ddbClient.PutItemAsync(PHOTO_TABLE, photo.ToDynamoDBAttributes()).ConfigureAwait(false);

                var data = JsonSerializer.Serialize(photo, CustomJsonSerializerContext.Default.Photo);

                await logger.WriteMessageAsync(
                    new MessageEvent
                    { Message = "Photo recognition metadata stored succesfully", Data = data, CompleteEvent = true },
                    ImageRecognitionLogger.Target.All);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}