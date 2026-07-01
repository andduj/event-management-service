using EventManagement.Auth.Infrastructure.DataAccess;
using EventManagement.Bookings.Infrastructure.DataAccess;
using EventManagement.Events.Infrastructure.DataAccess;
using EventApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(PostgresDbFixture.CollectionName)]
public sealed class DbSchemaTests
{
    private readonly PostgresDbFixture _fixture;

    public DbSchemaTests(PostgresDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Migrations_Create_Events_Table_In_Events_Database()
    {
        await _fixture.ResetAsync();

        await using var db = new EventsDbContext(_fixture.CreateEventsOptions());
        Assert.True(await TableExistsAsync(db, "events"));
    }

    [Fact]
    public async Task Migrations_Create_Bookings_Table_In_Bookings_Database()
    {
        await _fixture.ResetAsync();

        await using var db = new BookingsDbContext(_fixture.CreateBookingsOptions());
        Assert.True(await TableExistsAsync(db, "bookings"));
    }

    [Fact]
    public async Task Migrations_Create_Users_Table_In_Auth_Database()
    {
        await _fixture.ResetAsync();

        await using var db = new AuthDbContext(_fixture.CreateAuthOptions());
        Assert.True(await TableExistsAsync(db, "users"));
    }

    private static async Task<bool> TableExistsAsync(DbContext db, string tableName, string schema = "public")
    {
        await db.Database.OpenConnectionAsync();
        try
        {
            await using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText =
                """
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = @schema AND table_name = @tableName
                LIMIT 1
                """;

            AddParameter(cmd, "@schema", schema);
            AddParameter(cmd, "@tableName", tableName);

            var result = await cmd.ExecuteScalarAsync();
            return result is not null and not DBNull;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }

        static void AddParameter(System.Data.Common.DbCommand command, string name, string value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
}
