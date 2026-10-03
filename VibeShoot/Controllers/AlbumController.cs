using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VibeShoot.Data;
using VibeShoot.Models;
using VibeShoot.Models.Entities;

namespace VibeShoot.Controllers
{
    public class AlbumController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlbumController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("photographer/{photographer}/album")]
        public async Task<IActionResult> Gallery(string photographer)
        {
            var entity = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer && p.IsActive);
            if (entity == null)
            {
                return Redirect("/photographers");
            }

            var images = await _context.GalleryImages
                .Where(g => g.PhotographerId == entity.Id)
                .OrderBy(g => g.SortOrder).ThenBy(g => g.Id)
                .Select(g => new { g.Category, g.FilePath })
                .ToListAsync();

            var model = new AlbumViewModel
            {
                Slug = entity.Slug,
                Name = entity.Name,
                LogoPath = entity.LogoPath,
                Categories = GalleryImage.Categories.Select(cat => new GalleryCategory
                {
                    Name = cat,
                    Slug = cat.ToLowerInvariant(),
                    ImageUrls = images.Where(i => i.Category == cat).Select(i => i.FilePath).ToList()
                }).ToList()
            };

            return View("~/Views/Album/Gallery.cshtml", model);
        }
    }
}
