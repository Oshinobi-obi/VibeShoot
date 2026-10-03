using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VibeShoot.Data;
using VibeShoot.Models;
using VibeShoot.Models.Entities;
using VibeShoot.Services;

namespace VibeShoot.Controllers
{
    /// <summary>Client-side booking tracking, receipts and balance payments (no login needed).</summary>
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MediaStore _media;

        public BookingController(ApplicationDbContext context, MediaStore media)
        {
            _context = context;
            _media = media;
        }

        [HttpGet("booking/track")]
        public IActionResult Track() => View("~/Views/Booking/Track.cshtml", new TrackViewModel());

        [HttpPost("booking/track")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Track(TrackViewModel model)
        {
            var txId = (model.TransactionId ?? "").Trim().ToUpperInvariant();
            var mobile = BookingRules.NormalizeMobile(model.ContactNumber);

            var booking = mobile == null ? null : await _context.Bookings.FirstOrDefaultAsync(b => b.TransactionId == txId);
            if (booking == null || BookingRules.NormalizeMobile(booking.ContactNumber) != mobile)
            {
                model.Error = "We couldn't find a booking with that Transaction ID and mobile number.";
                return View("~/Views/Booking/Track.cshtml", model);
            }

            return RedirectToAction(nameof(Receipt), new { id = booking.TransactionId, t = booking.AccessToken });
        }

        [HttpGet("booking/{id}/receipt")]
        public async Task<IActionResult> Receipt(string id, string t)
        {
            var booking = await LoadAsync(id, t);
            if (booking == null) return RedirectToAction(nameof(Track));

            return View("~/Views/Booking/Receipt.cshtml", new ReceiptViewModel { Booking = booking });
        }

        /// <summary>Lets a client send the remaining balance through GCash from their receipt page.</summary>
        [HttpPost("booking/{id}/pay")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> PayBalance(string id, string t, string referenceNumber, Microsoft.AspNetCore.Http.IFormFile receiptImage)
        {
            var booking = await LoadAsync(id, t);
            if (booking == null) return RedirectToAction(nameof(Track));

            string? error = null;
            var reference = BookingRules.NormalizeReference(referenceNumber);
            var due = booking.Balance - booking.AmountForVerification;

            if (booking.Status is BookingStatus.Declined or BookingStatus.Cancelled) error = "This booking is no longer active.";
            else if (due <= 0) error = "There is no remaining balance to pay.";
            else if (reference == null) error = "Please enter a valid GCash reference number.";
            else if (await _context.Payments.AnyAsync(p => p.ReferenceNumber == reference && p.Status != PaymentStatus.Rejected))
                error = "This GCash reference number has already been used.";
            else error = MediaStore.Validate(receiptImage);

            var proof = error == null ? await _media.AddAsync(receiptImage, MediaKind.Receipt) : null;
            if (error == null && proof == null) error = "Please upload a JPG, PNG or WEBP image of your GCash receipt.";

            if (error != null)
            {
                TempData["PayError"] = error;
                return RedirectToAction(nameof(Receipt), new { id, t });
            }

            booking.Payments.Add(new Payment
            {
                ReceiptNumber = IdGenerator.ReceiptNumber(),
                Amount = due,
                Method = PaymentMethod.GCash,
                Type = PaymentType.Balance,
                ReferenceNumber = reference,
                ProofImagePath = proof,
                Status = PaymentStatus.ForVerification,
            });
            booking.UpdatedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["PaySuccess"] = "Balance payment submitted! The photographer will verify it shortly.";
            return RedirectToAction(nameof(Receipt), new { id, t });
        }

        private async Task<Booking?> LoadAsync(string id, string? token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var booking = await _context.Bookings
                .Include(b => b.Photographer)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.TransactionId == id);

            if (booking == null) return null;

            var ok = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(booking.AccessToken), Encoding.UTF8.GetBytes(token));
            if (!ok) return null;

            booking.Payments = booking.Payments.OrderBy(p => p.CreatedAt).ToList();
            return booking;
        }
    }
}
