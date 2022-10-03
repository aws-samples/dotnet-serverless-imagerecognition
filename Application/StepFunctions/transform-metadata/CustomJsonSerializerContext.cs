using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace transform_metadata
{
    [JsonSerializable(typeof(ExecutionInput))]
    [JsonSerializable(typeof(ImageMetadata))]
    [JsonSerializable(typeof(TransformedMetadata))]
    [JsonSerializable(typeof(GeoLocation))]
    [JsonSerializable(typeof(Dimensions))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(ExifProfile))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(System.String))]
    public partial class CustomJsonSerializerContext : JsonSerializerContext
    {
    }
}
