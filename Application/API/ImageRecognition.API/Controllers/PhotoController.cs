using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Model;
using ImageRecognition.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ImageRecognition.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PhotoController : Controller
    {
        private readonly AppOptions _appOptions;
        private readonly IAmazonDynamoDB _ddbClient;
        private readonly DynamoDBContext _ddbContext;
        private readonly IAmazonS3 _s3Client;

        public PhotoController(IOptions<AppOptions> appOptions, IAmazonDynamoDB dbClient, IAmazonS3 s3Client)
        {
            _appOptions = appOptions.Value;
            _ddbClient = dbClient;
            _s3Client = s3Client;
            _ddbContext = new DynamoDBContext(_ddbClient);
        }

        /// <summary>
        ///     Gets all photos by album Id.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Photo[]))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<JsonResult> GetPhotosByAlbum(string albumId)
        {
            var userId = Utilities.GetUsername(HttpContext.User);

            var photos = new List<Photo>();

            var photoQuery = _ddbContext.QueryAsync<Photo>(albumId,
                new DynamoDBOperationConfig { IndexName = "albumID-uploadTime-index" });
            foreach (var photo in await photoQuery.GetRemainingAsync().ConfigureAwait(false))
            {
                if (photo.Owner != userId) continue;

                photo.Thumbnail ??= new PhotoImage();
                photo.FullSize ??= new PhotoImage();

                photo.Thumbnail.Url = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = _appOptions.PhotoStorageBucket,
                    Key = $"private/resized/{userId}/{photo.PhotoId}",
                    Expires = DateTime.UtcNow.AddHours(1)
                });
                photo.FullSize.Url = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = _appOptions.PhotoStorageBucket,
                    Key = $"private/uploads/{userId}/{photo.PhotoId}",
                    Expires = DateTime.UtcNow.AddHours(1)
                });

                photos.Add(photo);
            }

            return new JsonResult(photos);
        }

        /// <summary>
        ///     Add photo to album.
        /// </summary>
        /// <param name="albumId">The album id to use to add the photo.</param>
        /// <param name="name">photo name.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<JsonResult> AddPhoto([FromQuery] string albumId, [FromQuery] string photoIdKey)
        {
            var userId = Utilities.GetUsername(HttpContext.User);

            var photoId = photoIdKey;

            var photo = new Photo
            {
                AlbumId = albumId,
                Bucket = _appOptions.PhotoStorageBucket,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                PhotoId = photoId,
                Owner = userId,
                ProcessingStatus = ProcessingStatus.Pending,
                UploadTime = DateTime.UtcNow
            };

            await _ddbContext.SaveAsync(photo).ConfigureAwait(false);

            string preSignedUrl = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _appOptions.PhotoStorageBucket,
                Key = $"private/uploads/{userId}/{photoId}",
                Verb = HttpVerb.PUT,
                ContentType = "image/jpg",
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return new JsonResult(preSignedUrl);
        }
    }
}