using EventManagement.Bookings.Domain.Models;
using EventManagement.Bookings.Infrastructure.Data.Repositories;
using EventManagement.Bookings.Infrastructure.DataAccess;
using EventApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(PostgresDbFixture.CollectionName)]
public sealed class BookableEventRepositoryTests
{
    private readonly PostgresDbFixture _fixture;

    public BookableEventRepositoryTests(PostgresDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UpsertAsync_Persists_BookableEvent()
    {
        await _fixture.ResetAsync();
        await using var ctx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var repo = new BookableEventRepository(ctx);
        var bookableEvent = CreateBookableEvent(availableSeats: 10);

        await repo.UpsertAsync(bookableEvent);

        await using var verifyCtx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var fromDb = await verifyCtx.BookableEvents.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == bookableEvent.Id);

        Assert.NotNull(fromDb);
        Assert.Equal(bookableEvent.Title, fromDb.Title);
        Assert.Equal(10, fromDb.AvailableSeats);
    }

    [Fact]
    public async Task TryReserveSeatsAsync_WhenSeatsAvailable_ShouldDecreaseAvailableSeats()
    {
        await _fixture.ResetAsync();
        await using var ctx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var repo = new BookableEventRepository(ctx);
        var bookableEvent = CreateBookableEvent(availableSeats: 5);
        await repo.UpsertAsync(bookableEvent);

        await using var actCtx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var actRepo = new BookableEventRepository(actCtx);
        bool wasReserved = await actRepo.TryReserveSeatsAsync(bookableEvent.Id, 2);

        Assert.True(wasReserved);
        var updated = await actRepo.TryGetByIdAsync(bookableEvent.Id);
        Assert.NotNull(updated);
        Assert.Equal(3, updated.AvailableSeats);
    }

    [Fact]
    public async Task TryReserveSeatsAsync_WhenNotEnoughSeats_ShouldReturnFalse()
    {
        await _fixture.ResetAsync();
        await using var ctx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var repo = new BookableEventRepository(ctx);
        var bookableEvent = CreateBookableEvent(availableSeats: 1);
        await repo.UpsertAsync(bookableEvent);

        await using var actCtx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var actRepo = new BookableEventRepository(actCtx);
        bool wasReserved = await actRepo.TryReserveSeatsAsync(bookableEvent.Id, 2);

        Assert.False(wasReserved);
        var unchanged = await actRepo.TryGetByIdAsync(bookableEvent.Id);
        Assert.NotNull(unchanged);
        Assert.Equal(1, unchanged.AvailableSeats);
    }

    [Fact]
    public async Task ReleaseSeatsAsync_ShouldIncreaseAvailableSeats()
    {
        await _fixture.ResetAsync();
        await using var ctx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var repo = new BookableEventRepository(ctx);
        var bookableEvent = CreateBookableEvent(availableSeats: 3);
        await repo.UpsertAsync(bookableEvent);
        await repo.TryReserveSeatsAsync(bookableEvent.Id, 2);

        await using var actCtx = new BookingsDbContext(_fixture.CreateBookingsOptions());
        var actRepo = new BookableEventRepository(actCtx);
        await actRepo.ReleaseSeatsAsync(bookableEvent.Id, 1);

        var updated = await actRepo.TryGetByIdAsync(bookableEvent.Id);
        Assert.NotNull(updated);
        Assert.Equal(2, updated.AvailableSeats);
    }

    private static BookableEvent CreateBookableEvent(int availableSeats)
    {
        return BookableEvent.Create(
            Guid.NewGuid(),
            "Integration test event",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            availableSeats,
            availableSeats);
    }
}
