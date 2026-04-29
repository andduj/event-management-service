using EventManagement.Bookings.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Bookings.DataAccess
{
    public sealed class BookingsDbContext : DbContext
    {
        public DbSet<Booking> Bookings => Set<Booking>();

        public BookingsDbContext(DbContextOptions<BookingsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingsDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
