using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Exceptions;
using EventManagement.Bookings.Models;
using EventManagement.Events.Api;
using FluentAssertions;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingServiceTests : IClassFixture<BookingServiceFixture>
    {
        public readonly BookingService _bookingService;
        public readonly Mock<IEventsClient> _eventsClient;
        public readonly IBookingRepository _bookingRepository;

        public BookingServiceTests(BookingServiceFixture fixture) 
        {
            _bookingService = fixture.BookingService;
            _eventsClient = fixture.EventsClient;
            _bookingRepository = fixture.BookingRepository;

            _eventsClient.Reset();
        }

        [Fact]
        public async Task CreateBookingAsync_ExistedEvent_Success()
        {
            var bookingInfo = await _bookingService.CreateBookingAsync(Guid.NewGuid());

            bookingInfo.Status.Should().Be(BookingStatus.Pending);
        }

        [Fact]
        public async Task CreateBookingAsync_MultipleEventBooking_ShouldReturnDifferentBookingId()
        {
            var bookingInfoIds = new List<Guid>();
            var eventId = Guid.NewGuid();

            for (int i = 0; i < 10; i++)
            {
                var bookingInfo = await _bookingService.CreateBookingAsync(eventId);
                bookingInfoIds.Add(bookingInfo.Id);
            }

            bookingInfoIds.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task GetBookingByIdAsync_ExistedBooking_Success()
        {
            var bookingInfo = await _bookingService.CreateBookingAsync(Guid.NewGuid());

            bookingInfo.Status.Should().Be(BookingStatus.Pending);

            var booking = await _bookingService.GetBookingByIdAsync(bookingInfo.Id);

            booking.Should().NotBeNull();
            booking.Id.Should().Be(bookingInfo.Id);
        }

        [Fact]
        public async Task GetBookingByIdAsync_UpdatedBookingStatus_ShouldReturnActualStatus()
        {
            var bookingInfo = await _bookingService.CreateBookingAsync(Guid.NewGuid());
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingInfo.Id);
            booking.Status = BookingStatus.Confirmed;
            await _bookingRepository.UpdateBookingAsync(booking);

            var updatedBooking = await _bookingService.GetBookingByIdAsync(bookingInfo.Id);

            updatedBooking.Status.Should().Be(BookingStatus.Confirmed);
        }

        [Fact]
        public async Task GetBookingByIdAsync_NotExistedBooking_ShouldBookingNotFoundException()
        {
            var action = () => _bookingService.GetBookingByIdAsync(Guid.NewGuid());

            await action.Should().ThrowAsync<BookingNotFoundException>();

        }

        [Fact]
        public async Task CreateBookingAsync_NotExistedEvent_ShouldApiException()
        {
            _eventsClient
                .Setup(client => client.EventsGetAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new ApiException("Event not found", 404, string.Empty, null, null));

            var action = () => _bookingService.CreateBookingAsync(Guid.NewGuid());

            await action.Should().ThrowAsync<ApiException>();

        }
    }
}
