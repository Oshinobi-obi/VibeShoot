using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VibeShoot.Data;
using VibeShoot.Models.Admin;
using VibeShoot.Models.Entities;

namespace VibeShoot.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (!await _context.Admins.AnyAsync())
            {
                var newAdmin = new Admin { Username = "admin", Role = "SuperAdmin", CreatedAt = DateTime.UtcNow };
                newAdmin.PasswordHash = new PasswordHasher<Admin>().HashPassword(newAdmin, "Admin123!");
                _context.Admins.Add(newAdmin);
                await _context.SaveChangesAsync();
            }

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }

            return View("~/Views/Admin/Login.cshtml", new AdminLoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            var admin = await _context.Admins
                .Include(a => a.Photographer)
                .FirstOrDefaultAsync(a => a.Username == model.Username);

            if (admin != null)
            {
                var hasher = new PasswordHasher<Admin>();
                var result = hasher.VerifyHashedPassword(admin, admin.PasswordHash, model.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, admin.Username),
                        new Claim("Role", admin.Role),
                        new Claim("PhotographerId", admin.PhotographerId?.ToString() ?? "0")
                    };

                    var identity = new ClaimsIdentity(claims, "VibeShootAdminCookie");
                    await HttpContext.SignInAsync("VibeShootAdminCookie", new ClaimsPrincipal(identity));

                    return RedirectToAction("Dashboard");
                }
            }

            model.ErrorMessage = "Invalid username or password.";
            return View("~/Views/Admin/Login.cshtml", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var role = User.FindFirst("Role")?.Value;
            var photographerIdClaim = User.FindFirst("PhotographerId")?.Value;

            IQueryable<Booking> query = _context.Bookings
                .Include(b => b.Photographer)
                .OrderByDescending(b => b.CreatedAt);
            
            if (role == "Photographer" && int.TryParse(photographerIdClaim, out int photogId) && photogId > 0)
            {
                query = query.Where(b => b.PhotographerId == photogId);
            }

            var bookings = await query.ToListAsync();
            return View("~/Views/Admin/Dashboard.cshtml", bookings);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string transactionId, string status)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.TransactionId == transactionId);
            if (booking != null)
            {
                booking.Status = status;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetScheduleData()
        {
            var photographerIdClaim = User.FindFirst("PhotographerId")?.Value;
            int.TryParse(photographerIdClaim, out int photogId);

            var bookings = await _context.Bookings
                .Where(b => photogId == 0 || b.PhotographerId == photogId)
                .Select(b => new { b.TargetDate, b.Status, b.ClientName })
                .ToListAsync();

            var blockedDates = await _context.BlockedDates
                .Where(b => photogId == 0 || b.PhotographerId == photogId)
                .Select(b => new { b.Date })
                .ToListAsync();

            return Json(new { bookings, blockedDates });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleBlockDate([FromBody] BlockDateRequest request)
        {
            var photographerIdClaim = User.FindFirst("PhotographerId")?.Value;
            int.TryParse(photographerIdClaim, out int photogId);

            if (photogId == 0) photogId = 1;

            if (DateTime.TryParse(request.Date, out DateTime targetDate))
            {
                var existingBlock = await _context.BlockedDates
                    .FirstOrDefaultAsync(b => b.PhotographerId == photogId && b.Date.Date == targetDate.Date);

                if (existingBlock != null)
                {
                    _context.BlockedDates.Remove(existingBlock);
                }
                else
                {
                    _context.BlockedDates.Add(new BlockedDate { PhotographerId = photogId, Date = targetDate });
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Invalid date format.");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("VibeShootAdminCookie");
            return RedirectToAction("Login");
        }
    }
}