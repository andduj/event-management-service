using AutoFixture;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Models;
using EventManagement.Events.Api;
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
                .Setup(repository => repository.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((booking, _) => updatedBookings.Add(booking))
                .Returns(Task.CompletedTask);

            var eventsClient = new Mock<IEventsClient>();
            eventsClient
                .Setup(client => client.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = new BookingProcessingService(bookingRepository.Object, eventsClient.Object, new Mock<ILogger<BookingProcessingService>>().Object);

            await service.ProcessPendingBookingsAsync(CancellationToken.None);

            updatedBookings.Should().OnlyContain(booking =>
                booking.Status == BookingStatus.Confirmed);
        }
    }
}
