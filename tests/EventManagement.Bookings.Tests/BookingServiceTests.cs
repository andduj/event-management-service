using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Exceptions;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Bookings.Infrastructure.DataAccess;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Bookings.Tests
{
    public class BookingServiceTests : IClassFixture<BookingServiceFixture>
    {
        private readonly IBookingService _bookingService;
        private readonly IBookingRepository _bookingRepository;
        private readonly BookingServiceFixture _fixture;
        private readonly Guid _testUserId;

        public BookingServiceTests(BookingServiceFixture fixture)
        {
            _fixture = fixture;
            _testUserId = fixture.TestUserId;
            _bookingService = fixture.BookingService;
            _bookingRepository = fixture.BookingRepository;

            using var cleanupScope = fixture.ServiceProvider.CreateScope();
            var context = cleanupScope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            context.Bookings.RemoveRange(context.Bookings);
            context.SaveChanges();
        }

        [Fact]
        public async Task CreateBookingAsync_ExistedEvent_Success()
        {
            var bookingInfo = await _bookingService.CreateBookingAsync(Guid.NewGuid(), _testUserId);

            bookingInfo.Status.Should().Be(BookingStatus.Pending);
        }

        [Fact]
        public async Task CreateBookingAsync_MultipleEventBooking_ShouldReturnDifferentBookingId()
        {
            var bookingInfoIds = new List<Guid>();
            var eventId = Guid.NewGuid();

            for (int i = 0; i < 10; i++)
            {
                var bookingInfo = await _bookingService.CreateBookingAsync(eventId, _testUserId);
                bookingInfoIds.Add(bookingInfo.Id);
            }

            bookingInfoIds.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task GetBookingByIdAsync_ExistedBooking_Success()
        {
            var bookingInfo = await _bookingService.CreateBookingAsync(Guid.NewGuid(), _testUserId);

            bookingInfo.Status.Should().Be(BookingStatus.Pending);

            var booking = await _bookingService.GetBookingByIdAsync(bookingInfo.Id);

            booking.Should().NotBeNull();
            booking.Id.Should().Be(bookingInfo.Id);
        }

        [Fact]
        public async Task GetBookingByIdAsync_UpdatedBookingStatus_ShouldReturnActualStatus()
        {
            var bookingInfo = await _bookingService.CreateBookingAsync(Guid.NewGuid(), _testUserId);
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingInfo.Id);
            booking.Confirm();
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
        public async Task CreateBookingAsync_ConcurrentRequests_ShouldReturnUniqueBookingIds()
        {
            const int concurrentRequests = 10;
            var eventId = Guid.NewGuid();
            var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var tasks = Enumerable.Range(0, concurrentRequests)
                .Select(_ => Task.Run(async () =>
                {
                    await startSignal.Task;
                    using var scope = _fixture.ServiceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    return await bookingService.CreateBookingAsync(eventId, _testUserId);
                }));

            startSignal.SetResult();

            var bookings = await Task.WhenAll(tasks);

            bookings.Should().HaveCount(concurrentRequests);
            bookings.Select(booking => booking.Id).Should().OnlyHaveUniqueItems();
        }
    }
}
