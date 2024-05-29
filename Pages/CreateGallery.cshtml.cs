using Microsoft.AspNetCore.Identity;
using Gallery.Data;
using Gallery.InputModels;
using Gallery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Gallery.Pages
{
    [Authorize]
    public class CreateGalleryModel : PageModel
    {
        [TempData]
        public string ErrorMessage { get; set; }
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public InAlbum InAlbum { get; set; }
        public CreateGalleryModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if(InAlbum.Title == "--- None ---")
            {
                ErrorMessage = "Nemuze se jmenovat -- None --";
                return RedirectToPage();
            }

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            var user = await _userManager.FindByIdAsync(userId);

            bool isPublic = false;
            if(InAlbum.IsPublic != null)
            {
                isPublic = (bool)InAlbum.IsPublic;
            }

            Album album = new Album
            {
                Title = InAlbum.Title,
                Description = InAlbum.Description,
                Uploader = user,
                IsPublic = isPublic,
            };

            _context.Albums.Add(album);
            _context.SaveChanges();

            return RedirectToPage("/Index");
        }
    }
}
