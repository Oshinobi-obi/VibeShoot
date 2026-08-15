using System.Collections.Generic;

namespace VibeShoot.Models
{
    public class GalleryCategory
    {
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}