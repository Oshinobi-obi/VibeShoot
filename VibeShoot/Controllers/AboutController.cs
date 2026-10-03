using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VibeShoot.Data;
using VibeShoot.Models;

namespace VibeShoot.Controllers
{
    public class AboutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AboutController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("photographer/{photographer}/about")]
        public async Task<IActionResult> AboutMe(string photographer)
        {
            var entity = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer && p.IsActive);
            if (entity == null)
            {
                return Redirect("/photographers");
            }

            // Featured photos first, then Highlights, then everything else.
            var photos = await _context.GalleryImages
                .Where(g => g.PhotographerId == entity.Id)
                .OrderByDescending(g => g.IsFeatured)
                .ThenByDescending(g => g.Category == "Highlights")
                .ThenBy(g => g.SortOrder)
                .Select(g => new { g.FilePath, g.Category })
                .ToListAsync();

            var packages = await _context.Packages
                .Where(k => k.PhotographerId == entity.Id && k.IsActive)
                .OrderBy(k => k.Price == 0).ThenBy(k => k.Price)
                .ToListAsync();

            var model = new PhotographerProfile
            {
                Slug = entity.Slug,
                Name = entity.Name,
                LogoPath = entity.LogoPath,
                Tagline = entity.Tagline,
                Bio = entity.Bio,
                FacebookUrl = string.IsNullOrWhiteSpace(entity.FacebookUrl) ? "#" : entity.FacebookUrl,
                InstagramUrl = string.IsNullOrWhiteSpace(entity.InstagramUrl) ? "#" : entity.InstagramUrl,
                TikTokUrl = string.IsNullOrWhiteSpace(entity.TikTokUrl) ? "#" : entity.TikTokUrl,
                XUrl = string.IsNullOrWhiteSpace(entity.XUrl) ? "#" : entity.XUrl,
                CoverImages = photos.Take(5).Select(p => p.FilePath).ToList(),
                Showcase = photos.Skip(3).Take(6).Select(p => p.FilePath).ToList(),
                PhotoCount = photos.Count,
                AlbumCount = photos.Select(p => p.Category).Distinct().Count(),
                StartingPrice = packages.Where(k => k.Price > 0).Select(k => (decimal?)k.Price).Min(),
                // One card per event type, cheapest first.
                Packages = packages
                    .GroupBy(k => k.Category)
                    .Select(g => g.First())
                    .Take(3)
                    .Select(k => new PackagePreview
                    {
                        Category = k.Category,
                        Name = k.Name,
                        Price = k.Price,
                        Hours = k.DurationHours,
                        Highlights = k.InclusionList.Take(4).ToList()
                    })
                    .ToList()
            };

            // Not enough photos for a separate grid? Reuse what we have.
            if (model.Showcase.Count < 3) model.Showcase = photos.Take(6).Select(p => p.FilePath).ToList();

            return View("~/Views/About/AboutMe.cshtml", model);
        }
    }
}
