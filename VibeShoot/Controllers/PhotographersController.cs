using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VibeShoot.Data;
using VibeShoot.Models;

namespace VibeShoot.Controllers
{
    public class PhotographersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhotographersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("photographers")]
        public async Task<IActionResult> Details()
        {
            var cards = await _context.Photographers
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
                .Select(p => new PhotographerCard
                {
                    Id = p.Id,
                    Slug = p.Slug,
                    Name = p.Name,
                    Tagline = p.Tagline,
                    LogoPath = p.LogoPath,
                    PhotoCount = p.GalleryImages.Count,
                    CoverImage = p.GalleryImages
                        .OrderByDescending(g => g.IsFeatured).ThenBy(g => g.SortOrder)
                        .Select(g => g.FilePath).FirstOrDefault(),
                })
                .ToListAsync();

            // Starting rate = cheapest active package today, after any discount.
            var today = System.DateTime.Today;
            var packages = await _context.Packages.Where(k => k.IsActive && k.Price > 0).ToListAsync();
            var ratings = await _context.Reviews.Where(r => !r.IsHidden)
                .GroupBy(r => r.PhotographerId)
                .Select(g => new { Id = g.Key, Avg = g.Average(r => r.Rating), Count = g.Count() })
                .ToListAsync();
            foreach (var c in cards)
            {
                c.StartingPrice = packages.Where(k => k.PhotographerId == c.Id).Select(k => (decimal?)k.PriceOn(today)).Min();
                var r = ratings.FirstOrDefault(x => x.Id == c.Id);
                if (r != null) { c.Rating = System.Math.Round(r.Avg, 1); c.ReviewCount = r.Count; }
            }

            return View("~/Views/Photographers/Details.cshtml", cards);
        }
    }
}
