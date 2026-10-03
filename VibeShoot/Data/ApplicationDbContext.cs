using Microsoft.EntityFrameworkCore;
using VibeShoot.Models.Entities;

namespace VibeShoot.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Photographer> Photographers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<BlockedDate> BlockedDates { get; set; }
    }
}