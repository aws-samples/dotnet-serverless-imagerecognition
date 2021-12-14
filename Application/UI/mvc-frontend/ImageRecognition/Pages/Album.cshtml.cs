using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ImageRecognition.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageRecognition.Frontend.Pages
{
    [Authorize]
    public class AlbumModel : PageModel
    {
        // 15 megabytes.
        public const long MAX_SOURCE_IMAGE_SIZE = 15 * 1048576;

        private readonly ImageRecognitionManager _imageRecognitionManager;

        public AlbumModel(ImageRecognitionManager imageRecognitionManager)
        {
            _imageRecognitionManager = imageRecognitionManager;
        }

        public Album Album { get; private set; }

        [BindProperty] [Required] public string AlbumId { get; set; }

        [BindProperty] [Required] public ICollection<IFormFile> PhotoSourceImages { get; set; }

        public async Task<IActionResult> OnGetAsync(string albumId)
        {
            Album = await _imageRecognitionManager.GetAlbumDetails(HttpContext.User.Identity.Name, albumId);

            if (Album == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(string name)
        {
            var album = await _imageRecognitionManager.CreateAlbum(HttpContext.User.Identity.Name, name);

            return new RedirectResult($"~/album/{WebUtility.UrlEncode(album.AlbumId).Replace('+', ' ')}/");
        }

        public async Task<IActionResult> OnPostUploadAsync(string albumId)
        {
            foreach (var photoSourceImage in PhotoSourceImages)
            {
                var fileName = WebUtility.HtmlEncode(Path.GetFileName(photoSourceImage.FileName));
                var extension = Path.GetExtension(fileName);

                if (photoSourceImage.Length > MAX_SOURCE_IMAGE_SIZE)
                    return BadRequest($"{fileName} is larger then the max size of {MAX_SOURCE_IMAGE_SIZE}");
                if (!string.Equals(".jpg", extension, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(".png", extension, StringComparison.OrdinalIgnoreCase))
                    return BadRequest($"File types {extension} are not supported, only jpg and png files");

                using (var stream = photoSourceImage.OpenReadStream())
                {
                    await _imageRecognitionManager
                        .AddPhoto(AlbumId, HttpContext.User.Identity.Name, fileName, stream)
                        .ConfigureAwait(false);
                }
            }

            return new RedirectResult($"~/album/{WebUtility.UrlEncode(AlbumId).Replace('+', ' ')}/");
        }
    }
}