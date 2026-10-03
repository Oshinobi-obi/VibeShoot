using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var model = new PhotographerProfile
            {
                Slug = entity.Slug,
                Name = entity.Name,
                LogoPath = entity.LogoPath,
                Bio = entity.Bio,
                FacebookUrl = string.IsNullOrWhiteSpace(entity.FacebookUrl) ? "#" : entity.FacebookUrl,
                InstagramUrl = string.IsNullOrWhiteSpace(entity.InstagramUrl) ? "#" : entity.InstagramUrl,
                TikTokUrl = string.IsNullOrWhiteSpace(entity.TikTokUrl) ? "#" : entity.TikTokUrl,
                XUrl = string.IsNullOrWhiteSpace(entity.XUrl) ? "#" : entity.XUrl,
            };

            return View("~/Views/About/AboutMe.cshtml", model);
        }
    }
}
