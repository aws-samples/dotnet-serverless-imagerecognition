using System;

namespace ImageRecognition.API.Models
{
    public class Album
    {
        public string AlbumId { get; set; }

        public string UserId { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}