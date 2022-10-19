using Amazon.Lambda.APIGatewayEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace metadata
{
    [JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
    [JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
    [JsonSerializable(typeof(OpenIdConfiguration))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(string))]
    public partial class CustomJsonSerializerContext : JsonSerializerContext
    {
    }
}
