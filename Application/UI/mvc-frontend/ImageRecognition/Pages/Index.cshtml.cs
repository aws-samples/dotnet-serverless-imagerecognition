using System.Collections.Generic;
using System.Threading.Tasks;
using ImageRecognition.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageRecognition.Frontend.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ImageRecognitionManager _imageRecognitionManager;

        public IndexModel(ImageRecognitionManager imageRecognitionManager)
        {
            _imageRecognitionManager = imageRecognitionManager;
        }

        public IList<Album> Albums { get; set; }

        public async Task OnGet()
        {
            Albums = await _imageRecognitionManager.GetAlbums(HttpContext.User.Identity.Name);
        }
    }
}