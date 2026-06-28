using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Guildwise.Infrastructure.Persistence;

public sealed class GuildwiseDbContextFactory : IDesignTimeDbContextFactory<GuildwiseDbContext>
{
    private const string LocalDevelopmentConnectionString =
        "Host=localhost;Port=55432;Database=guildwise;Username=guildwise;Password=guildwise";

    public GuildwiseDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("GUILDWISE_CONNECTION_STRING")
            ?? LocalDevelopmentConnectionString;

        var options = new DbContextOptionsBuilder<GuildwiseDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new GuildwiseDbContext(options);
    }
}
