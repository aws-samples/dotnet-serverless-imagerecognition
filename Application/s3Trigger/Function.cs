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

namespace s3Trigger
{
    public class Function
    {
        private const string STATE_MACHINE_ARN = "STATE_MACHINE_ARN";
        private const string PHOTO_TABLE = "PHOTO_TABLE";

        private static readonly IAmazonDynamoDB _ddbClient = new AmazonDynamoDBClient();
        private static readonly IAmazonStepFunctions _stepClient = new AmazonStepFunctionsClient();

        private static DynamoDBContext _ddbContext;

        private static string _stateMachineArn;

        static Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();

            _stateMachineArn = Environment.GetEnvironmentVariable(STATE_MACHINE_ARN) ?? string.Empty;

            _ddbContext = new DynamoDBContext(_ddbClient);
        }

        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
        /// <param name="args"></param>
        private static async Task Main()
        {
            AWSConfigsDynamoDB.Context
                .AddMapping(new TypeMapping(typeof(Photo), Environment.GetEnvironmentVariable(PHOTO_TABLE)));

            Func<S3Event, ILambdaContext, Task> handler = FunctionHandler;
            await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<CustomJsonSerializerContext>(options => {
                    options.PropertyNameCaseInsensitive = true;
                }))
                .Build()
                .RunAsync();
        }

        public static async Task FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            var bucket = evnt.Records[0].S3.Bucket.Name;
            var key = WebUtility.UrlDecode(evnt.Records[0].S3.Object.Key);

            Console.WriteLine(bucket);
            Console.WriteLine(key);

            var photoData = key.Split("/").Reverse().Take(2).ToArray();

            var photoId = photoData[0];
            var userId = photoData[1];

            Console.WriteLine(photoId);

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
                StateMachineArn = _stateMachineArn,
                Name = $"{MakeSafeName(key, 80)}",
                Input = JsonSerializer.Serialize(input, CustomJsonSerializerContext.Default.SfnInput)
            }).ConfigureAwait(false);

            var photo = new Photo
            {
                PhotoId = photoId,
                SfnExecutionArn = stepResponse.ExecutionArn,
                ProcessingStatus = ProcessingStatus.Running,
                UpdatedDate = DateTime.UtcNow
            };

            await _ddbContext.SaveAsync(photo).ConfigureAwait(false);
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

    [JsonSerializable(typeof(S3Event))]
    [JsonSerializable(typeof(SfnInput))]
    [JsonSerializable(typeof(Photo))]
    [JsonSerializable(typeof(DateTime?))]
    [JsonSerializable(typeof(ProcessingStatus))]
    [JsonSerializable(typeof(string))]
    public partial class CustomJsonSerializerContext : JsonSerializerContext
    {
        // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
        // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for
        // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
    }
}