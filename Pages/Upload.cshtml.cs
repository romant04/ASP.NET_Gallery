using Gallery.Data;
using Gallery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
using System.Security.Claims;

namespace Gallery.Pages
{
    [Authorize]
    public class UploadModel : PageModel
    {
        private IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _squareSize = 768;
        private readonly int _sameAspectRatioHeigth = 2160;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IFormFile Upload { get; set; }

        [BindProperty]
        public string Title { get; set; }
        [BindProperty]
        public string Description { get; set; }
        [BindProperty]
        public bool IsPublic { get; set; }
        [BindProperty]
        public int SelectedAlbumId { get; set; }
        [BindProperty]
        public string UserId { get; set; }
        public List<SelectListItem> Albums { get; set; }

        public UploadModel(IWebHostEnvironment environment, ApplicationDbContext context, IConfiguration configuration)
        {
            _environment = environment;
            _context = context;
            _configuration = configuration;
            if (Int32.TryParse(_configuration["Thumbnails:SquareSize"], out _squareSize) == false) _squareSize = 768;
            if (Int32.TryParse(_configuration["Thumbnails:SameAspectRatioHeigth"], out _sameAspectRatioHeigth) == false) _sameAspectRatioHeigth = 2160;
        }

        public void OnGet()
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            UserId = userId;
            Albums = _context.Albums.Include(f => f.Uploader)
                .Where(a => a.Uploader.Id == userId)
                .Select(f => new SelectListItem(f.Title, f.Id.ToString())).ToList();
        }
    }
}
