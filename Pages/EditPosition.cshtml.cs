using Gallery.Data;
using Gallery.InputModels;
using Gallery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gallery.Pages
{
    [Authorize]
    public class EditPositionModel : PageModel
    {
        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        private readonly ApplicationDbContext _context;
        [BindProperty]
        public InPosition Position { get; set; }

        [BindProperty]
        public string Id { get; set; }
        [BindProperty]
        public string UserId { get; set; }

        public EditPositionModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if(id == null || _context.Albums == null)
            {
                return NotFound();
            }

            var image = await _context.Files.Include(f => f.Position).Include(f => f.Album).SingleOrDefaultAsync(f => f.Id == Guid.Parse(id));
            if(image == null)
            {
                return NotFound();
            }

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;

            Position = new InPosition
            {
                row = image.Position?.SingleOrDefault(f => f.UserId == Guid.Parse(userId)).row,
                column = image.Position?.SingleOrDefault(f => f.UserId == Guid.Parse(userId)).column,
                order = image.Position?.SingleOrDefault(f => f.UserId == Guid.Parse(userId)).order,
                UserId = Guid.Parse(userId),
            };

            Id = id;
            UserId = userId;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var image = await _context.Files.Include(f => f.Position).SingleOrDefaultAsync(f => f.Id == Guid.Parse(Id));

            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            var myPos = image.Position.SingleOrDefault(f => f.UserId == Guid.Parse(userId));

            myPos.row = Position.row;
            myPos.column = Position.column;
            myPos.order = Position.order;
            myPos.UserId = Guid.Parse(userId);

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

            return RedirectToPage("/UserImages");

        }
        private bool ImageExists(string id)
        {
            return _context.Files.Any(e => e.Id == Guid.Parse(id));
        }

    }
}
