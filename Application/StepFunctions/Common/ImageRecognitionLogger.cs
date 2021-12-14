using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Common
{
    public class ImageRecognitionLogger
    {
        [Flags]
        public enum Target
        {
            CloudWatchLogs = 1,
            Client = 2,
            All = 0xFFFFFFF
        }

        private readonly ILambdaContext _context;
        private readonly ExecutionInput _input;
        private readonly ILambdaLogger _lambdaLogger;
        private readonly CommunicationManager _manager;


        public ImageRecognitionLogger(ExecutionInput input, ILambdaContext context)
        {
            _context = context;
            _lambdaLogger = _context?.Logger;
            _input = input;

            try
            {
                var connectionTable = Environment.GetEnvironmentVariable("COMMUNICATION_CONNECTION_TABLE");
                context.Logger.LogLine($"Configuring CommunicationManager to use connection table '{connectionTable}'");
                _manager = CommunicationManager.CreateManager(connectionTable);
            }
            catch (Exception e)
            {
                _lambdaLogger?.LogLine($"Communication manager failed to initialize: {e.Message}");
            }
        }

        public async Task WriteMessageAsync(string message, Target visibility)
        {
            var evnt = new MessageEvent {Message = message};
            await WriteMessageAsync(evnt, visibility);
        }

        public async Task WriteMessageAsync(MessageEvent evnt, Target visibility)
        {
            if ((visibility & Target.CloudWatchLogs) == Target.CloudWatchLogs)
                _lambdaLogger?.LogLine($"{_context.AwsRequestId}: {evnt.Message}");

            if (_manager != null && (visibility & Target.Client) == Target.Client)
            {
                evnt.TargetUser = _input.UserId;
                evnt.ResourceId = _input.PhotoId;

                await _manager.SendMessage(evnt);
            }
        }

        public void WriteMessage(string message, Target visibility)
        {
            if ((visibility & Target.CloudWatchLogs) == Target.CloudWatchLogs)
                _lambdaLogger?.LogLine($"{_context.AwsRequestId}: {message}");

            if (_manager != null && (visibility & Target.Client) == Target.Client)
            {
                var evnt = new MessageEvent(_input.UserId, _input.SourceKey) {Message = message};
                _manager.SendMessage(evnt).GetAwaiter().GetResult();
            }
        }
    }
}