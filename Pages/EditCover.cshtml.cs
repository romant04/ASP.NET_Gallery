using Gallery.Data;
using Gallery.InputModels;
using Gallery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gallery.Pages
{
    public class EditCoverModel : PageModel
    {
        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        private readonly ApplicationDbContext _context;
        public List<StoredFile> StoredFiles { get; set; }
        [BindProperty]
        public Guid? SelectedCoverId { get; set; }
        [BindProperty]
        public int? Id { get; set; }

        public EditCoverModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if(id == null || _context.Albums == null)
            {
                return NotFound();
            }

            var album = await _context.Albums.Include(f => f.Files).ThenInclude(f => f.Thumbnails).Include(f => f.Uploader).SingleOrDefaultAsync(f => f.Id == id);
            if(album == null)
            {
                return NotFound();
            }

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
            if (album.Uploader.Id != userId?.Value)
            {
                ErrorMessage = "The gallery you tried to edit doesn't belong to you";
                return RedirectToPage("User");
            }

            SelectedCoverId = album.CoverImageId;

            StoredFiles = album.Files.ToList();

            Id = id;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var album = await _context.Albums.SingleOrDefaultAsync(f => f.Id == Id);

            album.CoverImageId = SelectedCoverId;

            try
            {
                await _context.SaveChangesAsync();
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
