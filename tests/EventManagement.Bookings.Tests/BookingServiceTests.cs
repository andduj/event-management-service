using EventManagement.Bookings.Application.DTOs;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Exceptions;
using EventManagement.Bookings.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;

namespace EventManagement.Bookings.Tests
{
    public class BookingServiceTests : IClassFixture<BookingServiceFixture>
    {
        public readonly IBookingService _bookingService;
        public readonly Mock<IEventsGateway> _eventsGateway;
        public readonly IBookingRepository _bookingRepository;
        public readonly BookingServiceFixture _fixture;

        public BookingServiceTests(BookingServiceFixture fixture)
        {
            _fixture = fixture;
            _bookingService = fixture.BookingService;
            _eventsGateway = fixture.EventsGateway;
            _bookingRepository = fixture.BookingRepository;

            _eventsGateway
                .Setup(gateway => gateway.ReserveSeatsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _eventsGateway
                .Setup(gateway => gateway.EnsureEventExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
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
        public async Task CreateBookingAsync_CreateBooking_ShouldDecreaseAvailableSeatsByOne()
        {
            var eventId = Guid.NewGuid();
            int availableSeats = 3;

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

            await _bookingService.CreateBookingAsync(eventId);

            availableSeats.Should().Be(2);
            _eventsGateway.Verify(gateway => gateway.ReserveSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_MultipleBookingsUntilLimit_AllShouldBeSuccessfulAndUnique()
        {
            const int limit = 3;
            var eventId = Guid.NewGuid();
            int availableSeats = limit;
            var createdBookingIds = new List<Guid>();

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

            for (int i = 0; i < limit; i++)
            {
                var bookingInfo = await _bookingService.CreateBookingAsync(eventId);
                createdBookingIds.Add(bookingInfo.Id);
            }

            createdBookingIds.Should().HaveCount(limit);
            createdBookingIds.Should().OnlyHaveUniqueItems();
            availableSeats.Should().Be(0);
        }

        [Fact]
        public async Task CreateBookingAsync_WhenSeatsAreExhausted_ShouldThrowNoAvailableSeatsException()
        {
            const int limit = 2;
            var eventId = Guid.NewGuid();
            int availableSeats = limit;

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

            for (int i = 0; i < limit; i++)
            {
                await _bookingService.CreateBookingAsync(eventId);
            }

            var action = () => _bookingService.CreateBookingAsync(eventId);

            await action.Should().ThrowAsync<NoAvailableSeatsException>();
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
        public async Task CreateBookingAsync_NotExistedEvent_ShouldEventsGatewayExceptionWithNotFoundStatus()
        {
            _eventsGateway
                .Setup(gateway => gateway.EnsureEventExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new EventsGatewayException("Мероприятие не найдено", 404));

            var action = () => _bookingService.CreateBookingAsync(Guid.NewGuid());

            var exception = await action.Should().ThrowAsync<EventsGatewayException>();
            exception.Which.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateBookingAsync_DeletedEvent_ShouldEventsGatewayException()
        {
            _eventsGateway
                .Setup(gateway => gateway.EnsureEventExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new EventsGatewayException("Мероприятие было удалено", 410));

            var action = () => _bookingService.CreateBookingAsync(Guid.NewGuid());

            var exception = await action.Should().ThrowAsync<EventsGatewayException>();
            exception.Which.StatusCode.Should().Be((int)HttpStatusCode.Gone);
        }

        [Fact]
        public async Task CreateBookingAsync_WhenNoAvailableSeats_ShouldThrowNoAvailableSeatsException()
        {
            _eventsGateway
                .Setup(gateway => gateway.ReserveSeatsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = () => _bookingService.CreateBookingAsync(Guid.NewGuid());

            await action.Should().ThrowAsync<NoAvailableSeatsException>();
        }

        [Fact]
        public async Task CreateBookingAsync_ConcurrentRequests_ShouldPreventOverbooking()
        {
            const int totalSeats = 5;
            const int concurrentRequests = 20;
            var eventId = Guid.NewGuid();
            int availableSeats = totalSeats;
            var seatsLock = new object();
            var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            _eventsGateway
                .Setup(gateway => gateway.ReserveSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    lock (seatsLock)
                    {
                        if (availableSeats <= 0)
                        {
                            return false;
                        }

                        availableSeats--;
                        return true;
                    }
                });

            var tasks = Enumerable.Range(0, concurrentRequests)
                .Select(_ => Task.Run(async () =>
                {
                    await startSignal.Task;
                    return await CreateBookingWithResultAsyncInScope(eventId);
                }));

            startSignal.SetResult();

            var results = await Task.WhenAll(tasks);
            var successfulBookingIds = results
                .Where(result => result.Success)
                .Select(result => result.BookingInfo!.Id)
                .ToList();

            results.Count(result => result.Success).Should().Be(totalSeats);
            results.Count(result => result.Exception is NoAvailableSeatsException).Should().Be(concurrentRequests - totalSeats);
            results.Count(result => !result.Success && result.Exception is not NoAvailableSeatsException).Should().Be(0);
            successfulBookingIds.Should().OnlyHaveUniqueItems();
            availableSeats.Should().Be(0);
        }

        [Fact]
        public async Task CreateBookingAsync_ConcurrentRequests_ShouldReturnUniqueBookingIds()
        {
            const int totalSeats = 10;
            var eventId = Guid.NewGuid();
            int availableSeats = totalSeats;
            var seatsLock = new object();
            var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            _eventsGateway
                .Setup(gateway => gateway.ReserveSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    lock (seatsLock)
                    {
                        if (availableSeats <= 0)
                        {
                            return false;
                        }

                        availableSeats--;
                        return true;
                    }
                });

            var tasks = Enumerable.Range(0, totalSeats)
                .Select(_ => Task.Run(async () =>
                {
                    await startSignal.Task;
                    using var scope = _fixture.ServiceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    return await bookingService.CreateBookingAsync(eventId);
                }));

            startSignal.SetResult();

            var bookings = await Task.WhenAll(tasks);

            bookings.Should().HaveCount(totalSeats);
            bookings.Select(booking => booking.Id).Should().OnlyHaveUniqueItems();
            availableSeats.Should().Be(0);
        }

        private async Task<(bool Success, BookingInfo? BookingInfo, Exception? Exception)> CreateBookingWithResultAsyncInScope(Guid eventId)
        {
            try
            {
                using var scope = _fixture.ServiceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                var bookingInfo = await bookingService.CreateBookingAsync(eventId);
                return (true, bookingInfo, null);
            }
            catch (Exception exception)
            {
                return (false, null, exception);
            }
        }
    }
}
