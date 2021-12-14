using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Common;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace rekognition
{
    public class Function
    {
        /// <summary>
        ///     The default minimum confidence used for detecting labels.
        /// </summary>
        public const float DEFAULT_MIN_CONFIDENCE = 60f;

        /// <summary>
        ///     The name of the environment variable to set which will override the default minimum confidence level.
        /// </summary>
        public const string MIN_CONFIDENCE_ENVIRONMENT_VARIABLE_NAME = "MinConfidence";

        public const int MaxLabels = 10;

        public Function()
        {
            RekognitionClient = new AmazonRekognitionClient();

            var environmentMinConfidence = Environment.GetEnvironmentVariable(MIN_CONFIDENCE_ENVIRONMENT_VARIABLE_NAME);
            if (!string.IsNullOrWhiteSpace(environmentMinConfidence))
            {
                float value;
                if (float.TryParse(environmentMinConfidence, out value))
                {
                    MinConfidence = value;
                    Console.WriteLine($"Setting minimum confidence to {MinConfidence}");
                }
                else
                {
                    Console.WriteLine(
                        $"Failed to parse value {environmentMinConfidence} for minimum confidence. Reverting back to default of {MinConfidence}");
                }
            }
            else
            {
                Console.WriteLine($"Using default minimum confidence of {MinConfidence}");
            }
        }

        private IAmazonRekognition RekognitionClient { get; }

        private float MinConfidence { get; } = DEFAULT_MIN_CONFIDENCE;

        /// <summary>
        ///     A simple function that takes a string and returns both the upper and lower case version of the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<List<Label>> FunctionHandler(ExecutionInput input, ILambdaContext context)
        {
            context.Logger.LogLine($"Looking for labels in image {input.Bucket}:{input.SourceKey}");

            var key = WebUtility.UrlDecode(input.SourceKey);

            var detectResponses = await RekognitionClient.DetectLabelsAsync(new DetectLabelsRequest
            {
                MinConfidence = MinConfidence,
                MaxLabels = MaxLabels,
                Image = new Image
                {
                    S3Object = new S3Object
                    {
                        Bucket = input.Bucket,
                        Name = key
                    }
                }
            });

            context.Logger.LogLine("Photo labels extracted successfully");

            return detectResponses.Labels;
        }
    }
}