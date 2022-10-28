using System.Text.Json.Serialization;
using Common;

namespace extract_image_metadata
{
    [JsonSerializable(typeof(ImageMetadata))]
    [JsonSerializable(typeof(ExecutionInput))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(System.String))]
    public partial class CustomJsonSerializerContext : JsonSerializerContext
    {
    }
}
