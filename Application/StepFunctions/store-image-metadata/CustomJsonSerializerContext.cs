using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common;

namespace store_image_metadata
{
    [JsonSerializable(typeof(ExtractedMetadata))]
    [JsonSerializable(typeof(InputEvent))]
    [JsonSerializable(typeof(Photo))]
    [JsonSerializable(typeof(PhotoImage))]
    [JsonSerializable(typeof(List<Label>))]
    [JsonSerializable(typeof(Thumbnail))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(System.String))]
    public partial class CustomJsonSerializerContext : JsonSerializerContext
    {
        // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
        // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for
        // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
    }
}
