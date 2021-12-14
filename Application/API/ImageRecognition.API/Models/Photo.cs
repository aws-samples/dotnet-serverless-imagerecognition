using System;

namespace ImageRecognition.API.Models
{
    public enum ProcessingStatus
    {
        Pending = 0,
        Running = 1,
        Succeeded = 2,
        Failed = 3,
        Timed_Out = 4,
        Aborted = 5
    }

    public class Photo
    {
        public string PhotoId { get; set; }

        public ProcessingStatus ProcessingStatus { get; set; }

        public string SfnExecutionArn { get; set; }

        public string AlbumId { get; set; }

        public string Bucket { get; set; }

        public PhotoImage FullSize { get; set; }

        public PhotoImage Thumbnail { get; set; }

        public string[] ObjectDetected { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string Format { get; set; }

        public string Owner { get; set; }

        public string ExifMake { get; set; }

        public string ExifModel { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public DateTime? UploadTime { get; set; }

        public GeoLocation GeoLocation { get; set; }
    }
}