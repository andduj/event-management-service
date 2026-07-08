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
            context.BookableEvents.RemoveRange(context.BookableEvents);
            context.SaveChanges();
        }

        [Fact]
        public async Task CreateBookingAsync_WhenEventAlreadyStarted_ShouldThrowEventAlreadyStartedException()
        {
            var bookableEvent = await _fixture.SeedBookableEventAsync(startAt: DateTime.UtcNow.AddHours(-1));

            var action = () => _bookingService.CreateBookingAsync(bookableEvent.Id, _testUserId);

            await action.Should().ThrowAsync<EventAlreadyStartedException>();
        }

        [Fact]
        public async Task CreateBookingAsync_WhenActiveBookingsLimitReached_ShouldThrowActiveBookingsLimitExceededException()
        {
            var bookableEvent = await _fixture.SeedBookableEventAsync(availableSeats: BookingLimits.MaxActiveBookings + 1);

            for (int i = 0; i < BookingLimits.MaxActiveBookings; i++)
            {
                await _bookingService.CreateBookingAsync(bookableEvent.Id, _testUserId);
            }

            var action = () => _bookingService.CreateBookingAsync(bookableEvent.Id, _testUserId);

            var exception = await action.Should().ThrowAsync<ActiveBookingsLimitExceededException>();
            exception.Which.Limit.Should().Be(BookingLimits.MaxActiveBookings);
        }

        [Fact]
        public async Task CreateBookingAsync_ActiveBookingsLimit_ShouldNotAffectOtherUsers()
        {
            var bookableEvent = await _fixture.SeedBookableEventAsync(availableSeats: BookingLimits.MaxActiveBookings + 1);
            var otherUserId = Guid.NewGuid();

            for (int i = 0; i < BookingLimits.MaxActiveBookings; i++)
            {
                await _bookingService.CreateBookingAsync(bookableEvent.Id, _testUserId);
            }

            var action = () => _bookingService.CreateBookingAsync(bookableEvent.Id, otherUserId);

            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task CancelBookingAsync_OwnBooking_ShouldSetCancelledStatus()
        {
            var bookableEvent = await _fixture.SeedBookableEventAsync();
            var bookingInfo = await _bookingService.CreateBookingAsync(bookableEvent.Id, _testUserId);

            await _bookingService.CancelBookingAsync(bookingInfo.Id, _testUserId, UserRole.User);

            var booking = await _bookingService.GetBookingByIdAsync(bookingInfo.Id);
            booking.Status.Should().Be(BookingStatus.Cancelled);
        }

        [Fact]
        public async Task CancelBookingAsync_OtherUsersBookingAsUser_ShouldThrowAccessDeniedException()
        {
            var bookableEvent = await _fixture.SeedBookableEventAsync();
            var ownerId = Guid.NewGuid();
            var bookingInfo = await _bookingService.CreateBookingAsync(bookableEvent.Id, ownerId);

            var action = () => _bookingService.CancelBookingAsync(bookingInfo.Id, _testUserId, UserRole.User);

            await action.Should().ThrowAsync<AccessDeniedException>();
        }

        [Fact]
        public async Task CancelBookingAsync_OtherUsersBookingAsAdmin_ShouldSucceed()
        {
            var bookableEvent = await _fixture.SeedBookableEventAsync(availableSeats: 2);
            var ownerId = Guid.NewGuid();
            var bookingInfo = await _bookingService.CreateBookingAsync(bookableEvent.Id, ownerId);

            await _bookingService.CancelBookingAsync(bookingInfo.Id, _testUserId, UserRole.Admin);

            var booking = await _bookingService.GetBookingByIdAsync(bookingInfo.Id);
            booking.Status.Should().Be(BookingStatus.Cancelled);
        }

        [Fact]
        public async Task CancelBookingAsync_ActiveBooking_ShouldReleaseSeat()
        {
            var bookableEvent = await _fixture.SeedBookableEventAsync(availableSeats: 1);
            var bookingInfo = await _bookingService.CreateBookingAsync(bookableEvent.Id, _testUserId);

            await _bookingService.CancelBookingAsync(bookingInfo.Id, _testUserId, UserRole.User);

            var updatedEvent = await _fixture.BookableEventRepository.TryGetByIdAsync(bookableEvent.Id);
            updatedEvent!.AvailableSeats.Should().Be(1);
        }
    }
}
