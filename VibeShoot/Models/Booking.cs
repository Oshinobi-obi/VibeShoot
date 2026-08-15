using System;
using System.ComponentModel.DataAnnotations;

namespace VibeShoot.Models.Entities
{
    public class Booking
    {
        [Key]
        public string TransactionId { get; set; }
        public int PhotographerId { get; set; }
        public string ClientName { get; set; }
        public string ContactNumber { get; set; }
        public string SocialLink { get; set; }
        public DateTime TargetDate { get; set; }
        public string Venue { get; set; }
        public string Category { get; set; }
        public string PackageName { get; set; }
        public decimal AmountPaid { get; set; }
        public string ReceiptImagePath { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }

        public Photographer Photographer { get; set; }
    }
}