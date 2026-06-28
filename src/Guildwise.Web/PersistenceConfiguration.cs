using Guildwise.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Guildwise.Web;

public static class PersistenceConfiguration
{
    private const string PersistenceProviderKey = "Guildwise:PersistenceProvider";
    private const string GuildwiseDatabaseConnectionStringName = "GuildwiseDatabase";
    private const string InMemoryProvider = "InMemory";
    private const string PostgresProvider = "Postgres";
    private const string SupportedProviderMessage = "Supported values: InMemory, Postgres.";

    public static IServiceCollection AddConfiguredGuildwiseInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var configuredProvider = configuration[PersistenceProviderKey];
        if (string.IsNullOrWhiteSpace(configuredProvider))
        {
            if (!isDevelopment)
            {
                throw new InvalidOperationException("Guildwise:PersistenceProvider is required outside Development.");
            }

            services.AddInMemoryInfrastructure();
            return services;
        }

        var persistenceProvider = configuredProvider.Trim();
        if (string.Equals(persistenceProvider, InMemoryProvider, StringComparison.OrdinalIgnoreCase))
        {
            if (!isDevelopment)
            {
                throw new InvalidOperationException("InMemory persistence is not allowed outside Development.");
            }

            services.AddInMemoryInfrastructure();
            return services;
        }

        if (string.Equals(persistenceProvider, PostgresProvider, StringComparison.OrdinalIgnoreCase))
        {
            EnsurePostgresConnectionString(configuration);
            services.AddPostgresInfrastructure(configuration);
            return services;
        }

        throw new InvalidOperationException(
            $"Unsupported Guildwise:PersistenceProvider value '{configuredProvider}'. {SupportedProviderMessage}");
    }

    private static void EnsurePostgresConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(GuildwiseDatabaseConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:GuildwiseDatabase is required when using Postgres persistence.");
        }
    }
}
