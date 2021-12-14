namespace ImageRecognition.API
{
    public class AppOptions
    {
        public string TableAlbum { get; set; }

        public string TablePhoto { get; set; }

        public string PhotoStorageBucket { get; set; }

        public string StateMachineArn { get; set; }
    }
}