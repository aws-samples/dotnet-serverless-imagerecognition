using System.Collections.Generic;
using Common;

namespace store_image_metadata
{
    public class InputEvent : ExecutionInput
    {
        public ExtractedMetadata ExtractedMetadata { get; set; }

        public List<object> ParallelResults { get; set; }
    }

    public record Label (string Name);

    public record Thumbnail(decimal width, decimal height, string s3key, string s3Bucket);
}