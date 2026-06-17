using AutoFixture;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Domain.Models;
using FluentAssertions;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingProcessingServiceTests : IClassFixture<BookingProcessingServiceFixture>
    {
        private readonly Mock<IBookingRepository> _bookingRepository;
        private readonly Mock<IEventsGateway> _eventsGateway;
        private readonly BookingProcessingService _bookingProcessingService;
        private readonly BookingService _bookingService;
        private readonly IFixture _fixtureData;
        private readonly BookingProcessingServiceFixture _fixture;

        public BookingProcessingServiceTests(BookingProcessingServiceFixture fixture)
        {
            _fixture = fixture;
            _bookingRepository = fixture.BookingRepository;
            _eventsGateway = fixture.EventsGateway;
            _bookingProcessingService = fixture.BookingProcessingService;
            _bookingService = fixture.BookingService;
            _fixtureData = fixture.Fixture;

            _bookingRepository.Reset();
            _eventsGateway.Reset();
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
        public void Confirm_WhenBookingAlreadyConfirmed_ShouldBeIdempotent()
        {
            var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
            booking.Confirm();
            var processedAt = booking.ProcessedAt;

            booking.Confirm();

            booking.Status.Should().Be(BookingStatus.Confirmed);
            booking.ProcessedAt.Should().Be(processedAt);
        }

        [Fact]
        public void Reject_WhenBookingAlreadyRejected_ShouldBeIdempotent()
        {
            var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
            booking.Reject();
            var processedAt = booking.ProcessedAt;

            booking.Reject();

            booking.Status.Should().Be(BookingStatus.Rejected);
            booking.ProcessedAt.Should().Be(processedAt);
        }

        [Fact]
        public void Confirm_WhenBookingAlreadyRejected_ShouldThrowInvalidOperationException()
        {
            var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid());
            booking.Reject();

            var action = () => booking.Confirm();

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task ProcessPendingBookingsAsync_PendingBookings_ShouldConfirmAllBookings()
        {
            var bookings = _fixtureData.CreateMany<Booking>(2).ToList();
            var updatedBookings = new List<Booking>();
            _bookingRepository
                .Setup(repository => repository.GetBookingsAsync(BookingStatus.Pending))
                .ReturnsAsync(bookings);
            foreach (var booking in bookings)
            {
                _bookingRepository
                    .Setup(repository => repository.GetBookingByIdAsync(booking.Id))
                    .ReturnsAsync(booking);
            }
            _bookingRepository
                .Setup(repository => repository.TryUpdateBookingAsync(It.IsAny<Booking>(), BookingStatus.Pending, It.IsAny<CancellationToken>()))
                .Callback<Booking, BookingStatus, CancellationToken>((booking, _, _) => updatedBookings.Add(booking))
                .ReturnsAsync(true);

            _eventsGateway
                .Setup(gateway => gateway.EventExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await ProcessAllPendingBookingsAsync();

            updatedBookings.Should().HaveCount(2);
            updatedBookings.Should().OnlyContain(booking =>
                booking.Status == BookingStatus.Confirmed);
            updatedBookings.Should().OnlyContain(booking => booking.ProcessedAt.HasValue);
        }

        [Fact]
        public async Task ProcessPendingBookingsAsync_WhenEventDoesNotExist_ShouldRejectBookingAndReleaseSeat()
        {
            var eventId = Guid.NewGuid();
            var pendingBooking = Booking.Create(eventId, Guid.NewGuid());
            var bookings = new List<Booking> { pendingBooking };
            var updatedBookings = new List<Booking>();
            _bookingRepository
                .Setup(repository => repository.GetBookingsAsync(BookingStatus.Pending))
                .ReturnsAsync(bookings);
            _bookingRepository
                .Setup(repository => repository.GetBookingByIdAsync(pendingBooking.Id))
                .ReturnsAsync(pendingBooking);
            _bookingRepository
                .Setup(repository => repository.TryUpdateBookingAsync(It.IsAny<Booking>(), BookingStatus.Pending, It.IsAny<CancellationToken>()))
                .Callback<Booking, BookingStatus, CancellationToken>((booking, _, _) => updatedBookings.Add(booking))
                .ReturnsAsync(true);

            var releasedSeats = 0;
            _eventsGateway
                .Setup(gateway => gateway.EventExistsAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _eventsGateway
                .Setup(gateway => gateway.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()))
                .Callback(() => releasedSeats++)
                .Returns(Task.CompletedTask);

            await ProcessAllPendingBookingsAsync();

            updatedBookings.Should().ContainSingle();
            updatedBookings.Single().Status.Should().Be(BookingStatus.Rejected);
            releasedSeats.Should().Be(1);
            _eventsGateway.Verify(gateway => gateway.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPendingBookingsAsync_WhenProcessingFails_ShouldRejectBookingAndReleaseSeat()
        {
            var eventId = Guid.NewGuid();
            var pendingBooking = Booking.Create(eventId, Guid.NewGuid());
            var bookings = new List<Booking> { pendingBooking };
            var updatedBookings = new List<Booking>();
            _bookingRepository
                .Setup(repository => repository.GetBookingsAsync(BookingStatus.Pending))
                .ReturnsAsync(bookings);
            _bookingRepository
                .Setup(repository => repository.GetBookingByIdAsync(pendingBooking.Id))
                .ReturnsAsync(pendingBooking);
            _bookingRepository
                .Setup(repository => repository.TryUpdateBookingAsync(It.IsAny<Booking>(), BookingStatus.Pending, It.IsAny<CancellationToken>()))
                .Callback<Booking, BookingStatus, CancellationToken>((booking, _, _) => updatedBookings.Add(booking))
                .ReturnsAsync(true);

            var availableSeats = 0;
            _eventsGateway
                .Setup(gateway => gateway.EventExistsAsync(eventId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Processing failed"));
            _eventsGateway
                .Setup(gateway => gateway.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()))
                .Callback(() => availableSeats++)
                .Returns(Task.CompletedTask);

            await ProcessAllPendingBookingsAsync();

            updatedBookings.Should().ContainSingle();
            updatedBookings.Single().Status.Should().Be(BookingStatus.Rejected);
            updatedBookings.Single().ProcessedAt.Should().NotBeNull();
            availableSeats.Should().Be(1);
            _eventsGateway.Verify(gateway => gateway.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessBookingAsync_WhenAlreadyProcessedByAnotherWorker_ShouldNotReleaseSeatAgain()
        {
            var eventId = Guid.NewGuid();
            var pendingBooking = Booking.Create(eventId, Guid.NewGuid());
            var updateAttempts = 0;

            _bookingRepository
                .Setup(repository => repository.GetBookingByIdAsync(pendingBooking.Id))
                .ReturnsAsync(pendingBooking);
            _bookingRepository
                .Setup(repository => repository.TryUpdateBookingAsync(It.IsAny<Booking>(), BookingStatus.Pending, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => Interlocked.Increment(ref updateAttempts) == 1);
            _eventsGateway
                .Setup(gateway => gateway.EventExistsAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var tasks = Enumerable.Range(0, 2)
                .Select(_ => _bookingProcessingService.ProcessBookingAsync(pendingBooking.Id, CancellationToken.None));
            await Task.WhenAll(tasks);

            _eventsGateway.Verify(
                gateway => gateway.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessPendingBookingsAsync_AfterSeatReleased_ShouldAllowNewBookingCreation()
        {
            var eventId = Guid.NewGuid();
            var availableSeats = 1;
            var pendingBookings = new List<Booking>
            {
                Booking.Create(eventId, Guid.NewGuid())
            };
            _bookingRepository
                .Setup(repository => repository.GetBookingsAsync(BookingStatus.Pending))
                .ReturnsAsync(pendingBookings);
            _bookingRepository
                .Setup(repository => repository.GetBookingByIdAsync(pendingBookings[0].Id))
                .ReturnsAsync(pendingBookings[0]);
            _bookingRepository
                .Setup(repository => repository.TryUpdateBookingAsync(It.IsAny<Booking>(), BookingStatus.Pending, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _bookingRepository
                .Setup(repository => repository.CreateBookingAsync(It.IsAny<Booking>()))
                .ReturnsAsync((Booking booking) => booking);

            _eventsGateway
                .Setup(gateway => gateway.EventExistsAsync(eventId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Processing failed"));
            _eventsGateway
                .Setup(gateway => gateway.EnsureEventExistsAsync(eventId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _eventsGateway
                .Setup(gateway => gateway.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()))
                .Callback(() => availableSeats++)
                .Returns(Task.CompletedTask);
            _eventsGateway
                .Setup(gateway => gateway.ReserveSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    if (availableSeats <= 0)
                    {
                        return false;
                    }

                    availableSeats--;
                    return true;
                });

            await ProcessAllPendingBookingsAsync();

            var action = () => _bookingService.CreateBookingAsync(eventId);

            await action.Should().NotThrowAsync();
        }

        private async Task ProcessAllPendingBookingsAsync(CancellationToken cancellationToken = default)
        {
            var bookingIds = await _bookingProcessingService.GetPendingBookingIdsAsync(cancellationToken);
            foreach (var bookingId in bookingIds)
            {
                await _bookingProcessingService.ProcessBookingAsync(bookingId, cancellationToken);
            }
        }
    }
}
