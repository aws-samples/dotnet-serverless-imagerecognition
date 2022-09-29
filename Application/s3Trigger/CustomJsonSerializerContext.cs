using Amazon.Lambda.S3Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace s3Trigger
{
    [JsonSerializable(typeof(S3Event))]
    [JsonSerializable(typeof(SfnInput))]
    [JsonSerializable(typeof(Photo))]
    [JsonSerializable(typeof(DateTime?))]
    [JsonSerializable(typeof(ProcessingStatus))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(System.String))]
    public partial class CustomJsonSerializerContext : JsonSerializerContext
    {
        // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
        // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for
        // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
    }
}
