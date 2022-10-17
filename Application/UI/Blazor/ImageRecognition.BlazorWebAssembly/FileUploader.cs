using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImageRecognition.BlazorWebAssembly
{
    public interface IFileUploader
    {
        public Task<string> UploadFileAsync(Stream stream, string albumId, string photoName);
    }

    public class FileUploader : IFileUploader
    {
        private const int PART_SIZE = 6 * 1024 * 1024;
        private const int READ_BUFFER_SIZE = 20000;
        private readonly AppOptions _appOptions;
        private readonly ILogger<FileUploader> _logger;
        private readonly IServiceClientFactory _serviceClientFactory;

        public FileUploader(IOptions<AppOptions> appOptions, ILogger<FileUploader> logger, IServiceClientFactory ServiceClientFactory)
        {
            _appOptions = appOptions.Value;
            _logger = logger;
            _serviceClientFactory = ServiceClientFactory;
        }

        public async Task<string> UploadFileAsync(Stream stream, string albumId, string photoName)
        {
            var objectKey = $"{photoName}-{Guid.NewGuid()}";

            _logger.LogInformation($"Start uploading to {objectKey}");

            try
            {
                using var inputStream = stream;
                var photoClient = await _serviceClientFactory.CreatePhotoClient();

                string preSignedUrl = await photoClient.AddPhotoAsync(albumId, objectKey);

                var httpClient = _serviceClientFactory.CreateHttpClient();
                HttpContent content = new StreamContent(stream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");
                
                var response = await httpClient.PutAsync(preSignedUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    return preSignedUrl;
                }

                return string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }

    public class UploadEvent
    {
        public long UploadBytes { get; set; }
        public int UploadParts { get; set; }
    }
}