using AutoFixture;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Models;
using EventManagement.Logging;
using FluentAssertions;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingProcessingServiceTests
    {
        [Fact]
        public async Task ProcessPendingBookingsAsync_PendingBookings_ShouldConfirmAllBookings()
        {
            var fixture = new Fixture();
            var bookings = fixture
                .Build<Booking>()
                .With(booking => booking.Status, BookingStatus.Pending)
                .CreateMany(2)
                .ToList();

            var bookingRepository = new Mock<IBookingRepository>();
            bookingRepository
                .Setup(repository => repository.GetBookingsAsync(It.IsAny<BookingStatus>()))
                .ReturnsAsync(bookings);

            var updatedBookings = new List<Booking>();
            bookingRepository
                .Setup(repository => repository.UpdateBookingAsync(It.IsAny<Booking>()))
                .Callback<Booking>(updatedBookings.Add)
                .Returns(Task.CompletedTask);

            var service = new BookingProcessingService(bookingRepository.Object, new Mock<ILogger<BookingProcessingService>>().Object);

            await service.ProcessPendingBookingsAsync(CancellationToken.None);

            updatedBookings.Should().OnlyContain(booking =>
                booking.Status == BookingStatus.Confirmed);
        }
    }
}
