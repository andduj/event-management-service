using EventManagement.Auth.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Auth.Infrastructure.DataAccess
{
    public sealed class AuthDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();

        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
