using EventManagement.Auth.Infrastructure.DataAccess;
using EventManagement.Bookings.Infrastructure.DataAccess;
using EventManagement.Events.Infrastructure.DataAccess;
using EventApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests.Infrastructure;

public sealed class PostgresDbFixture : IAsyncLifetime
{
    public const string CollectionName = "PostgreDb";

    private const string EventsDatabaseName = "events";
    private const string BookingsDatabaseName = "bookings";
    private const string AuthDatabaseName = "auth";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    public string EventsConnectionString { get; private set; } = null!;

    public string BookingsConnectionString { get; private set; } = null!;

    public string AuthConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var template = new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString());
        EventsConnectionString = CloneWithDatabase(template, EventsDatabaseName);
        BookingsConnectionString = CloneWithDatabase(template, BookingsDatabaseName);
        AuthConnectionString = CloneWithDatabase(template, AuthDatabaseName);

        await using var eventsDb = new EventsDbContext(CreateEventsOptions());
        await eventsDb.Database.MigrateAsync();

        await using var bookingsDb = new BookingsDbContext(CreateBookingsOptions());
        await bookingsDb.Database.MigrateAsync();

        await using var authDb = new AuthDbContext(CreateAuthOptions());
        await authDb.Database.MigrateAsync();
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

        NpgsqlConnection.ClearAllPools();

        await using (var authDb = new AuthDbContext(CreateAuthOptions()))
        {
            await authDb.Database.EnsureDeletedAsync(cancellationToken);
            await authDb.Database.MigrateAsync(cancellationToken);
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

    public DbContextOptions<AuthDbContext> CreateAuthOptions() =>
        new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(AuthConnectionString)
            .Options;

    private static string CloneWithDatabase(NpgsqlConnectionStringBuilder template, string database)
    {
        template.Database = database;
        return template.ConnectionString;
    }
}
