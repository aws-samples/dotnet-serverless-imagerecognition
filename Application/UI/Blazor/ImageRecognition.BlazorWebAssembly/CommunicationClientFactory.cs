using System;
using System.Buffers;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Text.Json;

namespace ImageRecognition.BlazorWebAssembly
{
    public interface ICommunicationClientFactory
    {
        Task<ICommunicationClient> CreateCommunicationClient(CancellationToken cancellationToken);
    }

    public class CommunicationClientFactory : ICommunicationClientFactory
    {
        private readonly AppOptions _appOptions;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IAccessTokenProvider _tokenProvider;

        public CommunicationClientFactory(AppOptions appOptions,
            AuthenticationStateProvider authenticationStateProvider, IAccessTokenProvider tokenProvider)
        {
            _appOptions = appOptions;
            
            _authenticationStateProvider = authenticationStateProvider;
            _tokenProvider = tokenProvider;
        }

        public async Task<ICommunicationClient> CreateCommunicationClient(CancellationToken cancellationToken)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (!user.Identity.IsAuthenticated)
                throw new Exception();

            var accessTokenResult = await _tokenProvider.RequestAccessToken();
            accessTokenResult.TryGetToken(out var accessToken);

            var cws = new ClientWebSocket();
            
            UriBuilder baseUri = new UriBuilder(_appOptions.ImageRecognitionWebSocketAPI);
            baseUri.Query = "Authorization=Bearer " + accessToken.Value;

            //cws.Options.SetRequestHeader("Authorization", "Bearer " + accessToken.Value);
            await cws.ConnectAsync(baseUri.Uri, new CancellationToken());

            return new CommunicationClient(cws);
        }
    }


    public interface ICommunicationClient : IDisposable
    {
        Task<MessageEvent> ReadEventAsync(CancellationToken token);
    }

    public class CommunicationClient : ICommunicationClient
    {
        private readonly byte[] _buffer;
        private readonly ClientWebSocket _cws;
        private readonly Memory<byte> _memoryBlock;


        public CommunicationClient(ClientWebSocket cws)
        {
            _cws = cws;

            _buffer = ArrayPool<byte>.Shared.Rent(65536);
            _memoryBlock = new Memory<byte>(_buffer);
        }

        public async Task<MessageEvent> ReadEventAsync(CancellationToken token)
        {
            try
            {
                var recvResult = await _cws.ReceiveAsync(_memoryBlock, token);

                if (WebSocketMessageType.Text != recvResult.MessageType) return null;

                var content = Encoding.UTF8.GetString(_buffer, 0, recvResult.Count);
                var evnt = JsonSerializer.Deserialize<MessageEvent>(content);
                return evnt;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ArrayPool<byte>.Shared.Return(_buffer);
                    _cws.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}