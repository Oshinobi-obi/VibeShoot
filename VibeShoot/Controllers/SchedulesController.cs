using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VibeShoot.Data;
using VibeShoot.Models;
using VibeShoot.Models.Entities;
using VibeShoot.Services;

namespace VibeShoot.Controllers
{
    public class SchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ImageStorage _storage;

        public SchedulesController(ApplicationDbContext context, ImageStorage storage)
        {
            _context = context;
            _storage = storage;
        }

        [HttpGet("photographer/{photographer}/schedule")]
        public async Task<IActionResult> Book(string photographer)
        {
            var entity = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer && p.IsActive);
            if (entity == null) return Redirect("/photographers");

            var model = new BookPageViewModel
            {
                Photographer = entity,
                Packages = await _context.Packages
                    .Where(p => p.PhotographerId == entity.Id && p.IsActive)
                    .OrderBy(p => p.Category).ThenBy(p => p.SortOrder)
                    .ToListAsync()
            };

            return View("~/Views/Schedules/Book.cshtml", model);
        }

        /// <summary>Public availability feed for the booking calendar. Contains no client details.</summary>
        [HttpGet("photographer/{photographer}/api/schedule")]
        public async Task<IActionResult> GetPhotographerSchedule(string photographer)
        {
            var photog = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer);
            if (photog == null) return NotFound();

            var today = DateTime.Today;
            var bookings = await _context.Bookings
                .Where(b => b.PhotographerId == photog.Id && b.TargetDate >= today && b.Status != BookingStatus.Declined && b.Status != BookingStatus.Cancelled)
                .Select(b => new { b.TargetDate, b.StartTime, b.EndTime, b.Status })
                .ToListAsync();

            var blocked = await _context.BlockedDates
                .Where(b => b.PhotographerId == photog.Id && b.Date >= today)
                .Select(b => b.Date)
                .ToListAsync();

            var days = bookings
                .GroupBy(b => b.TargetDate.ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => new
                {
                    count = g.Count(),
                    hasConfirmed = g.Any(b => b.Status != BookingStatus.Pending),
                    sessions = g.Select(b => new { start = b.StartTime.ToString(@"hh\:mm"), end = b.EndTime.ToString(@"hh\:mm") })
                });

            return Json(new
            {
                maxPerDay = BookingRules.MaxBookingsPerDay,
                days,
                blocked = blocked.Select(d => d.ToString("yyyy-MM-dd"))
            });
        }

        [HttpPost("photographer/{photographer}/api/book")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> SubmitBooking(string photographer, [FromForm] BookingSubmission form)
        {
            var photog = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer && p.IsActive);
            if (photog == null) return Fail("Photographer not found.");

            if (!form.AcceptedTerms) return Fail("Please read and accept the Terms and Conditions.");
            if (string.IsNullOrWhiteSpace(form.FullName)) return Fail("Please enter your full name.");
            if (string.IsNullOrWhiteSpace(form.Venue)) return Fail("Please enter the venue / location.");

            var mobile = BookingRules.NormalizeMobile(form.ContactNumber);
            if (mobile == null) return Fail("Please enter a valid mobile number (e.g. 0917-123-4567).");

            if (!DateTime.TryParseExact(form.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return Fail("Please choose a date on the calendar.");
            if (date.Date <= DateTime.Today) return Fail("Bookings must be made at least one day in advance.");

            if (!TimeSpan.TryParseExact(form.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out var start))
                return Fail("Please choose a start time.");

            var package = await _context.Packages.FirstOrDefaultAsync(p => p.Id == form.PackageId && p.PhotographerId == photog.Id && p.IsActive);
            if (package == null) return Fail("Please choose a package.");

            var end = start + TimeSpan.FromHours(package.DurationHours);

            if (await _context.BlockedDates.AnyAsync(b => b.PhotographerId == photog.Id && b.Date == date.Date))
                return Fail("The photographer is not available on this date. Please pick another day.");

            var sameDay = await _context.Bookings
                .Where(b => b.PhotographerId == photog.Id && b.TargetDate == date.Date && b.Status != BookingStatus.Declined && b.Status != BookingStatus.Cancelled)
                .ToListAsync();
            var slotError = BookingRules.CheckSlot(start, end, sameDay);
            if (slotError != null) return Fail(slotError);

            // Price is always taken from the database, never from the browser.
            var downPayment = BookingRules.DownPaymentFor(package.Price);
            string? reference = null;
            if (downPayment > 0)
            {
                reference = BookingRules.NormalizeReference(form.ReferenceNumber);
                if (reference == null) return Fail("Please enter the GCash reference number (digits only, as shown on your GCash receipt).");

                var imageError = ImageStorage.Validate(form.ReceiptImage);
                if (imageError != null) return Fail("Receipt screenshot: " + imageError);

                if (await _context.Payments.AnyAsync(p => p.ReferenceNumber == reference && p.Status != PaymentStatus.Rejected))
                    return Fail("This GCash reference number has already been used for another booking.");
            }

            var booking = new Booking
            {
                TransactionId = IdGenerator.TransactionId(),
                AccessToken = IdGenerator.AccessToken(),
                PhotographerId = photog.Id,
                PackageId = package.Id,
                ClientName = form.FullName.Trim(),
                ContactNumber = BookingRules.FormatMobile(mobile),
                Email = form.Email?.Trim(),
                SocialLink = form.SocialLink?.Trim(),
                TargetDate = date.Date,
                StartTime = start,
                EndTime = end,
                Venue = form.Venue.Trim(),
                Category = package.Category,
                PackageName = package.Name,
                TotalPrice = package.Price,
                DownPaymentRequired = downPayment,
                Notes = form.Notes?.Trim(),
                Status = BookingStatus.Pending,
            };

            if (downPayment > 0)
            {
                var proof = await _storage.SaveAsync(form.ReceiptImage!, "Uploads/Receipts", booking.TransactionId);
                booking.Payments.Add(new Payment
                {
                    ReceiptNumber = IdGenerator.ReceiptNumber(),
                    Amount = downPayment,
                    Method = PaymentMethod.GCash,
                    Type = PaymentType.DownPayment,
                    ReferenceNumber = reference,
                    ProofImagePath = proof,
                    Status = PaymentStatus.ForVerification,
                });
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Json(new
            {
                ok = true,
                transactionId = booking.TransactionId,
                receiptUrl = Url.Action("Receipt", "Booking", new { id = booking.TransactionId, t = booking.AccessToken })
            });
        }

        private JsonResult Fail(string message)
        {
            Response.StatusCode = 400;
            return Json(new { ok = false, error = message });
        }
    }
}
