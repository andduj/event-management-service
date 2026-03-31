using AutoMapper;
using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Data.Repositories;
using EventManagement.Bookings.Exceptions;
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
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            var bookingService = new BookingService(new InMemoryBookingRepository(), new Mock<IEventsClient>().Object, mapper, new Mock<ILogger<BookingService>>().Object);

            var bookingInfo = await bookingService.CreateBookingAsync(Guid.NewGuid());

            bookingInfo.Status.Should().Be(BookingStatus.Pending);
        }

        [Fact]
        public async Task CreateBookingAsync_MultipleEventBooking_ShouldReturnDifferentBookingId()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            var bookingService = new BookingService(new InMemoryBookingRepository(), new Mock<IEventsClient>().Object, mapper, new Mock<ILogger<BookingService>>().Object);

            var bookingInfoIds = new List<Guid>();
            var eventId = Guid.NewGuid();

            for (int i = 0; i < 10; i++)
            {
                var bookingInfo = await bookingService.CreateBookingAsync(eventId);
                bookingInfoIds.Add(bookingInfo.Id);
            }

            bookingInfoIds.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task GetBookingByIdAsync_ExistedBooking_Success()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            var bookingService = new BookingService(new InMemoryBookingRepository(), new Mock<IEventsClient>().Object, mapper, new Mock<ILogger<BookingService>>().Object);

            var bookingInfo = await bookingService.CreateBookingAsync(Guid.NewGuid());

            bookingInfo.Status.Should().Be(BookingStatus.Pending);

            var booking = await bookingService.GetBookingByIdAsync(bookingInfo.Id);

            booking.Should().NotBeNull();
            booking.Id.Should().Be(bookingInfo.Id);
        }

        [Fact]
        public async Task GetBookingByIdAsync_NotExistedBooking_ShouldBookingNotFoundException()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            var bookingService = new BookingService(new InMemoryBookingRepository(), new Mock<IEventsClient>().Object, mapper, new Mock<ILogger<BookingService>>().Object);            

            var action = () => bookingService.GetBookingByIdAsync(Guid.NewGuid());

            await action.Should().ThrowAsync<BookingNotFoundException>();

        }

        [Fact]
        public async Task CreateBookingAsync_ExistedEvent_ShouldApiException()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            var eventsClient = new Mock<IEventsClient>();
            eventsClient
                .Setup(client => client.EventsGetAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => { throw new ApiException(); });

            var bookingService = new BookingService(new InMemoryBookingRepository(), eventsClient.Object, mapper, new Mock<ILogger<BookingService>>().Object);

            var action = () => bookingService.GetBookingByIdAsync(Guid.NewGuid());

            await action.Should().ThrowAsync<BookingNotFoundException>();

        }
    }
}
