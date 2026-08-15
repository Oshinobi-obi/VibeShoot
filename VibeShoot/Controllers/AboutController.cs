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
            if (string.IsNullOrEmpty(photographer))
            {
                return Redirect("/photographers");
            }

            var entity = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer);
            if (entity == null)
            {
                return Redirect("/photographers");
            }

            var model = new PhotographerProfile
            {
                Slug = entity.Slug,
                Name = entity.Name,
                LogoPath = entity.LogoPath
            };

            if (photographer == "ginger-snaps")
            {
                model.Bio = "Hi! I'm Mcrey Ronquillo, a photographer based in Quezon City and a graduate " +
                            "of Bachelor of Science in Information Technology from Quezon City University. " +
                            "With a keen eye for detail and a passion for visual storytelling, I enjoy " +
                            "experimenting with new approaches to photography, especially through color " +
                            "grading. I specialize in capturing genuine moments at birthdays, baptisms, " +
                            "debuts, and coffee shops, while also pursuing my love for street photography, " +
                            "where I find beauty in everyday life and authentic human moments. My goal is " +
                            "to create timeless images that preserve emotions and telling narratives " +
                            "through visuals.";

                model.FacebookUrl = "https://www.facebook.com/profile.php?id=61554942684841";
                model.InstagramUrl = "#";
                model.TikTokUrl = "https://www.tiktok.com/@gngrsnpsptgrpy?_r=1";
                model.XUrl = "#";
            }
            else if (photographer == "sulyap-films")
            {
                model.Bio = "";
                model.FacebookUrl = "https://www.facebook.com/share/1DbuUDU2iv/";
                model.InstagramUrl = "https://www.instagram.com/sulyapfilms?igsh=a3FuamY0Zzk2eDlz";
                model.TikTokUrl = "#";
                model.XUrl = "#";
            }
            else if (photographer == "chiyos-folder")
            {
                model.Bio = "Hi, I’m Jerick Ybarreta, but most people know me as Chiyo especially my artist friends. I’m a creative individual " +
                            "with a passion for photography, graphic design, and a little bit of drawing. I love working on projects that allow me to " +
                            "express my artistic side while exploring different subjects and ideas. I enjoy taking portraits, capturing the beauty of places, " +
                            "and focusing on unique subjects like cats and coffee shops. Whether I’m behind the camera or designing something new, " +
                            "I always aim to create work that feels authentic and meaningful. My goal is to bring ideas to life and connect with people through my art.";
                model.FacebookUrl = "https://www.facebook.com/share/186kdJyk7R/";
                model.InstagramUrl = "https://www.instagram.com/Definitely_not_jerick";
                model.TikTokUrl = "https://www.tiktok.com/@matchiiyoooo_?_r=1&_t=ZS-98kzNSnNaxY";
                model.XUrl = "https://x.com/JerickYbarreta";
            }

            return View("~/Views/About/AboutMe.cshtml", model);
        }
    }
}