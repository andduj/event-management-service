using EventManagement.Bookings.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagement.Bookings.Infrastructure.DataAccess.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(user => user.Id);
            builder.Property(user => user.Id)
                .ValueGeneratedNever();

            builder.Property(user => user.Login)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(user => user.Login)
                .IsUnique();

            builder.Property(user => user.PasswordHash)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(user => user.Role)
                .HasConversion<string>()
                .IsRequired();
        }
    }
}
