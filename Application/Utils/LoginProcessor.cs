using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace CognitoLogin
{
    public class LoginProcessor
    {
        private readonly ILogger<LoginProcessor> _logger;
        private readonly UserPoolOptions _userPoolOptions;

        public LoginProcessor(IOptions<UserPoolOptions> userPoolOption, ILogger<LoginProcessor> logger)
        {
            _logger = logger;
            this._userPoolOptions = userPoolOption.Value;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.GetBySystemName(this._userPoolOptions.Region));
                var userPool = new CognitoUserPool(this._userPoolOptions.UserPoolId, this._userPoolOptions.UserPoolClientId, provider, this._userPoolOptions.UserPoolClientSecret);

                var username = ConsoleUtilties.Prompt("Enter user name:", false);
                var password = ConsoleUtilties.Prompt("Enter password:", true);

                var user = userPool.GetUser(username);

                AuthFlowResponse authResponse = await user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest()
                {
                    Password = password
                });

                while (!string.IsNullOrEmpty(authResponse.ChallengeName))
                {
                    if (authResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
                    {
                        password = ConsoleUtilties.PromptForNewPassword();
                        authResponse = await user.RespondToNewPasswordRequiredAsync(new RespondToNewPasswordRequiredRequest
                        {
                            NewPassword = password,
                            SessionID = authResponse.SessionID
                        });
                    }
                }

                Console.WriteLine($"Login successful for {username}");

                Console.WriteLine($"User id token:{Environment.NewLine}{user.SessionTokens.IdToken}");

                var jwtHandler = new JwtSecurityTokenHandler();
                var jsonToken = jwtHandler.ReadJwtToken(user.SessionTokens.IdToken);

                Console.WriteLine("\nClaims in id token:");
                foreach(var claim in jsonToken.Claims)
                {
                    Console.WriteLine($"\t{claim.Type}: {claim.Value}");
                }

            }
            catch(Amazon.CognitoIdentityProvider.AmazonCognitoIdentityProviderException e)
            {
                Console.Error.WriteLine($"Error logging into Cognito: {e.Message}");
            }
            catch(Exception e)
            {
                Console.WriteLine($"Unknown error logging into Cognito: {e.Message}");
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
