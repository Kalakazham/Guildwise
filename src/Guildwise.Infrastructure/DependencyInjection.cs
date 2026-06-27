using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Guildwise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IGuildRepository, InMemoryGuildRepository>();
        services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();

        return services;
    }
}
