namespace ImageRecognition.BlazorWebAssembly
{
    public class AppOptions
    {
        public string ImageRecognitionApiGatewayUrl { get; set; }

        public string ImageRecognitionWebSocketAPI { get; set; }

        public string PhotoStorageBucket { get; set; }

        public string UploadBucketPrefix { get; set; }
    }
}