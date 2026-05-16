namespace EventApi.IntegrationTests.Infrastructure;

[CollectionDefinition(PostgresDbFixture.CollectionName)]
public sealed class PostgresDbCollection : ICollectionFixture<PostgresDbFixture>
{
}
