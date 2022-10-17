using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Amazon.Runtime;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace ImageRecognition.Communication.Functions
{
    public class Functions
    {
        private const string AUTHORIZATION_HEADER = "Authorization";
        private const string BEARER_PREFIX = "Bearer";
        private const string TABLE_NAME_ENV = "COMMUNICATION_TABLE";
        private TokenValidationParameters _jwtValidationParameters;

        private readonly CommunicationManager _manager;

        /// <summary>
        ///     Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            _manager = CommunicationManager.CreateManager(Environment.GetEnvironmentVariable(TABLE_NAME_ENV));
        }


        /// <summary>
        ///     Verify JWT token in Authorization header and if valid allow connection.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> OnConnect(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (_jwtValidationParameters == null)
                _jwtValidationParameters = await CreateTokenValidationParameters(context);

            try
            {
                var username = ValidateAndGetUsername(request, context);


                var domainName = request.RequestContext.DomainName;
                var stage = request.RequestContext.Stage;
                var endpoint = $"https://{domainName}/{stage}";

                if (string.IsNullOrEmpty(username))
                {
                    context.Logger.LogLine("Error, no username claim found in JWT token");
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int) HttpStatusCode.Unauthorized
                    };
                }

                context.Logger.LogLine(
                    $"Login with connection id: {request.RequestContext.ConnectionId}, Endpoint: {endpoint}, Username: {username}");
                await _manager.LoginAsync(request.RequestContext.ConnectionId, endpoint, username);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.OK
                };
            }
            catch
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.Unauthorized
                };
            }
        }


        public async Task<APIGatewayProxyResponse> OnDisconnect(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine($"Logoff with connection id: {request.RequestContext.ConnectionId}");
            await _manager.LogoffAsync(request.RequestContext.ConnectionId);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK
            };

            return response;
        }

        public string ValidateAndGetUsername(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string authorization;
            if (!request.QueryStringParameters.TryGetValue(AUTHORIZATION_HEADER, out authorization))
            {
                context.Logger.LogLine("Error, no Authorization header found");
                throw new Exception("Error, no Authorization header found");
            }

            if (authorization.StartsWith(BEARER_PREFIX, StringComparison.OrdinalIgnoreCase))
                authorization = authorization.Substring(BEARER_PREFIX.Length + 1);

            ClaimsPrincipal user;
            try
            {
                SecurityToken validatedToken;
                user = new JwtSecurityTokenHandler().ValidateToken(authorization, _jwtValidationParameters,
                    out validatedToken);

                if (DateTime.UtcNow < validatedToken.ValidFrom || validatedToken.ValidTo < DateTime.UtcNow)
                {
                    Console.WriteLine(
                        $"Error, JWT Token expired. Token was valid from {validatedToken.ValidFrom} to {validatedToken.ValidTo}");
                    throw new Exception("JWT Token expired");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error validating JWT token: {e.Message}");
                throw;
            }

            return user.FindFirst("username")?.Value;
        }


        private async Task<TokenValidationParameters> CreateTokenValidationParameters(ILambdaContext context)
        {
            context.Logger.LogLine("Loading user pool configuration from SSM Parameter Store.");
            var configuration = new ConfigurationBuilder()
                .AddSystemsManager("/ImageRecognition")
                .Build();

            var region = configuration["AWS:Region"];
            if (string.IsNullOrEmpty(region)) region = FallbackRegionFactory.GetRegionEndpoint().SystemName;
            var userPoolId = configuration["AWS:UserPoolId"];
            var userPoolClientId = configuration["AWS:UserPoolClientId"];

            context.Logger.LogLine("Configuring JWT Validation parameters");

            var openIdConfigurationUrl =
                $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}/.well-known/openid-configuration";
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(openIdConfigurationUrl,
                new OpenIdConnectConfigurationRetriever());

            context.Logger.LogLine($"Loading open id configuration from {openIdConfigurationUrl}");
            var openIdConfig = await configurationManager.GetConfigurationAsync();


            var validIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
            context.Logger.LogLine($"Valid Issuer: {validIssuer}");
            context.Logger.LogLine($"Valid Audiences: {userPoolClientId}");

            return new TokenValidationParameters
            {
                ValidIssuer = validIssuer,
                ValidateAudience = false,
                ValidAudiences = new[] {userPoolClientId},
                IssuerSigningKeys = openIdConfig.SigningKeys
            };
        }
    }
}