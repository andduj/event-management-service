using EventManagement.Events.Application.Filters;
using EventManagement.Events.Infrastructure.Data.Repositories;
using EventManagement.Events.Infrastructure.DataAccess;
using EventManagement.Events.Domain.Exceptions;
using EventManagement.Events.Domain.Models;
using EventApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(PostgresDbFixture.CollectionName)]
public sealed class EventTests
{
    private readonly PostgresDbFixture _fixture;

    public EventTests(PostgresDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateEventAsync_Persists_Event()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);

        var evt = Event.Create("Conference", Utc(2030, 1, 10), Utc(2030, 1, 11), 50, "Desc");
        var saved = await repo.CreateEventAsync(evt);

        Assert.Equal(evt.Id, saved.Id);
        Assert.Equal("Conference", saved.Title);

        await using var verifyCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        var fromDb = await verifyCtx.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == saved.Id);
        Assert.NotNull(fromDb);
        Assert.Equal(50, fromDb!.AvailableSeats);
    }

    [Fact]
    public async Task GetEventByIdAsync_Returns_Event()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        var evt = Event.Create("X", Utc(2030, 2, 1), Utc(2030, 2, 2), 10);
        await repo.CreateEventAsync(evt);

        await using var actCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        var found = await new EventRepository(actCtx).GetEventByIdAsync(evt.Id);

        Assert.Equal(evt.Id, found.Id);
        Assert.Equal("X", found.Title);
    }

    [Fact]
    public async Task GetEventByIdAsync_Throws_When_Missing()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);

        var ex = await Assert.ThrowsAsync<EventNotFoundException>(() => repo.GetEventByIdAsync(Guid.NewGuid()));

        Assert.NotNull(ex.Message);
    }

    [Fact]
    public async Task UpdateEventAsync_Updates_Row()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        var evt = Event.Create("Old", Utc(2030, 3, 1), Utc(2030, 3, 2), 20);
        await repo.CreateEventAsync(evt);

        evt.Title = "NewTitle";
        evt.Description = "NewDesc";
        await repo.UpdateEventAsync(evt);

        await using var verifyCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        var fromDb = await verifyCtx.Events.AsNoTracking().FirstAsync(e => e.Id == evt.Id);
        Assert.Equal("NewTitle", fromDb.Title);
        Assert.Equal("NewDesc", fromDb.Description);
    }

    [Fact]
    public async Task DeleteEventAsync_Removes_Event()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        var evt = Event.Create("Del", Utc(2030, 4, 1), Utc(2030, 4, 2), 5);
        await repo.CreateEventAsync(evt);

        await repo.DeleteEventAsync(evt.Id);

        await using var verifyCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        Assert.False(await verifyCtx.Events.AsNoTracking().AnyAsync(e => e.Id == evt.Id));
    }

    [Fact]
    public async Task Exists_Returns_False_When_Missing()
    {
        await _fixture.ResetAsync();

        await using var actCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        Assert.False(await new EventRepository(actCtx).Exists(Guid.NewGuid()));
    }

    [Fact]
    public async Task Exists_Returns_True_When_Present()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        var evt = Event.Create("E", Utc(2030, 5, 1), Utc(2030, 5, 2), 3);
        await repo.CreateEventAsync(evt);

        await using var verifyCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        Assert.True(await new EventRepository(verifyCtx).Exists(evt.Id));
    }

    [Fact]
    public async Task TryReserveSeats_Decrements_And_Returns_True()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        var evt = Event.Create("R", Utc(2030, 6, 1), Utc(2030, 6, 2), 5);
        await repo.CreateEventAsync(evt);

        var ok = await repo.TryReserveSeats(evt.Id, 2);
        Assert.True(ok);

        await using var verifyCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        var fromDb = await verifyCtx.Events.AsNoTracking().FirstAsync(e => e.Id == evt.Id);
        Assert.Equal(3, fromDb.AvailableSeats);
    }

    [Fact]
    public async Task TryReserveSeats_Returns_False_When_Not_Enough_Seats()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        var evt = Event.Create("R2", Utc(2030, 7, 1), Utc(2030, 7, 2), 2);
        await repo.CreateEventAsync(evt);

        var ok = await repo.TryReserveSeats(evt.Id, 5);
        Assert.False(ok);

        await using var verifyCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        var fromDb = await verifyCtx.Events.AsNoTracking().FirstAsync(e => e.Id == evt.Id);
        Assert.Equal(2, fromDb.AvailableSeats);
    }

    [Fact]
    public async Task ReleaseSeats_Increments_Available()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        var evt = Event.Create("Rel", Utc(2030, 8, 1), Utc(2030, 8, 2), 4);
        await repo.CreateEventAsync(evt);
        await repo.TryReserveSeats(evt.Id, 2);

        await repo.ReleaseSeats(evt.Id, 1);

        await using var verifyCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        var fromDb = await verifyCtx.Events.AsNoTracking().FirstAsync(e => e.Id == evt.Id);
        Assert.Equal(3, fromDb.AvailableSeats);
    }

    [Fact]
    public async Task FilterAsync_No_Filter_Returns_Paginated_All()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        for (int i = 0; i < 3; i++)
        {
            await repo.CreateEventAsync(Event.Create($"T{i}", Utc(2030, 9, 1 + i), Utc(2030, 9, 2 + i), 1));
        }

        await using var actCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        var result = await new EventRepository(actCtx).FilterAsync(new EventFilter(), 1, 2);

        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task FilterAsync_Title_Uses_Case_Insensitive_Substring()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        await repo.CreateEventAsync(Event.Create("Alpha workshop", Utc(2030, 10, 1), Utc(2030, 10, 2), 1));
        await repo.CreateEventAsync(Event.Create("Beta", Utc(2030, 10, 3), Utc(2030, 10, 4), 1));

        await using var actCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        var result = await new EventRepository(actCtx).FilterAsync(new EventFilter { Title = "work" }, 1, 10);

        Assert.Single(result.Items);
        Assert.Contains("work", result.Items[0].Title, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FilterAsync_StartAt_Inclusive()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        var early = Event.Create("Early", Utc(2030, 11, 1), Utc(2030, 11, 2), 1);
        var late = Event.Create("Late", Utc(2030, 12, 5), Utc(2030, 12, 6), 1);
        await repo.CreateEventAsync(early);
        await repo.CreateEventAsync(late);

        var boundary = Utc(2030, 12, 5);
        await using var actCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        var result = await new EventRepository(actCtx).FilterAsync(new EventFilter { StartAt = boundary }, 1, 10);

        Assert.Single(result.Items);
        Assert.Equal(late.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task FilterAsync_EndAt_Upper_Bound()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        var a = Event.Create("A", Utc(2031, 1, 1), Utc(2031, 1, 10), 1);
        var b = Event.Create("B", Utc(2031, 2, 1), Utc(2031, 2, 20), 1);
        await repo.CreateEventAsync(a);
        await repo.CreateEventAsync(b);

        await using var actCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        var result = await new EventRepository(actCtx).FilterAsync(new EventFilter { EndAt = Utc(2031, 1, 15) }, 1, 10);

        Assert.Single(result.Items);
        Assert.Equal(a.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task FilterAsync_Combined_Filters_And_Second_Page()
    {
        await _fixture.ResetAsync();
        await using var ctx = new EventsDbContext(_fixture.CreateEventsOptions());
        var repo = new EventRepository(ctx);
        await repo.CreateEventAsync(Event.Create("Meetup one", Utc(2032, 6, 1), Utc(2032, 6, 5), 1));
        await repo.CreateEventAsync(Event.Create("Meetup two", Utc(2032, 6, 2), Utc(2032, 6, 8), 1));
        await repo.CreateEventAsync(Event.Create("Noise", Utc(2032, 6, 3), Utc(2032, 6, 9), 1));

        var filter = new EventFilter
        {
            Title = "Meetup",
            StartAt = Utc(2032, 6, 1),
            EndAt = Utc(2032, 6, 30),
        };

        await using var actCtx = new EventsDbContext(_fixture.CreateEventsOptions());
        var actRepo = new EventRepository(actCtx);
        var page1 = await actRepo.FilterAsync(filter, 1, 1);
        Assert.Single(page1.Items);
        Assert.Equal(2, page1.TotalItems);

        var page2 = await actRepo.FilterAsync(filter, 2, 1);
        Assert.Single(page2.Items);
        Assert.NotEqual(page1.Items[0].Id, page2.Items[0].Id);
    }

    private static DateTime Utc(int y, int m, int d) => new DateTime(y, m, d, 12, 0, 0, DateTimeKind.Utc);
}
