using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VibeShoot.Models.Entities;

namespace VibeShoot.Data
{
    /// <summary>
    /// Creates/updates the schema and fills a fresh database with the studio's photographers,
    /// packages and every portfolio image already sitting in wwwroot/Uploads.
    /// Safe to run on every start: it only inserts what is missing.
    /// </summary>
    public static class DbSeeder
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public static async Task SeedAsync(ApplicationDbContext db, IWebHostEnvironment env, ILogger logger)
        {
            await db.Database.MigrateAsync();

            await SeedPhotographersAsync(db, env);
            await SeedPackagesAsync(db);
            var imported = await ImportGalleryFromDiskAsync(db, env);

            if (imported > 0)
            {
                logger.LogInformation("Imported {Count} gallery images from wwwroot/Uploads/Album into the database.", imported);
            }
        }

        private static async Task SeedPhotographersAsync(ApplicationDbContext db, IWebHostEnvironment env)
        {
            var seeds = new List<Photographer>
            {
                new Photographer
                {
                    Slug = "sulyap-films", Name = "Sulyap Films", MediaFolder = "SulyapFilms", SortOrder = 1,
                    Tagline = "Cinematic films and photographs for life's quiet glances.",
                    Bio = "Sulyap Films captures the fleeting glances that make every celebration yours — " +
                          "candid, cinematic and full of feeling.",
                    FacebookUrl = "https://www.facebook.com/share/1DbuUDU2iv/",
                    InstagramUrl = "https://www.instagram.com/sulyapfilms?igsh=a3FuamY0Zzk2eDlz",
                },
                new Photographer
                {
                    Slug = "ginger-snaps", Name = "Ginger Snaps Photography", MediaFolder = "GingerSnaps", SortOrder = 2,
                    Tagline = "Genuine moments, rich color, timeless stories.",
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
                    TikTokUrl = "https://www.tiktok.com/@gngrsnpsptgrpy?_r=1",
                },
                new Photographer
                {
                    Slug = "chiyos-folder", Name = "Chiyos Folder", MediaFolder = "ChiyosFolder", SortOrder = 3,
                    Tagline = "Portraits, places, cats and coffee — told with an artist's eye.",
                    Bio = "Hi, I'm Jerick Ybarreta, but most people know me as Chiyo especially my artist friends. I'm a creative individual " +
                          "with a passion for photography, graphic design, and a little bit of drawing. I love working on projects that allow me to " +
                          "express my artistic side while exploring different subjects and ideas. I enjoy taking portraits, capturing the beauty of places, " +
                          "and focusing on unique subjects like cats and coffee shops. Whether I'm behind the camera or designing something new, " +
                          "I always aim to create work that feels authentic and meaningful. My goal is to bring ideas to life and connect with people through my art.",
                    FacebookUrl = "https://www.facebook.com/share/186kdJyk7R/",
                    InstagramUrl = "https://www.instagram.com/Definitely_not_jerick",
                    TikTokUrl = "https://www.tiktok.com/@matchiiyoooo_?_r=1&_t=ZS-98kzNSnNaxY",
                    XUrl = "https://x.com/JerickYbarreta",
                },
            };

            var existing = await db.Photographers.Select(p => p.Slug).ToListAsync();
            foreach (var p in seeds.Where(s => !existing.Contains(s.Slug)))
            {
                p.LogoPath = FirstExistingFile(env, $"Uploads/Logos/{p.MediaFolder}") ?? "";
                p.GCashQrPath = FirstExistingFile(env, $"Uploads/QRCodes/{p.MediaFolder}");
                db.Photographers.Add(p);
            }
            await db.SaveChangesAsync();
        }

