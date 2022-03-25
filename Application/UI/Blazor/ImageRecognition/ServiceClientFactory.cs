using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.XRay.Recorder.Handlers.System.Net;
using ImageRecognition.API.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ImageRecognition.BlazorFrontend
{
    public interface IServiceClientFactory
    {
        public Task<AlbumClient> CreateAlbumClient();

        public Task<PhotoClient> CreatePhotoClient();
    }

    public class ServiceClientFactory : IServiceClientFactory
    {
        private readonly AppOptions _appOptions;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly CognitoUserManager<CognitoUser> _cognitoUserManager;

        public ServiceClientFactory(IOptions<AppOptions> appOptions,
            AuthenticationStateProvider authenticationStateProvider, UserManager<CognitoUser> userManager)
        {
            _appOptions = appOptions.Value;

            _authenticationStateProvider = authenticationStateProvider;
            _cognitoUserManager = userManager as CognitoUserManager<CognitoUser>;
        }

        public async Task<AlbumClient> CreateAlbumClient()
        {
            var httpClient = await ConstructHttpClient();
            var albumClient = new AlbumClient(httpClient)
            {
                BaseUrl = _appOptions.ImageRecognitionApiUrl
            };


            return albumClient;
        }


        public async Task<PhotoClient> CreatePhotoClient()
        {
            var httpClient = await ConstructHttpClient();
            var photoClient = new PhotoClient(httpClient)
            {
                BaseUrl = _appOptions.ImageRecognitionApiUrl
            };

            return photoClient;
        }


        private async Task<HttpClient> ConstructHttpClient()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (!user.Identity.IsAuthenticated)
                throw new Exception();

            var userId = _cognitoUserManager.GetUserId(user);
            if (string.IsNullOrEmpty(userId))
                throw new Exception();

            var cognitoUser = await _cognitoUserManager.FindByIdAsync(userId);
            if (string.IsNullOrEmpty(cognitoUser?.SessionTokens.IdToken))
                throw new Exception();


            var httpClient = new HttpClient(new HttpClientXRayTracingHandler(new HttpClientHandler()));
            httpClient.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse($"bearer {cognitoUser.SessionTokens.IdToken}");


            return httpClient;
        }
    }
}