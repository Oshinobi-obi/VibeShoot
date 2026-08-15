using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using VibeShoot.Data;
using VibeShoot.Models;

namespace VibeShoot.Controllers
{
    public class AlbumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string ImageExtension = ".jpg";

        public AlbumController(ApplicationDbContext context)
        {
            _context = context;
        }

        private static readonly (string Display, string FileSuffix)[] CategoryDefs = new[]
        {
            ("Birthday", "Bday"),
            ("Baptism", "Baptism"),
            ("Wedding", "Wedding"),
            ("Highlights", "Highlights")
        };

        private static readonly Dictionary<string, (string FolderName, string Prefix, Dictionary<string, int> Counts)> Galleries =
            new Dictionary<string, (string, string, Dictionary<string, int>)>
            {
                ["chiyos-folder"] = ("ChiyosFolder", "CF",
                    new Dictionary<string, int> { ["Birthday"] = 5, ["Baptism"] = 5, ["Wedding"] = 0, ["Highlights"] = 15 }),

                ["ginger-snaps"] = ("GingerSnaps", "GS",
                    new Dictionary<string, int> { ["Birthday"] = 15, ["Baptism"] = 15, ["Wedding"] = 9, ["Highlights"] = 15 }),

                ["sulyap-films"] = ("SulyapFilms", "SF",
                    new Dictionary<string, int> { ["Birthday"] = 0, ["Baptism"] = 0, ["Wedding"] = 0, ["Highlights"] = 0 })
            };

        [HttpGet("photographer/{photographer}/album")]
        public async Task<IActionResult> Gallery(string photographer)
        {
            if (string.IsNullOrEmpty(photographer) || !Galleries.ContainsKey(photographer))
            {
                return Redirect("/photographers");
            }

            var entity = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer);
            if (entity == null)
            {
                return Redirect("/photographers");
            }

            var info = Galleries[photographer];

            var model = new AlbumViewModel
            {
                Slug = photographer,
                Name = entity.Name,
                LogoPath = entity.LogoPath,
                Categories = new List<GalleryCategory>()
            };

            foreach (var def in CategoryDefs)
            {
                var count = info.Counts.TryGetValue(def.Display, out var c) ? c : 0;
                var urls = new List<string>();

                for (int i = 1; i <= count; i++)
                {
                    urls.Add($"/Uploads/Album/{info.FolderName}/{def.Display}/{info.Prefix}{i}{def.FileSuffix}{ImageExtension}");
                }

                model.Categories.Add(new GalleryCategory
                {
                    Name = def.Display,
                    Slug = def.Display.ToLowerInvariant(),
                    ImageUrls = urls
                });
            }

            return View("~/Views/Album/Gallery.cshtml", model);
        }
    }
}