using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http;
using System.Text.Json;

namespace authorize;

public class Function
{

    private static string authDomainPrefix = Environment.GetEnvironmentVariable("AUTH_DOMAIN_PREFIX") ?? string.Empty;
    private static string region = Environment.GetEnvironmentVariable("AWS_REGION") ?? string.Empty;

    /// <summary>
    /// The main entry point for the custom runtime.
    /// </summary>
    /// <param name="args"></param>
    static Function()
    {
        AWSSDKHandler.RegisterXRayForAllServices();
    }


    /// <summary>
    /// The main entry point for the custom runtime.
    /// </summary>
    /// <param name="args"></param>
    private static async Task Main(string[] args)
    {
        Func<APIGatewayHttpApiV2ProxyRequest, ILambdaContext, APIGatewayHttpApiV2ProxyResponse> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<CustomJsonSerializerContext>(options => {
            options.PropertyNameCaseInsensitive = true;
        }))
            .Build()
            .RunAsync();
    }

    public static APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent, ILambdaContext context)
    {
        string locationUrl;

        if (IsSilentAuth(apigProxyEvent))
        {
            locationUrl = $"{apigProxyEvent.QueryStringParameters["redirect_uri"]}#state={apigProxyEvent.QueryStringParameters["state"]}&error_subtype=access_denied&error=interaction_required";
        }
        else {
            apigProxyEvent.QueryStringParameters["response_type"] = "token";
            locationUrl = QueryHelpers.AddQueryString($"https://{authDomainPrefix}.auth.{region}.amazoncognito.com/oauth2{apigProxyEvent.RawPath}", apigProxyEvent.QueryStringParameters);
        }

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 302,
            Headers = new Dictionary<string, string> { { "Location", locationUrl }, { "Access-Control-Allow-Origin", "*" } }
        };
    }

    private static bool IsSilentAuth(APIGatewayHttpApiV2ProxyRequest apigProxyEvent)
    {
        return apigProxyEvent.QueryStringParameters["prompt"] == "none";
    }
}