using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using VibeShoot.Models;

namespace VibeShoot.Controllers
{
    public class AboutController : Controller
    {
        private static readonly List<PhotographerProfile> Profiles = new List<PhotographerProfile>
        {
            new PhotographerProfile
            {
                Slug = "ginger-snaps",
                Name = "Ginger Snaps",
                LogoPath = "/Logos/GingerSnaps.png",
                Bio = "Hi! I'm Mcrey Ronquillo, a photographer based in Quezon City and a graduate " +
                      "of Bachelor of Science in Information Technology from Quezon City University. " +
                      "With a keen eye for detail and a passion for visual storytelling, I enjoy " +
                      "experimenting with new approaches to photography, especially through color " +
                      "grading. I specialize in capturing genuine moments at birthdays, baptisms, " +
                      "debuts, and coffee shops, while also pursuing my love for street photography, " +
                      "where I find beauty in everyday life and authentic human moments. My goal is " +
                      "to create timeless images that preserve emotions and telling narratives " +
                      "through visuals.",

                FacebookUrl = "https://www.facebook.com/profile.php?id=61554942684841",
                InstagramUrl = "#",
                TikTokUrl = "https://www.tiktok.com/@gngrsnpsptgrpy?_r=1"
            },
            new PhotographerProfile
            {
                Slug = "sulyap-films",
                Name = "Sulyap Films",
                LogoPath = "/Logos/SulyapFilms.png",
                Bio = "Sulyap Films focuses on cinematic event coverage and short-form video, " +
                      "turning weddings, debuts, and milestones into films clients can rewatch " +
                      "for years — replace this with the photographer's real bio.",
                FacebookUrl = "#",
                InstagramUrl = "#",
                TikTokUrl = "#"
            },
            new PhotographerProfile
            {
                Slug = "chiyos-folder",
                Name = "Chiyos Folder",
                LogoPath = "/Logos/ChiyosFolder.png",
                Bio = "Chiyos Folder brings a playful, editorial eye to portrait and concept " +
                      "shoots, with a style that leans bright, colorful, and a little bit " +
                      "K-pop-inspired — replace this with the photographer's real bio.",
                FacebookUrl = "#",
                InstagramUrl = "#",
                TikTokUrl = "#"
            }
        };

        // GET: /About/AboutMe?photographer=ginger-snaps
        public IActionResult AboutMe(string photographer)
        {
            var profile = Profiles.FirstOrDefault(p => p.Slug == photographer);

            if (profile == null)
            {
                // Unknown or missing slug — send them back to pick a photographer.
                return RedirectToAction("Details", "Photographers");
            }

            return View(profile);
        }
    }
}