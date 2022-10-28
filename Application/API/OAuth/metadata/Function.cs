using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Transform;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using System.Text.Json;

namespace metadata;

public class Function
{

    private static readonly string UserPoolId = Environment.GetEnvironmentVariable("USER_POOL_ID") ?? string.Empty;
    private static string region = Environment.GetEnvironmentVariable("AWS_REGION") ?? string.Empty;

    private static HttpClient httpClient { get; }

    static Function()
    {
        AWSSDKHandler.RegisterXRayForAllServices();
        httpClient = new HttpClient();
    }


    /// <summary>
    /// The main entry point for the custom runtime.
    /// </summary>
    /// <param name="args"></param>
    private static async Task Main(string[] args)
    {
        Func<APIGatewayHttpApiV2ProxyRequest, ILambdaContext, Task<APIGatewayHttpApiV2ProxyResponse>> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<CustomJsonSerializerContext>(options => {
            options.PropertyNameCaseInsensitive = true;
        }))
            .Build()
            .RunAsync();
    }

    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent, ILambdaContext context)
    {
        if (string.IsNullOrEmpty(region)) region = FallbackRegionFactory.GetRegionEndpoint().SystemName;
        var cognitoMetadataUrl = $"https://cognito-idp.{region}.amazonaws.com/{UserPoolId}/.well-known/openid-configuration";

        var response = await httpClient.GetStringAsync(cognitoMetadataUrl);

        var config = JsonSerializer.Deserialize(response, CustomJsonSerializerContext.Default.OpenIdConfiguration);

        config.authorization_endpoint = $"https://{apigProxyEvent.RequestContext.DomainName}/authorize";

        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = JsonSerializer.Serialize(config, CustomJsonSerializerContext.Default.OpenIdConfiguration),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" }, { "Access-Control-Allow-Origin" , "*" } }
        };

    }
}