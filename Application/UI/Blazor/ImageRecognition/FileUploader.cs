using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImageRecognition.BlazorFrontend
{
    public interface IFileUploader
    {
        public Task<string> UploadFileAsync(Stream stream, Action<UploadEvent> callback);
    }

    public class FileUploader : IFileUploader
    {
        private const int PART_SIZE = 6 * 1024 * 1024;
        private const int READ_BUFFER_SIZE = 20000;
        private readonly AppOptions _appOptions;
        private readonly ILogger<FileUploader> _logger;
        private readonly IAmazonS3 _s3Client;


        public FileUploader(IOptions<AppOptions> appOptions, ILogger<FileUploader> logger, IAmazonS3 s3Client)
        {
            _appOptions = appOptions.Value;
            _logger = logger;
            _s3Client = s3Client;
        }

        public async Task<string> UploadFileAsync(Stream stream, Action<UploadEvent> callback)
        {
            var objectKey = $"{_appOptions.UploadBucketPrefix}/{Guid.NewGuid()}";

            _logger.LogInformation($"Start uploading to {objectKey}");
            var initateResponse = await _s3Client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest
            {
                BucketName = _appOptions.PhotoStorageBucket,
                Key = objectKey
            });
            _logger.LogInformation($"Initiated multi part upload with id {initateResponse.UploadId}");
            try
            {
                using var inputStream = stream;
                var partETags = new List<PartETag>();
                var readBuffer = ArrayPool<byte>.Shared.Rent(READ_BUFFER_SIZE);
                var partBuffer = ArrayPool<byte>.Shared.Rent(PART_SIZE + READ_BUFFER_SIZE * 3);

                var callbackEvent = new UploadEvent();
                var nextUploadBuffer = new MemoryStream(partBuffer);
                try
                {
                    var partNumber = 1;
                    int readCount;
                    while ((readCount = await inputStream.ReadAsync(readBuffer, 0, readBuffer.Length)) != 0)
                    {
                        callbackEvent.UploadBytes += readCount;
                        callback?.Invoke(callbackEvent);

                        await nextUploadBuffer.WriteAsync(readBuffer, 0, readCount);

                        if (PART_SIZE < nextUploadBuffer.Position)
                        {
                            var isLastPart = readCount == READ_BUFFER_SIZE;
                            var partSize = nextUploadBuffer.Position;
                            nextUploadBuffer.Position = 0;
                            var partResponse = await _s3Client.UploadPartAsync(new UploadPartRequest
                            {
                                BucketName = _appOptions.PhotoStorageBucket,
                                Key = objectKey,
                                UploadId = initateResponse.UploadId,
                                InputStream = nextUploadBuffer,
                                PartSize = partSize,
                                PartNumber = partNumber,
                                IsLastPart = isLastPart
                            });
                            _logger.LogInformation(
                                $"Uploaded part {partNumber}. (Last part = {isLastPart}, Part size = {partSize}, Upload Id: {initateResponse.UploadId}");

                            partETags.Add(new PartETag
                                {PartNumber = partResponse.PartNumber, ETag = partResponse.ETag});
                            partNumber++;
                            nextUploadBuffer = new MemoryStream(partBuffer);

                            callbackEvent.UploadParts++;
                            callback?.Invoke(callbackEvent);
                        }
                    }


                    if (nextUploadBuffer.Position != 0)
                    {
                        var partSize = nextUploadBuffer.Position;
                        nextUploadBuffer.Position = 0;
                        var partResponse = await _s3Client.UploadPartAsync(new UploadPartRequest
                        {
                            BucketName = _appOptions.PhotoStorageBucket,
                            Key = objectKey,
                            UploadId = initateResponse.UploadId,
                            InputStream = nextUploadBuffer,
                            PartSize = partSize,
                            PartNumber = partNumber,
                            IsLastPart = true
                        });
                        _logger.LogInformation(
                            $"Uploaded final part. (Part size = {partSize}, Upload Id: {initateResponse.UploadId})");
                        partETags.Add(new PartETag {PartNumber = partResponse.PartNumber, ETag = partResponse.ETag});

                        callbackEvent.UploadParts++;
                        callback?.Invoke(callbackEvent);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(partBuffer);
                    ArrayPool<byte>.Shared.Return(readBuffer);
                }


                await _s3Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest
                {
                    BucketName = _appOptions.PhotoStorageBucket,
                    Key = objectKey,
                    UploadId = initateResponse.UploadId,
                    PartETags = partETags
                });
                _logger.LogInformation(
                    $"Completed multi part upload. (Part count: {partETags.Count}, Upload Id: {initateResponse.UploadId})");
            }
            catch (Exception e)
            {
                await _s3Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest
                {
                    BucketName = _appOptions.PhotoStorageBucket,
                    Key = objectKey,
                    UploadId = initateResponse.UploadId
                });
                _logger.LogError($"Error uploading to S3 with error: {e.Message}");

                throw;
            }

            return _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _appOptions.PhotoStorageBucket,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddDays(1)
            });
        }
    }

    public class UploadEvent
    {
        public long UploadBytes { get; set; }
        public int UploadParts { get; set; }
    }
}