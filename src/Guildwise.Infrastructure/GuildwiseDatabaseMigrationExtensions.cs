using Guildwise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Guildwise.Infrastructure;

public static class GuildwiseDatabaseMigrationExtensions
{
    public static async Task ApplyGuildwiseDatabaseMigrationsAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GuildwiseDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
