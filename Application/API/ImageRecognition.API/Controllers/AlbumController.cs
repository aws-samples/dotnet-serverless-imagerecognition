using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using ImageRecognition.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ImageRecognition.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumController : Controller
    {
        private AppOptions _appOptions;
        private readonly DynamoDBContext _ddbContext;
        private IAmazonS3 _s3Client;

        public AlbumController(IOptions<AppOptions> appOptions, IAmazonDynamoDB dbClient, IAmazonS3 s3Client)
        {
            _appOptions = appOptions.Value;
            _s3Client = s3Client;
            _ddbContext = new DynamoDBContext(dbClient);
        }

        /// <summary>
        ///     Get the list of albums the user has created.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Album[]))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<JsonResult> GetUserAlbums()
        {
            var userId = Utilities.GetUsername(HttpContext.User);

            var search = _ddbContext.QueryAsync<Album>(userId);

            var albums = await search.GetRemainingAsync().ConfigureAwait(false);

            return new JsonResult(albums);
        }

        /// <summary>
        ///     Get the list of albums the user has created.
        /// </summary>
        /// <param name="includePublic">If true then also include the galleries that have been marked as public.</param>
        /// <returns></returns>
        [HttpGet("{albumId}")]
        [ProducesResponseType(200, Type = typeof(Album))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<JsonResult> GetAlbumById(string albumId)
        {
            var userId = Utilities.GetUsername(HttpContext.User);

            var albumQuery = _ddbContext.QueryAsync<Album>(userId, QueryOperator.Equal, new[] {albumId});

            var album = await albumQuery.GetRemainingAsync().ConfigureAwait(false);

            return new JsonResult(album.FirstOrDefault());
        }


        /// <summary>
        ///     Create a new empty album.
        /// </summary>
        /// <param name="name">The name of the album.</param>
        /// <returns>The album id to use for adding photos to the album.</returns>
        [HttpPut("{name}")]
        [ProducesResponseType(200, Type = typeof(CreateAlbumResult))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<JsonResult> CreateAlbum(string name)
        {
            var userId = Utilities.GetUsername(HttpContext.User);

            var album = new Album
            {
                AlbumId = name + "-" + Guid.NewGuid(),
                Name = name,
                UserId = userId,
                CreateDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _ddbContext.SaveAsync(album).ConfigureAwait(false);

            return new JsonResult(new CreateAlbumResult {AlbumId = album.AlbumId});
        }

        /// <summary>
        ///     Delete a album.
        /// </summary>
        /// <param name="albumId">The id of the album to delete.</param>
        /// <returns></returns>
        [HttpDelete("{albumId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteAlbum(string albumId)
        {
            var userId = Utilities.GetUsername(HttpContext.User);

            await _ddbContext.DeleteAsync<Album>(userId, albumId);

            return Ok();
        }

        private class CreateAlbumResult
        {
            public string AlbumId { get; set; }
        }
    }
}