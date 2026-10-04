using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
                .ToListAsync();
            var today = System.DateTime.Today;
            packages = packages.OrderBy(k => k.Price == 0).ThenBy(k => k.PriceOn(today)).ToList();

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
                StartingPrice = packages.Where(k => k.Price > 0).Select(k => (decimal?)k.PriceOn(today)).Min(),
                // One card per event type, cheapest first.
                Packages = packages
                    .GroupBy(k => k.Category)
                    .Select(g => g.First())
                    .Take(3)
                    .Select(k => new PackagePreview
                    {
                        Category = k.Category,
                        Name = k.Name,
                        Price = k.PriceOn(today),
                        OriginalPrice = k.Price,
                        DiscountBadge = k.HasActiveDiscount(today) ? k.DiscountBadge : null,
                        DiscountLabel = k.HasActiveDiscount(today) ? k.DiscountLabel : null,
                        Hours = k.DurationHours,
                        Highlights = k.InclusionList.Take(4).ToList()
                    })
                    .ToList()
            };

            // Reviews (hidden ones are left out entirely).
            var reviews = await _context.Reviews
                .Where(r => r.PhotographerId == entity.Id && !r.IsHidden)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            model.Reviews = new ReviewSummary
            {
                Count = reviews.Count,
                Average = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 1),
                Breakdown = Enumerable.Range(1, 5).ToDictionary(n => n, n => reviews.Count(r => r.Rating == n)),
                TopTags = reviews.SelectMany(r => r.TagList)
                    .GroupBy(tag => tag).Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                    .OrderByDescending(kv => kv.Value).Take(6).ToList(),
                Latest = reviews.Take(6).ToList(),
            };

            // Not enough photos for a separate grid? Reuse what we have.
            if (model.Showcase.Count < 3) model.Showcase = photos.Take(6).Select(p => p.FilePath).ToList();

            return View("~/Views/About/AboutMe.cshtml", model);
        }
    }
}
