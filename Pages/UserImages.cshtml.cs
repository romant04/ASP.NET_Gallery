using Gallery.Data;
using Gallery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using tusdotnet.Interfaces;
using tusdotnet.Stores;
using System.Text.RegularExpressions;

namespace Gallery.Pages
{
    [Authorize]
    public class UserImagesModel : PageModel
    {
        private IWebHostEnvironment _environment;
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public ICollection<StoredFile> Images { get; set; }
        public string UserId { get; set; }

        public string ChosenImage { get; set; }
        public int Width { get; set; }

        public bool ShowUserModal { get; set; }
        public UserImagesModel(ILogger<IndexModel> logger, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }

        public void OnGet(string SelectedImageId)
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            Images = _context.Files.Include(f => f.Position).Include(f => f.Album).Include(f => f.Thumbnails).Include(f => f.Uploader).Where(f => f.UploaderId == userId).ToList();

            UserId = userId;

            if (!string.IsNullOrEmpty(SelectedImageId))
            {
                ChosenImage = SelectedImageId;
                ShowUserModal = true;
            }
        }

        public IActionResult OnGetDownload(string filename)
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            StoredFile file = _context.Files
             .AsNoTracking()
             .Where(f => f.Id == Guid.Parse(filename))
             .SingleOrDefault();
            if (file.UploaderId != userId && file.IsPublic == false)
            {
                ErrorMessage = "You don't have access to that image";
                return RedirectToPage();
            }

            filename = Path.GetFileName(filename);
            var fullName = Path.Combine(_environment.ContentRootPath, "Uploads", filename);
            if (System.IO.File.Exists(fullName)) // existuje soubor na disku?
            {
                var fileRecord = _context.Files.Find(Guid.Parse(filename));
                if (fileRecord != null) // je soubor v datab·zi?
                {
                    return PhysicalFile(fullName, fileRecord.ContentType, fileRecord.OriginalName);
                    // vraù ho zp·tky pod p˘vodnÌm n·zvem a typem
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
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            StoredFile file = await _context.Files
              .AsNoTracking()
              .Where(f => f.Id == Guid.Parse(filename))
              .SingleOrDefaultAsync();
            if (file == null)
            {
                return NotFound("no record for this file");
            }
            if(file.UploaderId != userId && file.IsPublic == false)
            {
                ErrorMessage = "You don't have access to that image";
                return RedirectToPage();
            }
            Thumbnail thumbnail = await _context.Thumbnails
              .AsNoTracking()
              .Where(t => t.FileId == Guid.Parse(filename) && t.Type == type)
              .SingleOrDefaultAsync();
            if (thumbnail != null)
            {
                return File(thumbnail.Blob, file.ContentType);
            }
            return NotFound("no thumbnail for this file");
        }

        public IActionResult OnGetEditPosition(string id)
        {
            return RedirectToPage("EditPosition", new { id });
        }

        public async Task<IActionResult> OnGetDelete(string filename)
        {
            Guid id = Guid.Parse(filename);
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            var file = await _context.Files.FindAsync(id);
            if (file.UploaderId != userId)
            {
                ErrorMessage = "You don't have access to that image";
                return RedirectToPage();
            }

            var galOrders = _context.GalleryOrders.Where(f => f.StoredFileId == id).ToList();
            List<int> albumsAfectedIds = new List<int>();
            foreach(var v in galOrders)
            {
                albumsAfectedIds.Add(v.GalleryId);
            }
            foreach (var galOrder in galOrders)
            {
                _context.GalleryOrders.Remove(galOrder);
            }
            await _context.SaveChangesAsync();

            // rearange ordeds in old galeries
            var old = _context.Albums.Include(f => f.Files).ThenInclude(f => f.Order).Where(f => albumsAfectedIds.Contains(f.Id)).ToList();
            foreach (var oldAlbum in old)
            {
                var orders = _context.GalleryOrders.Where(f => f.GalleryId == oldAlbum.Id).ToList();
                var index = 1;
                foreach (var order in orders)
                {
                    order.Order = index;
                    index++;
                }
                await _context.SaveChangesAsync();
            }

            if (file != null)
            {
                _context.Files.Remove(file);
                _context.SaveChanges();
            }
            else
            {
                return NotFound("record doesn't exist");
            }

            var wasCover = _context.Albums.Include(f => f.Uploader).Where(f => f.Uploader.Id == userId)
                           .Where(f => f.CoverImageId == id)
                           .ToList();
            if (wasCover.Count > 0)
            {
                foreach (var x in wasCover)
                {
                    x.CoverImageId = null;
                    await _context.SaveChangesAsync();
                }
            }

            filename = Path.GetFileNameWithoutExtension(filename);
            string toDelete = filename + ".*";
            string toDeleteWith = toDelete.Replace("-", "");
            string[] files = System.IO.Directory.GetFiles(Path.Combine(_environment.ContentRootPath, "Uploads"), toDelete);
            string[] files2 = System.IO.Directory.GetFiles(Path.Combine(_environment.ContentRootPath, "Uploads"), toDeleteWith);
            string[] filesToDelete = files.Concat(files2).ToArray();
            foreach (string f in filesToDelete)
            {
                System.IO.File.Delete(f);
            }

            return RedirectToPage();
        }

        public IActionResult OnGetShowImage(string id)
        {
            return RedirectToPage("UserImages", new { SelectedImageId = id });
        }
    }
}
