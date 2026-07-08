using EventManagement.Bookings.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Bookings.Infrastructure.DataAccess
{
    public sealed class BookingsDbContext : DbContext
    {
        public DbSet<Booking> Bookings => Set<Booking>();

        public DbSet<BookableEvent> BookableEvents => Set<BookableEvent>();

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
