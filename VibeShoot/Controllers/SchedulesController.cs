using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VibeShoot.Data;
using VibeShoot.Models;
using VibeShoot.Models.Entities;

namespace VibeShoot.Controllers
{
    public class SchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SchedulesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("photographer/{photographer}/schedule")]
        public async Task<IActionResult> Book(string photographer)
        {
            if (string.IsNullOrEmpty(photographer)) return Redirect("/photographers");

            var entity = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer);
            if (entity == null) return Redirect("/photographers");

            var model = new PhotographerProfile
            {
                Slug = entity.Slug,
                Name = entity.Name,
                LogoPath = entity.LogoPath
            };

            return View("~/Views/Schedules/Book.cshtml", model);
        }

        [HttpPost("photographer/{photographer}/schedule")]
        public async Task<IActionResult> Book(string photographer, BookingRequest request, [FromForm] string TimeSlot, [FromForm] string EndTime)
        {
            var photog = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer);
            if (photog == null) return Redirect("/photographers");

            DateTime parsedDate = DateTime.TryParse(request.Date, out var pd) ? pd : DateTime.Today;

            if (!TimeSpan.TryParse(TimeSlot, out TimeSpan reqStart) || !TimeSpan.TryParse(EndTime, out TimeSpan reqEnd))
            {
                TempData["ErrorMessage"] = "Invalid time format selected.";
                return Redirect($"/photographer/{photographer}/schedule");
            }

            var existingBookings = await _context.Bookings
                .Where(b => b.PhotographerId == photog.Id && b.TargetDate.Date == parsedDate.Date && b.Status != "Declined")
                .ToListAsync();

            if (existingBookings.Count >= 2)
            {
                TempData["ErrorMessage"] = "Sorry! This date has reached the maximum limit of 2 bookings.";
                return Redirect($"/photographer/{photographer}/schedule");
            }

            foreach (var existing in existingBookings)
            {
                TimeSpan exStart, exEnd;

                if (DateTime.TryParse(existing.TimeSlot, out DateTime ts)) exStart = ts.TimeOfDay;
                else TimeSpan.TryParse(existing.TimeSlot, out exStart);

                if (DateTime.TryParse(existing.EndTime, out DateTime te)) exEnd = te.TimeOfDay;
                else TimeSpan.TryParse(existing.EndTime, out exEnd);

                if (!(reqStart >= exEnd.Add(TimeSpan.FromHours(2)) || reqEnd.Add(TimeSpan.FromHours(2)) <= exStart))
                {
                    TempData["ErrorMessage"] = "Your selected time does not leave the required 2-hour preparation interval from another booking on this day.";
                    return Redirect($"/photographer/{photographer}/schedule");
                }
            }

            string savedFileName = "no-receipt.png";
            if (request.ReceiptImage != null && request.ReceiptImage.Length > 0)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "Receipts");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                string fileExtension = Path.GetExtension(request.ReceiptImage.FileName);
                savedFileName = $"{request.TransactionId}_{Guid.NewGuid():N}{fileExtension}";
                string filePath = Path.Combine(uploadFolder, savedFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ReceiptImage.CopyToAsync(fileStream);
                }
            }

            string formattedStart = DateTime.Today.Add(reqStart).ToString("hh:mm tt");
            string formattedEnd = DateTime.Today.Add(reqEnd).ToString("hh:mm tt");

            var newBooking = new Booking
            {
                TransactionId = string.IsNullOrEmpty(request.TransactionId) ? Guid.NewGuid().ToString().Substring(0, 16) : request.TransactionId,
                PhotographerId = photog.Id,
                ClientName = request.FullName,
                ContactNumber = request.ContactNumber,
                SocialLink = request.SocialLink,
                TargetDate = parsedDate,
                Venue = request.Venue,
                Category = request.Category,
                PackageName = request.SelectedPackage,
                AmountPaid = request.AmountPaid,
                TimeSlot = formattedStart,
                EndTime = formattedEnd,
                ReceiptImagePath = "/Uploads/Receipts/" + savedFileName,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();

            return Redirect($"/photographer/{photographer}/schedule");
        }

        [HttpGet("photographer/{photographer}/api/schedule")]
        public async Task<IActionResult> GetPhotographerSchedule(string photographer)
        {
            var photog = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer);
            if (photog == null) return NotFound();

            var bookings = await _context.Bookings
                .Where(b => b.PhotographerId == photog.Id && b.Status != "Declined")
                .Select(b => new { b.TargetDate, b.Status })
                .ToListAsync();

            var blockedDates = await _context.BlockedDates
                .Where(b => b.PhotographerId == photog.Id)
                .Select(b => new { b.Date })
                .ToListAsync();

            return Json(new { bookings, blockedDates });
        }
    }
}