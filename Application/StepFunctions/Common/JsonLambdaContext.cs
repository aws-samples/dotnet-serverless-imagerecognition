using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common
{
    
    [JsonSerializable(typeof(ExecutionInput))]
    public partial class JsonLambdaContext : JsonSerializerContext
    {

    }
}
