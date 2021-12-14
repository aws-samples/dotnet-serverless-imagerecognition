namespace ImageRecognition.BlazorFrontend
{
    public class AppOptions
    {
        public string ImageRecognitionApiUrl { get; set; }

        public string ImageRecognitionWebSocketAPI { get; set; }

        public string PhotoStorageBucket { get; set; }

        public string UploadBucketPrefix { get; set; }
    }
}