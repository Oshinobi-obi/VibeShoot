using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using VibeShoot.Models.Entities;

namespace VibeShoot.Models
{
    public class PhotographerCard
    {
        public string Slug { get; set; } = "";
        public string Name { get; set; } = "";
        public string Tagline { get; set; } = "";
        public string LogoPath { get; set; } = "";
        public string? CoverImage { get; set; }
        public int PhotoCount { get; set; }
        public decimal? StartingPrice { get; set; }
    }

    public class BookPageViewModel
    {
        public Photographer Photographer { get; set; } = new Photographer();
        public List<ServicePackage> Packages { get; set; } = new List<ServicePackage>();
    }

    public class BookingSubmission
    {
        public string Date { get; set; } = "";
        public string StartTime { get; set; } = "";
        public int PackageId { get; set; }
        public string FullName { get; set; } = "";
        public string ContactNumber { get; set; } = "";
        public string? Email { get; set; }
        public string? SocialLink { get; set; }
        public string Venue { get; set; } = "";
        public string? Notes { get; set; }
        public string? ReferenceNumber { get; set; }
        public IFormFile? ReceiptImage { get; set; }
        public bool AcceptedTerms { get; set; }
    }

    public class TrackViewModel
    {
        public string? TransactionId { get; set; }
        public string? ContactNumber { get; set; }
        public string? Error { get; set; }
    }

    public class ReceiptViewModel
    {
        public Booking Booking { get; set; } = new Booking();
        public bool IsAdminView { get; set; }
        /// <summary>When set, the receipt focuses on this single payment (official receipt).</summary>
        public Payment? Payment { get; set; }
    }
}
