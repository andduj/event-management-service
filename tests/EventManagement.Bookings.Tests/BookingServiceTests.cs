using AutoFixture;
using AutoMapper;
using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Models;
using EventManagement.Events.Api;
using EventManagement.Logging;
using FluentAssertions;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingServiceTests
    {
        [Fact]
        public async Task CreateBookingAsync_ExistedEvent_Success()
        {
            var booking = new Fixture().Create<Booking>();
            booking.Status = BookingStatus.Pending;

            var bookingRepsitory = new Mock<IBookingRepository>();
            bookingRepsitory.Setup(repsitory => repsitory.CreateBookingAsync(booking))
                .ReturnsAsync(booking);

            var eventsClient = new Mock<IEventsClient>();

            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            var bookingService = new BookingService(bookingRepsitory.Object, eventsClient.Object, mapper, new Mock<ILogger<BookingService>>().Object);

            var bookingInfo = await bookingService.CreateBookingAsync(Guid.NewGuid());

            bookingInfo.Status.Should().Be(BookingStatus.Pending);
        }
    }
}
