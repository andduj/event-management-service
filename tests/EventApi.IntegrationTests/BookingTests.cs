using EventManagement.Bookings.Data.Repositories;
using EventManagement.Bookings.DataAccess;
using EventManagement.Bookings.Exceptions;
using EventManagement.Bookings.Models;
using EventApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(PostgresDbFixture.CollectionName)]
public sealed class BookingTests
{
    private readonly PostgresDbFixture _fixture;

    public BookingTests(PostgresDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateBookingAsync_Persists_Booking()
    {
        await _fixture.ResetAsync();
        await using var ctx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var repo = new BookingRepository(ctx);
        var eventId = Guid.NewGuid();

        var booking = Booking.Create(eventId);
        var saved = await repo.CreateBookingAsync(booking);

        Assert.Equal(booking.Id, saved.Id);
        Assert.Equal(eventId, saved.EventId);
        Assert.Equal(BookingStatus.Pending, saved.Status);

        await using var verifyCtx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var fromDb = await verifyCtx.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == saved.Id);
        Assert.NotNull(fromDb);
    }

    [Fact]
    public async Task GetBookingByIdAsync_Returns_Booking()
    {
        await _fixture.ResetAsync();
        await using var ctx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var repo = new BookingRepository(ctx);
        var booking = Booking.Create(Guid.NewGuid());
        await repo.CreateBookingAsync(booking);

        await using var actCtx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var found = await new BookingRepository(actCtx).GetBookingByIdAsync(booking.Id);

        Assert.Equal(booking.Id, found.Id);
        Assert.Equal(booking.EventId, found.EventId);
    }

    [Fact]
    public async Task GetBookingByIdAsync_Throws_When_Missing()
    {
        await _fixture.ResetAsync();
        await using var ctx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var repo = new BookingRepository(ctx);

        await Assert.ThrowsAsync<BookingNotFoundException>(() => repo.GetBookingByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetBookingsAsync_Filters_By_Status()
    {
        await _fixture.ResetAsync();
        await using var ctx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var repo = new BookingRepository(ctx);
        var bookingId = Guid.NewGuid();

        var pending = Booking.Create(bookingId);
        await repo.CreateBookingAsync(pending);

        var confirmed = Booking.Create(bookingId);
        await repo.CreateBookingAsync(confirmed);
        confirmed.Confirm();
        await repo.UpdateBookingAsync(confirmed);

        var rejected = Booking.Create(bookingId);
        await repo.CreateBookingAsync(rejected);
        rejected.Reject();
        await repo.UpdateBookingAsync(rejected);

        await using var actCtx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var actRepo = new BookingRepository(actCtx);
        var pendingOnly = await actRepo.GetBookingsAsync(BookingStatus.Pending);
        var confirmedOnly = await actRepo.GetBookingsAsync(BookingStatus.Confirmed);
        var rejectedOnly = await actRepo.GetBookingsAsync(BookingStatus.Rejected);

        Assert.Single(pendingOnly);
        Assert.Equal(BookingStatus.Pending, pendingOnly.First().Status);

        Assert.Single(confirmedOnly);
        Assert.Equal(BookingStatus.Confirmed, confirmedOnly.First().Status);

        Assert.Single(rejectedOnly);
        Assert.Equal(BookingStatus.Rejected, rejectedOnly.First().Status);
    }

    [Fact]
    public async Task UpdateBookingAsync_Updates_Row()
    {
        await _fixture.ResetAsync();
        await using var ctx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var repo = new BookingRepository(ctx);
        var booking = Booking.Create(Guid.NewGuid());
        await repo.CreateBookingAsync(booking);

        booking.Confirm();
        await repo.UpdateBookingAsync(booking);

        await using var verifyCtx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var fromDb = await verifyCtx.Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id);
        Assert.Equal(BookingStatus.Confirmed, fromDb.Status);
        Assert.NotNull(fromDb.ProcessedAt);
    }
}
