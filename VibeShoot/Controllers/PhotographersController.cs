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
                    Slug = p.Slug,
                    Name = p.Name,
                    Tagline = p.Tagline,
                    LogoPath = p.LogoPath,
                    PhotoCount = p.GalleryImages.Count,
                    CoverImage = p.GalleryImages
                        .OrderByDescending(g => g.IsFeatured).ThenBy(g => g.SortOrder)
                        .Select(g => g.FilePath).FirstOrDefault(),
                    StartingPrice = p.Packages.Where(k => k.IsActive && k.Price > 0).Min(k => (decimal?)k.Price),
                })
                .ToListAsync();

            return View("~/Views/Photographers/Details.cshtml", cards);
        }
    }
}
