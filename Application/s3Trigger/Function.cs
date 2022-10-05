using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Amazon.Util;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.Lambda.RuntimeSupport;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;

namespace s3Trigger
{
    public class Function
    {
        private static readonly string STATE_MACHINE_ARN = Environment.GetEnvironmentVariable("STATE_MACHINE_ARN") ?? string.Empty;
        private static readonly string PHOTO_TABLE = Environment.GetEnvironmentVariable("PHOTO_TABLE") ?? string.Empty;
        private static readonly IAmazonDynamoDB _ddbClient = new AmazonDynamoDBClient();
        private static readonly IAmazonStepFunctions _stepClient = new AmazonStepFunctionsClient();

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
            Func<S3Event, ILambdaContext, Task> handler = FunctionHandler;
            await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<CustomJsonSerializerContext>(options =>
            {
                options.PropertyNameCaseInsensitive = true;
            }))
                .Build()
                .RunAsync();
        }

        public static async Task FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            var bucket = evnt.Records[0].S3.Bucket.Name;
            var key = WebUtility.UrlDecode(evnt.Records[0].S3.Object.Key);

            Console.WriteLine($"Bucket: {bucket}");
            Console.WriteLine($"key: {key}");

            var photoData = key.Split("/").Reverse().Take(2).ToArray();

            var photoId = photoData[0];
            var userId = photoData[1];

            Console.WriteLine($"Parsed photoId: {photoId}");

            var input = new SfnInput
            {
                Bucket = bucket,
                SourceKey = key,
                PhotoId = photoId,
                UserId = userId,
                TablePhoto = Environment.GetEnvironmentVariable(PHOTO_TABLE)
            };

            var stepResponse = await _stepClient.StartExecutionAsync(new StartExecutionRequest
            {
                StateMachineArn = STATE_MACHINE_ARN,
                Name = $"{MakeSafeName(key, 80)}",
                Input = JsonSerializer.Serialize(input, CustomJsonSerializerContext.Default.SfnInput)
            }).ConfigureAwait(false);

            int status = (int)ProcessingStatus.Running;

            var request = new UpdateItemRequest
            {
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "PhotoId", new AttributeValue { S = photoId } }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":sfnArn",new AttributeValue { S = stepResponse.ExecutionArn }},
                    {":status",new AttributeValue { S = status.ToString() }},
                    {":date",new AttributeValue { S = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff'Z'", CultureInfo.InvariantCulture)}},
                },

                UpdateExpression = "SET SfnExecutionArn = :sfnArn, ProcessingStatus = :status, UpdatedDate = :date",

                TableName = PHOTO_TABLE
            };

            await _ddbClient.UpdateItemAsync(request);
        }

        public static string MakeSafeName(string displayName, int maxSize)
        {
            var builder = new StringBuilder();
            foreach (var c in displayName)
                if (char.IsLetterOrDigit(c))
                    builder.Append(c);
                else
                    builder.Append('-');

            var name = builder.ToString();

            if (maxSize < name.Length) name = name.Substring(0, maxSize);

            return name;
        }

    }
}