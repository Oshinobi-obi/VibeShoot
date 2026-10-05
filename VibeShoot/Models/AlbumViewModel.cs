using System.Collections.Generic;

namespace VibeShoot.Models
{
    public class AlbumViewModel
    {
        public string Slug { get; set; } = "";
        public string Name { get; set; } = "";
        public string LogoPath { get; set; } = "";
        public string Tagline { get; set; } = "";
        public string? CoverImage { get; set; }
        public int TotalCount { get; set; }
        public List<GalleryCategory> Categories { get; set; } = new List<GalleryCategory>();
    }
}