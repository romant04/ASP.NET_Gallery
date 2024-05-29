using Gallery.Data;
using Gallery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using System.Security.Claims;
using static NuGet.Packaging.PackagingConstants;

namespace Gallery.Pages
{
    [Authorize]
    public class UserModel : PageModel
    {
        private IWebHostEnvironment _environment;
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public ICollection<Album> Albums { get; set; }
        public ICollection<StoredFile> StoredFiles { get; set; }

        public int ChosenAlbum { get; set; }
        public bool ShowModal { get; set; }
        [BindProperty]
        public int GalleryOrder { get; set; }
        [BindProperty]
        public string EditedFile { get; set; }
        [BindProperty]
        public int AlbumEdited { get; set; }

        public bool ShowUserModal { get; set; }
        public int imageId { get; set; }
        public UserModel(ILogger<IndexModel> logger, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }

        public void OnGet(int? SelectedAlbumId, int imgId)
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            Albums = _context.Albums.Include(f => f.Files).ThenInclude(f => f.Order).Include(f => f.Files).ThenInclude(f => f.Thumbnails).Include(f => f.Uploader).Where(f => f.Uploader.Id == userId).ToList();
            StoredFiles = _context.Files.Include(f => f.Thumbnails).ToList();

            if (SelectedAlbumId != null)
            {
                ChosenAlbum = (int)SelectedAlbumId;
                ShowUserModal = true;
                imageId = imgId;
            }
        }

        public IActionResult OnGetChangeCover(int id)
        {
            return RedirectToPage("EditCover", new {id});
        }

        public IActionResult OnGetShowImagePage(string id)
        {
            return RedirectToPage("UserImages", new { SelectedImageId = id });
        }

        public IActionResult OnGetShowAlbum(int id)
        {
            return RedirectToPage("User", new { SelectedAlbumId = id, imgId = 0 });
        }

        public IActionResult OnGetNextImage(int id, int imgId)
        {
            Album SelectedAlbum = _context.Albums.Include(f => f.Files).Where(f => f.Id == id).SingleOrDefault();
            int nextId;
            if(imgId + 1 >= SelectedAlbum.Files.Count)
            {
                nextId = 0;
            }
            else
            {
                nextId = imgId + 1;
            }
            return RedirectToPage("User", new { SelectedAlbumId = id, imgId = nextId });
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
            return RedirectToPage("User", new { SelectedAlbumId = id, imgId = nextId });
        }

        public async Task<IActionResult> OnGetDelete(int id)
        {
            var toDel = _context.Albums.Include(f => f.Files).ThenInclude(f => f.Order).Include(f => f.Uploader).SingleOrDefault(f => f.Id == id);
            var files = toDel.Files.OrderBy(f => f.Order.SingleOrDefault(f => f.GalleryId == id).Order);

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            if (toDel.Uploader.Id != userId)
            {
                ErrorMessage = "You don't have access to that image";
                return RedirectToPage();
            }

            if (toDel != null)
            {
                _context.Albums.Remove(toDel);
                await _context.SaveChangesAsync();
            }

            foreach(var file in files)
            {
                if(file.Album == null || !(file.Album.Count > 0))
                {
                    var defaultAlubm = _context.Albums.Include(f => f.Files).SingleOrDefault(f => f.IsDefault == true);
                    file.Album = new List<Album> { defaultAlubm };
                    file.Order.Add(new GalleryOrder { GalleryId = defaultAlubm.Id, Order = defaultAlubm.Files.Count });
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var album = _context.Albums.Include(f => f.Files).ThenInclude(f => f.Order).SingleOrDefault(f => f.Id == AlbumEdited);
            var file = _context.Files.SingleOrDefault(f => f.Id == Guid.Parse(EditedFile));

            if (album == null || file == null)
            {
                return NotFound();
            }

            var galOrder = _context.GalleryOrders.SingleOrDefault(f => f.GalleryId == album.Id && f.StoredFileId == file.Id);

            var galOrderId = galOrder.Id;
            var curOrder = galOrder.Order;
            var diff = curOrder - GalleryOrder;
            if(diff == 0)
            {
                return RedirectToPage();
            }

            galOrder.Order = GalleryOrder;

            _context.SaveChanges();

            if(diff > 0)
            {
                var orderdsToShift = _context.GalleryOrders.Where(f => f.GalleryId == album.Id && (f.Order >= GalleryOrder && f.Order <= curOrder)).ToList();
                foreach (var v in orderdsToShift)
                {
                    if(v.Id != galOrderId)
                    {
                        v.Order = v.Order + 1;
                        _context.SaveChanges();
                    }
                }
            }
            else if(diff < 0)
            {
                var orderdsToShift = _context.GalleryOrders.Where(f => f.GalleryId == album.Id && (f.Order <= GalleryOrder && f.Order >= curOrder)).ToList();
                foreach (var v in orderdsToShift)
                {
                    if(v.Id != galOrderId)
                    {
                        v.Order = v.Order - 1;
                        _context.SaveChanges();
                    }
                }   
            }
            

            return RedirectToPage();
        }
    }
}