        private static async Task SeedPackagesAsync(ApplicationDbContext db)
        {
            var photographers = await db.Photographers.Include(p => p.Packages).ToListAsync();

            foreach (var p in photographers.Where(p => p.Packages.Count == 0))
            {
                if (p.Slug == "ginger-snaps")
                {
                    const string basic = "1 Photographer\n1 Assistant\n2-3hrs. Photo Coverage\nUnlimited Shots\n150 minimum Photos\n7-9 Days Editing Process";
                    const string combo = "1 Photographer, 1 Videographer, 1 Assistant\n2-3hrs. Photo & Video Coverage\nUnlimited Shots\n3-5 min. Video Highlights\n150 minimum Photos\n7-9 Days Editing Process";
                    p.Packages.Add(new ServicePackage { Category = "Birthday", Name = "Basic Package", Price = 2999, DurationHours = 3, Inclusions = basic, SortOrder = 1 });
                    p.Packages.Add(new ServicePackage { Category = "Birthday", Name = "Combo Package", Price = 6499, DurationHours = 3, Inclusions = combo, SortOrder = 2 });
                    p.Packages.Add(new ServicePackage { Category = "Baptism", Name = "Basic Package", Price = 2999, DurationHours = 3, Inclusions = basic, SortOrder = 1 });
                    p.Packages.Add(new ServicePackage { Category = "Baptism", Name = "Combo Package", Price = 6499, DurationHours = 3, Inclusions = combo, SortOrder = 2 });
                    p.Packages.Add(new ServicePackage
                    {
                        Category = "Photoshoot", Name = "Classic Package", Price = 3499, DurationHours = 2, SortOrder = 1,
                        Inclusions = "1 Photographer\n1 Assistant\n1 Location\n2hrs. Photo Session\nUnlimited Shots\n50 minimum Composed Edited Photos\n1-2 Weeks Editing Process"
                    });
                    p.Packages.Add(new ServicePackage
                    {
                        Category = "Photoshoot", Name = "Deluxe Package", Price = 7499, DurationHours = 2, SortOrder = 2,
                        Inclusions = "1 Photographer, 1 Videographer, 1 Assistant\n1 Location\n2hrs. Photo & Video Session\nUnlimited Shots\n2-4 min. Video Shoot\n50 minimum Composed Edited Photos\n1-2 Weeks Editing Process"
                    });
                    p.Packages.Add(new ServicePackage
                    {
                        Category = "Wedding", Name = "Wedding Package", Price = 0, DurationHours = 5, SortOrder = 1,
                        Inclusions = "Packages for Weddings are currently custom tailored.\nSubmit a request and we will contact you for a quote!"
                    });
                }
                else
                {
                    foreach (var cat in ServicePackage.Categories)
                    {
                        p.Packages.Add(new ServicePackage
                        {
                            Category = cat, Name = "Custom Package", Price = 0, DurationHours = cat == "Wedding" ? 5 : 3, SortOrder = 1,
                            Inclusions = "Package details coming soon!\nSubmit a request and we will send you a quote."
                        });
                    }
                }
            }
            await db.SaveChangesAsync();
        }

        /// <summary>Registers every image under wwwroot/Uploads/Album/{MediaFolder}/{Category} that is not in the database yet.</summary>
        public static async Task<int> ImportGalleryFromDiskAsync(ApplicationDbContext db, IWebHostEnvironment env)
        {
            var known = (await db.GalleryImages.Select(g => g.FilePath).ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var photographers = await db.Photographers.ToListAsync();
            int added = 0;

            foreach (var p in photographers.Where(p => !string.IsNullOrEmpty(p.MediaFolder)))
            {
                foreach (var category in GalleryImage.Categories)
                {
                    var dir = Path.Combine(env.WebRootPath, "Uploads", "Album", p.MediaFolder, category);
                    if (!Directory.Exists(dir)) continue;

                    var files = Directory.GetFiles(dir)
                        .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                        .OrderBy(f => NaturalKey(Path.GetFileName(f)))
                        .ToList();

                    int order = await db.GalleryImages.CountAsync(g => g.PhotographerId == p.Id && g.Category == category);
                    foreach (var file in files)
                    {
                        var url = $"/Uploads/Album/{p.MediaFolder}/{category}/{Path.GetFileName(file)}";
                        if (known.Contains(url)) continue;

                        db.GalleryImages.Add(new GalleryImage
                        {
                            PhotographerId = p.Id,
                            Category = category,
                            FilePath = url,
                            FileSizeBytes = new FileInfo(file).Length,
                            SortOrder = ++order,
                            IsFeatured = category == "Highlights" && order <= 3,
                        });
                        known.Add(url);
                        added++;
                    }
                }
            }

            await db.SaveChangesAsync();
            return added;
        }

        private static string? FirstExistingFile(IWebHostEnvironment env, string relativeDir)
        {
            var dir = Path.Combine(env.WebRootPath, relativeDir.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(dir)) return null;

            var file = Directory.GetFiles(dir)
                .FirstOrDefault(f => ImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));
            return file == null ? null : $"/{relativeDir}/{Path.GetFileName(file)}";
        }

        /// <summary>Sorts "GS2" before "GS10".</summary>
        private static string NaturalKey(string name) =>
            Regex.Replace(name, @"\d+", m => m.Value.PadLeft(8, '0'));
    }
}
