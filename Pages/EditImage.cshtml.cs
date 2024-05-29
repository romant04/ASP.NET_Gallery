using Gallery.Data;
using Gallery.InputModels;
using Gallery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gallery.Pages
{
    public class EditImageModel : PageModel
    {
        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        private readonly ApplicationDbContext _context;
        [BindProperty]
        public InStoredFile Image { get; set; }
        public StoredFile ImageData { get; set; }
        [BindProperty]
        public ICollection<int> SelectedAlbumIds { get; set; }
        public List<Album> Albums { get; set; }
        [BindProperty]
        public string Id { get; set; }

        public EditImageModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if(id == null || _context.Albums == null)
            {
                return NotFound();
            }

            var image = await _context.Files.Include(f => f.Album).SingleOrDefaultAsync(f => f.Id == Guid.Parse(id));
            if(image == null)
            {
                return NotFound();
            }

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            if (image.UploaderId != userId)
            {
                ErrorMessage = "The image you tried to edit doesn't belong to you";
                return RedirectToPage("UserImages");
            }

            ImageData = image;

            Albums = _context.Albums.Include(f => f.Uploader)
               .Where(a => a.Uploader.Id == userId).ToList();

            Image = new InStoredFile
            {
                Title = image.Title,
                Description = image.Description,
                IsPublic = image.IsPublic,
            };

            Id = id;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var image = await _context.Files.Include(f => f.Order).Include(f => f.Album).SingleOrDefaultAsync(f => f.Id == Guid.Parse(Id));
            ICollection<Album> SelectedAlbums = await _context.Albums.Include(f => f.Files).ThenInclude(f => f.Order).Where(f => SelectedAlbumIds.Contains(f.Id)).ToListAsync();

            image.Title = Image.Title;
            image.Description = Image.Description;
            image.IsPublic = (bool)Image.IsPublic;
            image.Album = SelectedAlbums;
            var oldOrders = image.Order.Where(f => !SelectedAlbumIds.Contains(f.Id)).ToList();
            foreach(var album in SelectedAlbums)
            {
                if(image.Order.SingleOrDefault(f => f.GalleryId == album.Id) == null)
                {
                    image.Order.Add(new GalleryOrder { GalleryId = album.Id, Order = album.Files.Count + 1 });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageExists(Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            var partOfImage = image.Order.ToList();
            List<int> partOfImgIds = new List<int>();
            foreach(var v in partOfImage)
            {
                partOfImgIds.Add(v.Id);
            }
            var ordersToDelete = _context.GalleryOrders.Where(f => !(SelectedAlbumIds.Contains(f.GalleryId)) && partOfImgIds.Contains(f.Id)).ToList();
            if(ordersToDelete.Count > 0)
            {
                _context.RemoveRange(ordersToDelete);
                await _context.SaveChangesAsync();
            }

            foreach (var v in SelectedAlbums)
            {
                if (v.CoverImageId == null)
                {
                    v.CoverImageId = image.Id;
                    await _context.SaveChangesAsync();
                }
            }

            // Get galleries where this image was set as cover
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            var wasCover = _context.Albums.Include(f => f.Uploader).Where(f => f.Uploader.Id == userId)
                           .Where(f => !SelectedAlbumIds.Contains(f.Id) && f.CoverImageId == image.Id)
                           .ToList();
            if(wasCover.Count > 0)
            {
                foreach (var x in wasCover)
                {
                    x.CoverImageId = null;
                    await _context.SaveChangesAsync();
                }
            }

            // set new cover when this was cover
            var lostCover = _context.Albums.Include(f => f.Files).Include(f => f.Uploader).Where(f => f.Uploader.Id == userId)
                          .Where(f => !SelectedAlbumIds.Contains(f.Id) && f.Files.Count > 0 && f.CoverImageId == null)
                          .ToList();

            if (lostCover.Count > 0)
            {
                foreach (var x in lostCover)
                {
                    x.CoverImageId = x.Files.First().Id;
                    await _context.SaveChangesAsync();
                }
            }


            // rearange ordeds in old galeries
            List<int> oldIdsList = new List<int>();
            foreach (var oldOrder in oldOrders)
            {
                oldIdsList.Add(oldOrder.GalleryId); 
            }
            var old = _context.Albums.Include(f => f.Files).ThenInclude(f => f.Order).Where(f => oldIdsList.Contains(f.Id)).ToList();
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

            return RedirectToPage("/UserImages");

        }
        private bool ImageExists(string id)
        {
            return _context.Files.Any(e => e.Id == Guid.Parse(id));
        }

    }
}
