using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Exceptions;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Bookings.Infrastructure.DataAccess;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Bookings.Tests
{
    public class BookingRulesTests : IClassFixture<BookingServiceFixture>
    {
        private readonly BookingServiceFixture _fixture;
        private readonly IBookingService _bookingService;
        private readonly Guid _testUserId;

        public BookingRulesTests(BookingServiceFixture fixture)
        {
            _fixture = fixture;
            _bookingService = fixture.BookingService;
            _testUserId = fixture.TestUserId;

            using var cleanupScope = fixture.ServiceProvider.CreateScope();
            var context = cleanupScope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            context.Bookings.RemoveRange(context.Bookings);
            context.SaveChanges();
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
            var otherUserId = Guid.NewGuid();

            for (int i = 0; i < BookingLimits.MaxActiveBookings; i++)
            {
                await _bookingService.CreateBookingAsync(eventId, _testUserId);
            }

            var action = () => _bookingService.CreateBookingAsync(eventId, otherUserId);

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
            var ownerId = Guid.NewGuid();
            var bookingInfo = await _bookingService.CreateBookingAsync(Guid.NewGuid(), ownerId);

            var action = () => _bookingService.CancelBookingAsync(bookingInfo.Id, _testUserId, UserRole.User);

            await action.Should().ThrowAsync<AccessDeniedException>();
        }

        [Fact]
        public async Task CancelBookingAsync_OtherUsersBookingAsAdmin_ShouldSucceed()
        {
            var ownerId = Guid.NewGuid();
            var bookingInfo = await _bookingService.CreateBookingAsync(Guid.NewGuid(), ownerId);

            await _bookingService.CancelBookingAsync(bookingInfo.Id, _testUserId, UserRole.Admin);

            var booking = await _bookingService.GetBookingByIdAsync(bookingInfo.Id);
            booking.Status.Should().Be(BookingStatus.Cancelled);
        }
    }
}
