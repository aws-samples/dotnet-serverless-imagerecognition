using System;
using System.Collections.Generic;

namespace ImageRecognition.Frontend.Models
{
    public class Album
    {
        public string AlbumId { get; set; }

        public string UserId { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public IList<Photo> Photos { get; set; }
    }
}