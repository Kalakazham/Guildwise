using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Guildwise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        => services.AddInMemoryInfrastructure();

    public static IServiceCollection AddInMemoryInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IGuildRepository, InMemoryGuildRepository>();
        services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();

        return services;
    }

    public static IServiceCollection AddPostgresInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("GuildwiseDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'GuildwiseDatabase' is required.");
        }

        services.AddDbContext<GuildwiseDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
