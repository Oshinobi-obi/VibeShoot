using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using VibeShoot.Models.Entities;

namespace VibeShoot.Models
{
    public class PhotographerCard
    {
        public int Id { get; set; }
        public string Slug { get; set; } = "";
        public string Name { get; set; } = "";
        public string Tagline { get; set; } = "";
        public string LogoPath { get; set; } = "";
        public string? CoverImage { get; set; }
        public int PhotoCount { get; set; }
        public decimal? StartingPrice { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
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

        /// <summary>The client's review of this booking, if they already left one.</summary>
        public Review? Review { get; set; }

        /// <summary>True when the session is done and no review exists yet.</summary>
        public bool CanReview { get; set; }
    }

    /// <summary>Reviews block on a photographer's About page.</summary>
    public class ReviewSummary
    {
        public int Count { get; set; }
        public double Average { get; set; }
        /// <summary>Stars (5..1) → number of reviews.</summary>
        public Dictionary<int, int> Breakdown { get; set; } = new Dictionary<int, int>();
        /// <summary>"What clients liked", most-picked first.</summary>
        public List<KeyValuePair<string, int>> TopTags { get; set; } = new List<KeyValuePair<string, int>>();
        public List<Review> Latest { get; set; } = new List<Review>();
    }
}
