using EventManagement.Bookings.Infrastructure.DataAccess;
using EventManagement.Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests.Infrastructure;

public sealed class PostgresDbFixture : IAsyncLifetime
{
    public const string CollectionName = "PostgreDb";

    private const string EventsDatabaseName = "events";
    private const string BookingsDatabaseName = "bookings";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    public string EventsConnectionString { get; private set; } = null!;

    public string BookingsConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var template = new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString());
        EventsConnectionString = CloneWithDatabase(template, EventsDatabaseName);
        BookingsConnectionString = CloneWithDatabase(template, BookingsDatabaseName);

        await using var eventsDb = new EventsDbContext(CreateEventsOptions());
        await eventsDb.Database.MigrateAsync();

        await using var bookingsDb = new BookingsDbContext(CreateBookingsOptions());
        await bookingsDb.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    } 

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        NpgsqlConnection.ClearAllPools();

        await using (var eventsDb = new EventsDbContext(CreateEventsOptions()))
        {
            await eventsDb.Database.EnsureDeletedAsync(cancellationToken);
            await eventsDb.Database.MigrateAsync(cancellationToken);
        }

        NpgsqlConnection.ClearAllPools();

        await using (var bookingsDb = new BookingsDbContext(CreateBookingsOptions()))
        {
            await bookingsDb.Database.EnsureDeletedAsync(cancellationToken);
            await bookingsDb.Database.MigrateAsync(cancellationToken);
        }
    }

    public DbContextOptions<EventsDbContext> CreateEventsOptions() =>
        new DbContextOptionsBuilder<EventsDbContext>()
            .UseNpgsql(EventsConnectionString)
            .Options;

    public DbContextOptions<BookingsDbContext> CreateBookingsOptions() =>
        new DbContextOptionsBuilder<BookingsDbContext>()
            .UseNpgsql(BookingsConnectionString)
            .Options;

    private static string CloneWithDatabase(NpgsqlConnectionStringBuilder template, string database)
    {
        template.Database = database;
        return template.ConnectionString;
    }
}
