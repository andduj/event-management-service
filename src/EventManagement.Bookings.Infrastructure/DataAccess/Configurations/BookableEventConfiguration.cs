using EventManagement.Bookings.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagement.Bookings.Infrastructure.DataAccess.Configurations
{
    internal sealed class BookableEventConfiguration : IEntityTypeConfiguration<BookableEvent>
    {
        public void Configure(EntityTypeBuilder<BookableEvent> builder)
        {
            builder.ToTable("bookable_events");

            builder.HasKey(bookableEvent => bookableEvent.Id);
            builder.Property(bookableEvent => bookableEvent.Id)
                .ValueGeneratedNever();

            builder.Property(bookableEvent => bookableEvent.Title)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(bookableEvent => bookableEvent.StartAt)
                .IsRequired();

            builder.Property(bookableEvent => bookableEvent.EndAt)
                .IsRequired();

            builder.Property(bookableEvent => bookableEvent.TotalSeats)
                .IsRequired();

            builder.Property(bookableEvent => bookableEvent.AvailableSeats)
                .IsRequired();
        }
    }
}
