using System;

namespace VibeShoot.Models.Entities
{
    public class Photographer
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string LogoPath { get; set; }
        public string GCashQrPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}