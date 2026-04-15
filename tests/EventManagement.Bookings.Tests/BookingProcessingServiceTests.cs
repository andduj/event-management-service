using AutoFixture;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Models;
using EventManagement.Events.Api;
using FluentAssertions;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingProcessingServiceTests : IClassFixture<BookingProcessingServiceFixture>
    {
        private readonly Mock<IBookingRepository> _bookingRepository;
        private readonly Mock<IEventsClient> _eventsClient;
        private readonly BookingProcessingService _bookingProcessingService;
        private readonly BookingService _bookingService;
        private readonly IFixture _fixtureData;
        private readonly BookingProcessingServiceFixture _fixture;

        public BookingProcessingServiceTests(BookingProcessingServiceFixture fixture)
        {
            _fixture = fixture;
            _bookingRepository = fixture.BookingRepository;
            _eventsClient = fixture.EventsClient;
            _bookingProcessingService = fixture.BookingProcessingService;
            _bookingService = fixture.BookingService;
            _fixtureData = fixture.Fixture;

            _bookingRepository.Reset();
            _eventsClient.Reset();
        }

        [Fact]
        public void Confirm_WhenCalled_ShouldSetConfirmedStatusAndProcessedAt()
        {
            var booking = _fixtureData.Create<Booking>();

            booking.Confirm();

            booking.Status.Should().Be(BookingStatus.Confirmed);
            booking.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public void Reject_WhenCalled_ShouldSetRejectedStatusAndProcessedAt()
        {
            var booking = _fixtureData.Create<Booking>();

            booking.Reject();

            booking.Status.Should().Be(BookingStatus.Rejected);
            booking.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task ProcessPendingBookingsAsync_PendingBookings_ShouldConfirmAllBookings()
        {
            var bookings = _fixtureData.CreateMany<Booking>(2).ToList();
            var updatedBookings = new List<Booking>();
            _bookingRepository
                .Setup(repository => repository.GetBookingsAsync(BookingStatus.Pending))
                .ReturnsAsync(bookings);
            _bookingRepository
                .Setup(repository => repository.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((booking, _) => updatedBookings.Add(booking))
                .Returns(Task.CompletedTask);

            _eventsClient
                .Setup(client => client.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _bookingProcessingService.ProcessPendingBookingsAsync(CancellationToken.None);

            updatedBookings.Should().HaveCount(2);
            updatedBookings.Should().OnlyContain(booking =>
                booking.Status == BookingStatus.Confirmed);
            updatedBookings.Should().OnlyContain(booking => booking.ProcessedAt.HasValue);
        }

        [Fact]
        public async Task ProcessPendingBookingsAsync_WhenProcessingFails_ShouldRejectBookingAndReleaseSeat()
        {
            var eventId = Guid.NewGuid();
            var pendingBooking = _fixtureData
                .Build<Booking>()
                .With(booking => booking.EventId, eventId)
                .Create();
            var bookings = new List<Booking> { pendingBooking };
            var updatedBookings = new List<Booking>();
            _bookingRepository
                .Setup(repository => repository.GetBookingsAsync(BookingStatus.Pending))
                .ReturnsAsync(bookings);
            _bookingRepository
                .Setup(repository => repository.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((booking, _) => updatedBookings.Add(booking))
                .Returns(Task.CompletedTask);

            var availableSeats = 0;
            _eventsClient
                .Setup(client => client.ExistsAsync(eventId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Processing failed"));
            _eventsClient
                .Setup(client => client.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()))
                .Callback(() => availableSeats++)
                .Returns(Task.CompletedTask);

            await _bookingProcessingService.ProcessPendingBookingsAsync(CancellationToken.None);

            updatedBookings.Should().ContainSingle();
            updatedBookings.Single().Status.Should().Be(BookingStatus.Rejected);
            updatedBookings.Single().ProcessedAt.Should().NotBeNull();
            availableSeats.Should().Be(1);
            _eventsClient.Verify(client => client.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPendingBookingsAsync_AfterSeatReleased_ShouldAllowNewBookingCreation()
        {
            var eventId = Guid.NewGuid();
            var availableSeats = 1;
            var pendingBookings = new List<Booking>
            {
                _fixtureData
                    .Build<Booking>()
                    .With(booking => booking.EventId, eventId)
                    .Create()
            };
            _bookingRepository
                .Setup(repository => repository.GetBookingsAsync(BookingStatus.Pending))
                .ReturnsAsync(pendingBookings);
            _bookingRepository
                .Setup(repository => repository.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _bookingRepository
                .Setup(repository => repository.CreateBookingAsync(It.IsAny<Booking>()))
                .ReturnsAsync((Booking booking) => booking);

            _eventsClient
                .Setup(client => client.ExistsAsync(eventId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Processing failed"));
            _eventsClient
                .Setup(client => client.EventsGetAsync(eventId))
                .ReturnsAsync(() => _fixture.CreateTestEvent(eventId, totalSeats: 1, availableSeats));
            _eventsClient
                .Setup(client => client.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()))
                .Callback(() => availableSeats++)
                .Returns(Task.CompletedTask);
            _eventsClient
                .Setup(client => client.ReserveSeatsAsync(eventId, 1))
                .ReturnsAsync(() =>
                {
                    if (availableSeats <= 0)
                    {
                        return false;
                    }

                    availableSeats--;
                    return true;
                });

            await _bookingProcessingService.ProcessPendingBookingsAsync(CancellationToken.None);

            var action = () => _bookingService.CreateBookingAsync(eventId);

            await action.Should().NotThrowAsync();
        }
    }
}
