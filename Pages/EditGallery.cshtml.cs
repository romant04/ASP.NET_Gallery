using Gallery.Data;
using Gallery.InputModels;
using Gallery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gallery.Pages
{
    public class EditGalleryModel : PageModel
    {
        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        private readonly ApplicationDbContext _context;
        [BindProperty]
        public InAlbum Album { get; set; }
        [BindProperty]
        public int? Id { get; set; }

        public EditGalleryModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if(id == null || _context.Albums == null)
            {
                return NotFound();
            }

            var album = await _context.Albums.Include(f => f.Uploader).SingleOrDefaultAsync(f => f.Id == id);
            if(album == null)
            {
                return NotFound();
            }

            if(album.IsDefault == true)
            {
                ErrorMessage = "Default gallery is not editable";
                return RedirectToPage("Index");
            }

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
            if (album.Uploader.Id != userId?.Value)
            {
                ErrorMessage = "The gallery you tried to edit doesn't belong to you";
                return RedirectToPage("User");
            }

            Album = new InAlbum
            {
                Title = album.Title,
                Description = album.Description,
                IsPublic = album.IsPublic,
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

            var album = await _context.Albums.Include(f => f.Files).SingleOrDefaultAsync(f => f.Id == Id);

            album.Title = Album.Title;
            album.Description = Album.Description;
            album.IsPublic = (bool)Album.IsPublic;

            try
            {
                await _context.SaveChangesAsync();
                if(album.IsPublic)
                {
                    List<StoredFile> albumImages = new List<StoredFile>();
                    foreach(var v in album.Files)
                    {
                        albumImages.Add(v);
                    }
                    foreach (StoredFile f in albumImages)
                    {
                        f.IsPublic = true;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlbumExists(Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("/User");

        }
        private bool AlbumExists(int? id)
        {
            return _context.Albums.Any(e => e.Id == id);
        }

    }
}
