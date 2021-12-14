using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ImageRecognition.BlazorFrontend
{
    public interface ICommunicationClientFactory
    {
        Task<ICommunicationClient> CreateCommunicationClient(CancellationToken token);
    }

    public class CommunicationClientFactory : ICommunicationClientFactory
    {
        private readonly AppOptions _appOptions;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly CognitoUserManager<CognitoUser> _cognitoUserManager;

        public CommunicationClientFactory(IOptions<AppOptions> appOptions,
            AuthenticationStateProvider authenticationStateProvider, UserManager<CognitoUser> userManager)
        {
            _appOptions = appOptions.Value;

            _authenticationStateProvider = authenticationStateProvider;
            _cognitoUserManager = userManager as CognitoUserManager<CognitoUser>;
        }

        public async Task<ICommunicationClient> CreateCommunicationClient(CancellationToken token)
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


            var cws = new ClientWebSocket();
            cws.Options.SetRequestHeader("Authorization", cognitoUser.SessionTokens.IdToken);
            await cws.ConnectAsync(new Uri(_appOptions.ImageRecognitionWebSocketAPI), token);

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
                var evnt = JsonConvert.DeserializeObject<MessageEvent>(content);
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