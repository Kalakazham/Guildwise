namespace Guildwise.IntegrationTests.Persistence;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PostgreSqlTestCollection : ICollectionFixture<PostgreSqlTestFixture>
{
    public const string Name = "PostgreSQL persistence";
}
