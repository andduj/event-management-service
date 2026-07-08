using EventManagement.Bookings.Domain.Models;
using FluentAssertions;

namespace EventManagement.Bookings.Tests
{
    public class BookableEventDomainTests
    {
        [Fact]
        public void HasStarted_WhenStartAtInPast_ShouldReturnTrue()
        {
            var bookableEvent = BookableEvent.Create(
                Guid.NewGuid(),
                "Past event",
                null,
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow.AddHours(1),
                10,
                10);

            bookableEvent.HasStarted(DateTime.UtcNow).Should().BeTrue();
        }

        [Fact]
        public void TryReserveSeats_WhenEnoughSeats_ShouldDecreaseAvailableSeats()
        {
            var bookableEvent = BookableEvent.Create(
                Guid.NewGuid(),
                "Event",
                null,
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(2),
                5,
                5);

            bookableEvent.TryReserveSeats(2).Should().BeTrue();
            bookableEvent.AvailableSeats.Should().Be(3);
        }

        [Fact]
        public void ReleaseSeats_ShouldIncreaseAvailableSeats()
        {
            var bookableEvent = BookableEvent.Create(
                Guid.NewGuid(),
                "Event",
                null,
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(2),
                5,
                3);

            bookableEvent.ReleaseSeats(1);

            bookableEvent.AvailableSeats.Should().Be(4);
        }
    }
}
