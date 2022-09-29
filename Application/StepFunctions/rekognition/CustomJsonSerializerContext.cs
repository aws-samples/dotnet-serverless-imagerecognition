using System.Collections.Generic;
using System.Text.Json.Serialization;
using Amazon.Rekognition.Model;
using Common;

namespace rekognition
{
    [JsonSerializable(typeof(List<Label>))]
    [JsonSerializable(typeof(Label))]
    [JsonSerializable(typeof(ExecutionInput))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(System.String))]
    public partial class CustomJsonSerializerContext : JsonSerializerContext
    {
    }
}
