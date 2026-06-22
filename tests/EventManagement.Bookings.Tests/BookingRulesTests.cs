using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Exceptions;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Bookings.Infrastructure.DataAccess;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingRulesTests : IClassFixture<BookingServiceFixture>
    {
        private readonly BookingServiceFixture _fixture;
        private readonly IBookingService _bookingService;
        private readonly Mock<IEventsGateway> _eventsGateway;
        private readonly Guid _testUserId;

        public BookingRulesTests(BookingServiceFixture fixture)
        {
            _fixture = fixture;
            _bookingService = fixture.BookingService;
            _eventsGateway = fixture.EventsGateway;
            _testUserId = fixture.TestUserId;

            _eventsGateway.Reset();
            _eventsGateway
                .Setup(gateway => gateway.ReserveSeatsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _eventsGateway
                .Setup(gateway => gateway.EnsureEventExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _eventsGateway
                .Setup(gateway => gateway.GetEventStartAtUtcAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DateTime.UtcNow.AddDays(1));

            using var cleanupScope = fixture.ServiceProvider.CreateScope();
            var context = cleanupScope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            context.Bookings.RemoveRange(context.Bookings);
            context.SaveChanges();
        }

        [Fact]
        public async Task CreateBookingAsync_WhenEventAlreadyStarted_ShouldThrowEventAlreadyStartedException()
        {
            var eventId = Guid.NewGuid();
            _eventsGateway
                .Setup(gateway => gateway.GetEventStartAtUtcAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(DateTime.UtcNow.AddHours(-1));

            var action = () => _bookingService.CreateBookingAsync(eventId, _testUserId);

            await action.Should().ThrowAsync<EventAlreadyStartedException>();
            _eventsGateway.Verify(
                gateway => gateway.ReserveSeatsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateBookingAsync_WhenActiveBookingsLimitReached_ShouldThrowActiveBookingsLimitExceededException()
        {
            var eventId = Guid.NewGuid();

            for (int i = 0; i < BookingLimits.MaxActiveBookings; i++)
            {
                await _bookingService.CreateBookingAsync(eventId, _testUserId);
            }

            var action = () => _bookingService.CreateBookingAsync(eventId, _testUserId);

            var exception = await action.Should().ThrowAsync<ActiveBookingsLimitExceededException>();
            exception.Which.Limit.Should().Be(BookingLimits.MaxActiveBookings);
        }

        [Fact]
        public async Task CreateBookingAsync_ActiveBookingsLimit_ShouldNotAffectOtherUsers()
        {
            var eventId = Guid.NewGuid();
            var otherUser = await _fixture.CreateUserAsync($"other-user-{Guid.NewGuid():N}");

            for (int i = 0; i < BookingLimits.MaxActiveBookings; i++)
            {
                await _bookingService.CreateBookingAsync(eventId, _testUserId);
            }

            var action = () => _bookingService.CreateBookingAsync(eventId, otherUser.Id);

            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task CancelBookingAsync_OwnBooking_ShouldSetCancelledStatus()
        {
            var bookingInfo = await _bookingService.CreateBookingAsync(Guid.NewGuid(), _testUserId);

            await _bookingService.CancelBookingAsync(bookingInfo.Id, _testUserId, UserRole.User);

            var booking = await _bookingService.GetBookingByIdAsync(bookingInfo.Id);
            booking.Status.Should().Be(BookingStatus.Cancelled);
        }

        [Fact]
        public async Task CancelBookingAsync_OtherUsersBookingAsUser_ShouldThrowAccessDeniedException()
        {
            var owner = await _fixture.CreateUserAsync($"owner-{Guid.NewGuid():N}");
            var bookingInfo = await _bookingService.CreateBookingAsync(Guid.NewGuid(), owner.Id);

            var action = () => _bookingService.CancelBookingAsync(bookingInfo.Id, _testUserId, UserRole.User);

            await action.Should().ThrowAsync<AccessDeniedException>();
        }

        [Fact]
        public async Task CancelBookingAsync_OtherUsersBookingAsAdmin_ShouldSucceed()
        {
            var owner = await _fixture.CreateUserAsync($"owner-{Guid.NewGuid():N}");
            var bookingInfo = await _bookingService.CreateBookingAsync(Guid.NewGuid(), owner.Id);

            await _bookingService.CancelBookingAsync(bookingInfo.Id, _testUserId, UserRole.Admin);

            var booking = await _bookingService.GetBookingByIdAsync(bookingInfo.Id);
            booking.Status.Should().Be(BookingStatus.Cancelled);
        }

        [Fact]
        public async Task CancelBookingAsync_ActiveBooking_ShouldReleaseSeat()
        {
            var eventId = Guid.NewGuid();
            var bookingInfo = await _bookingService.CreateBookingAsync(eventId, _testUserId);

            await _bookingService.CancelBookingAsync(bookingInfo.Id, _testUserId, UserRole.User);

            _eventsGateway.Verify(
                gateway => gateway.ReleaseSeatsAsync(eventId, 1, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
