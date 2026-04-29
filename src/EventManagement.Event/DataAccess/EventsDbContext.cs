using EventManagement.Events.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Events.DataAccess
{
    public sealed class EventsDbContext : DbContext
    {
        public DbSet<Event> Events => Set<Event>();

        public EventsDbContext(DbContextOptions<EventsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventsDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
