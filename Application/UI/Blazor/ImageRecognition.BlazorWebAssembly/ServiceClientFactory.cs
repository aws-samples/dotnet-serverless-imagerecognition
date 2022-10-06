using System;
using System.Net.Http;
using System.Net.Http.Headers;
using ImageRecognition.API.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;

namespace ImageRecognition.BlazorWebAssembly
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
        private readonly IAccessTokenProvider _tokenProvider;

        public ServiceClientFactory(AppOptions appOptions,
            AuthenticationStateProvider authenticationStateProvider, IAccessTokenProvider tokenProvider)
        {
            _appOptions = appOptions;

            _authenticationStateProvider = authenticationStateProvider;
            _tokenProvider = tokenProvider;
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

            var accessTokenResult = await _tokenProvider.RequestAccessToken();
            accessTokenResult.TryGetToken(out var accessToken);

            //var httpClient = new HttpClient(new HttpClientXRayTracingHandler(new HttpClientHandler()));
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse($"bearer {accessToken.Value}");

            return httpClient;
        }
    }
}