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
        private readonly Mock<IBookableEventRepository> _bookableEventRepository;
        private readonly Mock<IBookingConfirmedPublisher> _bookingConfirmedPublisher;
        private readonly BookingProcessingService _bookingProcessingService;
        private readonly IFixture _fixtureData;
        private readonly BookingProcessingServiceFixture _fixture;

        public BookingProcessingServiceTests(BookingProcessingServiceFixture fixture)
        {
            _fixture = fixture;
            _bookingRepository = fixture.BookingRepository;
            _bookableEventRepository = fixture.BookableEventRepository;
            _bookingConfirmedPublisher = fixture.BookingConfirmedPublisher;
            _bookingProcessingService = fixture.BookingProcessingService;
            _fixtureData = fixture.Fixture;

            _bookingRepository.Reset();
            _bookableEventRepository.Reset();
            _bookingConfirmedPublisher.Reset();
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

            _bookableEventRepository
                .Setup(repository => repository.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await ProcessAllPendingBookingsAsync();

            updatedBookings.Should().HaveCount(2);
            updatedBookings.Should().OnlyContain(booking =>
                booking.Status == BookingStatus.Confirmed);
            updatedBookings.Should().OnlyContain(booking => booking.ProcessedAt.HasValue);
            _bookingConfirmedPublisher.Verify(
                publisher => publisher.PublishConfirmedAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
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

            _bookableEventRepository
                .Setup(repository => repository.ExistsAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _bookableEventRepository
                .Setup(repository => repository.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await ProcessAllPendingBookingsAsync();

            updatedBookings.Should().ContainSingle();
            updatedBookings.Single().Status.Should().Be(BookingStatus.Rejected);
            _bookableEventRepository.Verify(
                repository => repository.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()),
                Times.Once);
            _bookingConfirmedPublisher.Verify(
                publisher => publisher.PublishConfirmedAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessBookingAsync_WhenAlreadyProcessedByAnotherWorker_ShouldPersistConfirmationOnlyOnce()
        {
            var eventId = Guid.NewGuid();
            var pendingBooking = Booking.Create(eventId, Guid.NewGuid());
            var successfulUpdates = 0;

            _bookingRepository
                .Setup(repository => repository.GetBookingByIdAsync(pendingBooking.Id))
                .ReturnsAsync(pendingBooking);
            _bookingRepository
                .Setup(repository => repository.TryUpdateBookingAsync(It.IsAny<Booking>(), BookingStatus.Pending, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => Interlocked.Increment(ref successfulUpdates) == 1);
            _bookableEventRepository
                .Setup(repository => repository.ExistsAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var tasks = Enumerable.Range(0, 2)
                .Select(_ => _bookingProcessingService.ProcessBookingAsync(pendingBooking.Id, CancellationToken.None));
            await Task.WhenAll(tasks);

            successfulUpdates.Should().Be(1);
            pendingBooking.Status.Should().Be(BookingStatus.Confirmed);
            _bookingConfirmedPublisher.Verify(
                publisher => publisher.PublishConfirmedAsync(pendingBooking, It.IsAny<CancellationToken>()),
                Times.Once);
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
