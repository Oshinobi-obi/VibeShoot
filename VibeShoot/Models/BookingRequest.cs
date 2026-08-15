using Microsoft.AspNetCore.Http;
using System;

namespace VibeShoot.Models
{
    public class BookingRequest
    {
        public string TransactionId { get; set; }
        public string Date { get; set; }
        public string FullName { get; set; }
        public string ContactNumber { get; set; }
        public string SocialLink { get; set; }
        public string Venue { get; set; }
        public string Category { get; set; }
        public string SelectedPackage { get; set; }
        public decimal AmountPaid { get; set; }
        public IFormFile ReceiptImage { get; set; }
    }
}