using System;
using Common;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace extract_image_metadata
{
    public class ImageMetadata : ExecutionInput
    {
        public long Id { get; set; }

        public string Format { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public decimal OriginalImagePixelCount { get; set; }

        public long Size { get; set; }

        //public IReadOnlyCollection<IExifValue> ExifProfile { get; set; }

        public string ExifProfileBase64 =>
            ExifProfile != null ? Convert.ToBase64String(ExifProfile.ToByteArray()) : string.Empty;

        public ExifProfile ExifProfile { get; set; }
    }
}