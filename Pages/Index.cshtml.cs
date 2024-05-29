using Gallery.Data;
using Gallery.Models;
using Gallery.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Security.Claims;

namespace Gallery.Pages
{
    public class IndexModel : PageModel
    {
        private IWebHostEnvironment _environment;
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public ICollection<StoredFileListViewModel> Files { get; set; }
        public ICollection<StoredFile> StoredFiles { get; set; }
        public ICollection<Album> Albums { get; set; }

        public string ChosenImage { get; set; }
        public int ChosenAlbum { get; set; }
        public bool ShowModal { get; set; }

        public bool ShowUserModal { get; set; }
        public string? UserId { get; set; }
        public int imageId { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _environment = environment;
            _logger = logger;
            _context = context;
        }

        public void OnGet(string Who, int? SelectedAlbumId, int imgId)
        {
            Files = _context.Files
                .AsNoTracking()
                .Include(f => f.Uploader)
                .Include(f => f.Thumbnails)
                .Where(f => f.IsPublic == true)
                .Select(f => new StoredFileListViewModel
                {
                    Id = f.Id,
                    Title = f.Title,
                    Description = f.Description,
                    ContentType = f.ContentType,
                    OriginalName = f.OriginalName,
                    UploaderId = f.UploaderId,
                    Uploader = f.Uploader,
                    UploadedAt = f.UploadedAt,
                    DateTaken = (DateTime)f.DateTaken,
                    ThumbnailCount = f.Thumbnails.Count,
                    Position = f.Position
                })
                .OrderByDescending(f => f.DateTaken)
                .Take(12)
                .ToList();

            StoredFiles = _context.Files.Include(f => f.Thumbnails).Include(f => f.Uploader).ToList();
            Albums = _context.Albums.Include(f => f.Uploader).Include(f => f.Files).ThenInclude(f => f.Order).ToList();

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
            UserId = userId;

            if (!string.IsNullOrEmpty(Who))
            {
                ChosenImage = Who;
                ShowModal = true;
            }

            if (SelectedAlbumId != null)
            {
                ChosenAlbum = (int)SelectedAlbumId;
                ShowUserModal = true;
                imageId = imgId;
            }
        }

        public IActionResult OnGetDownload(string filename)
        {
            filename = Path.GetFileName(filename);
            var fullName = Path.Combine(_environment.ContentRootPath, "Uploads", filename);
            if (System.IO.File.Exists(fullName)) // existuje soubor na disku?
            {
                var fileRecord = _context.Files.Find(Guid.Parse(filename));
                if (fileRecord != null) // je soubor v databázi?
                {
                    return PhysicalFile(fullName, fileRecord.ContentType, fileRecord.OriginalName);
                    // vrať ho zpátky pod původním názvem a typem
                }
                else
                {
                    ErrorMessage = "There is no record of such file.";
                    return RedirectToPage();
                }
            }
            else
            {
                ErrorMessage = "There is no such file.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetThumbnail(string filename, ThumbnailType type = ThumbnailType.Square)
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
            StoredFile file = await _context.Files
              .AsNoTracking()
              .Where(f => f.Id == Guid.Parse(filename))
              .SingleOrDefaultAsync();
            if (file == null)
            {
                return NotFound("no record for this file");
            }
            Thumbnail thumbnail = await _context.Thumbnails
              .AsNoTracking()
              .Where(t => t.FileId == Guid.Parse(filename) && t.Type == type)
              .SingleOrDefaultAsync();
            if (file.UploaderId != userId?.Value && file.IsPublic == false)
            {
                ErrorMessage = "You don't have access to that image";
                return RedirectToPage();
            }
            if (thumbnail != null)
            {
                return File(thumbnail.Blob, file.ContentType);
            }
            return NotFound("no thumbnail for this file");
        }

        public async Task<IActionResult> OnGetIdk(string who)
        {
            return RedirectToPage("Index", new { Who = who});
        }

        public async Task<IActionResult> OnGetDeleteAlbum(int id)
        {
            var album = await _context.Albums.Include(f => f.Files).SingleOrDefaultAsync(f => f.Id == id);
            if (album != null)
            {
                _context.Albums.Remove(album);
                _context.SaveChanges();
            }
            else
            {
                return NotFound("record doesn't exist");
            }

            return RedirectToPage();
        }

        public IActionResult OnGetShowAlbum(int id)
        {
            return RedirectToPage("Index", new { SelectedAlbumId = id, imgId = 0 });
        }

        public IActionResult OnGetNextImage(int id, int imgId)
        {
            Album SelectedAlbum = _context.Albums.Include(f => f.Files).Where(f => f.Id == id).SingleOrDefault();
            int nextId;
            if (imgId + 1 >= SelectedAlbum.Files.Count)
            {
                nextId = 0;
            }
            else
            {
                nextId = imgId + 1;
            }
            return RedirectToPage("Index", new { SelectedAlbumId = id, imgId = nextId });
        }

        public IActionResult OnGetPrevImage(int id, int imgId)
        {
            Album SelectedAlbum = _context.Albums.Include(f => f.Files).Where(f => f.Id == id).SingleOrDefault();
            int nextId;
            if (imgId - 1 < 0)
            {
                nextId = SelectedAlbum.Files.Count - 1;
            }
            else
            {
                nextId = imgId - 1;
            }
            return RedirectToPage("Index", new { SelectedAlbumId = id, imgId = nextId });
        }
    }
}