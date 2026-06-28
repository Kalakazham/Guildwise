using Guildwise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Guildwise.IntegrationTests.Persistence;

public sealed class PostgreSqlTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("guildwise_tests")
        .WithUsername("guildwise")
        .WithPassword("guildwise")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await ResetDatabaseAsync();
    }

    public Task DisposeAsync()
        => _container.DisposeAsync().AsTask();

    public GuildwiseDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GuildwiseDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new GuildwiseDbContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateDbContext();

        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }
}
