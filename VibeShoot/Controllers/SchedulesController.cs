using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
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
            if (string.IsNullOrEmpty(photographer))
            {
                return Redirect("/photographers");
            }

            var entity = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer);
            if (entity == null)
            {
                return Redirect("/photographers");
            }

            var model = new PhotographerProfile
            {
                Slug = entity.Slug,
                Name = entity.Name,
                LogoPath = entity.LogoPath
            };

            return View("~/Views/Schedules/Book.cshtml", model);
        }

        [HttpPost("photographer/{photographer}/schedule")]
        public async Task<IActionResult> Book(string photographer, BookingRequest request)
        {
            var photog = await _context.Photographers.FirstOrDefaultAsync(p => p.Slug == photographer);
            if (photog == null)
            {
                return Redirect("/photographers");
            }

            string savedFileName = "no-receipt.png";

            if (request.ReceiptImage != null && request.ReceiptImage.Length > 0)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "Receipts");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string fileExtension = Path.GetExtension(request.ReceiptImage.FileName);
                savedFileName = $"{request.TransactionId}_{Guid.NewGuid():N}{fileExtension}";
                string filePath = Path.Combine(uploadFolder, savedFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ReceiptImage.CopyToAsync(fileStream);
                }
            }

            DateTime parsedDate;
            if (!DateTime.TryParse(request.Date, out parsedDate))
            {
                parsedDate = DateTime.Today;
            }

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
                ReceiptImagePath = "/Uploads/Receipts/" + savedFileName,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();

            return Redirect($"/photographer/{photographer}/schedule");
        }
    }
}