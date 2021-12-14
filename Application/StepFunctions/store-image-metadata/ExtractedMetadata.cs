using System;
using Common;

namespace store_image_metadata
{
    public class ExtractedMetadata
    {
        public GeoLocation Geo { get; set; }

        public string ExifMake { get; set; }

        public string ExifModel { get; set; }

        public Dimensions Dimensions { get; set; }

        public long FileSize { get; set; }

        public string Format { get; set; }

        public DateTime CreationTime { get; set; }
    }
}