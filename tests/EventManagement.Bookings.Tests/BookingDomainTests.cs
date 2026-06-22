using EventManagement.Bookings.Domain.Models;
using FluentAssertions;

namespace EventManagement.Bookings.Tests
{
    public class BookingDomainTests
    {
        [Fact]
        public void Cancel_WhenAlreadyCancelled_ShouldBeIdempotent()
        {
            var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
            booking.Cancel();
            var processedAt = booking.ProcessedAt;

            booking.Cancel();

            booking.Status.Should().Be(BookingStatus.Cancelled);
            booking.ProcessedAt.Should().Be(processedAt);
        }

        [Fact]
        public void Cancel_WhenRejected_ShouldThrowInvalidOperationException()
        {
            var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
            booking.Reject();

            var action = () => booking.Cancel();

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void IsActive_WhenPendingOrConfirmed_ShouldBeTrue()
        {
            var pending = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
            pending.IsActive.Should().BeTrue();

            var confirmed = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
            confirmed.Confirm();
            confirmed.IsActive.Should().BeTrue();
        }

        [Fact]
        public void IsActive_WhenCancelledOrRejected_ShouldBeFalse()
        {
            var cancelled = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
            cancelled.Cancel();
            cancelled.IsActive.Should().BeFalse();

            var rejected = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
            rejected.Reject();
            rejected.IsActive.Should().BeFalse();
        }
    }
}
