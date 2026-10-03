using System.Collections.Generic;

namespace VibeShoot.Models
{
    public class PhotographerProfile
    {
        public string Slug { get; set; } = "";
        public string Name { get; set; } = "";
        public string LogoPath { get; set; } = "";
        public string Tagline { get; set; } = "";
        public string Bio { get; set; } = "";
        public string FacebookUrl { get; set; } = "#";
        public string InstagramUrl { get; set; } = "#";
        public string TikTokUrl { get; set; } = "#";
        public string XUrl { get; set; } = "#";

        /// <summary>Featured photos for the hero slideshow and the polaroid stack.</summary>
        public List<string> CoverImages { get; set; } = new List<string>();

        /// <summary>A handful of photos for the "Selected work" grid.</summary>
        public List<string> Showcase { get; set; } = new List<string>();

        public int PhotoCount { get; set; }
        public int AlbumCount { get; set; }
        public decimal? StartingPrice { get; set; }
        public List<PackagePreview> Packages { get; set; } = new List<PackagePreview>();
    }

    public class PackagePreview
    {
        public string Category { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Hours { get; set; }
        public List<string> Highlights { get; set; } = new List<string>();
    }
}
