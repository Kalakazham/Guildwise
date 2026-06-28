using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Infrastructure.Persistence;
using Guildwise.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Guildwise.IntegrationTests;

public sealed class WebPersistenceConfigurationTests
{
    [Fact]
    public void Development_Missing_Provider_Uses_InMemory()
    {
        var services = ConfigureInfrastructure(isDevelopment: true);

        AssertInMemoryRepositoriesRegistered(services);
    }

    [Fact]
    public void Development_InMemory_Is_Allowed()
    {
        var services = ConfigureInfrastructure(isDevelopment: true, persistenceProvider: "InMemory");

        AssertInMemoryRepositoriesRegistered(services);
    }

    [Fact]
    public void Development_Postgres_With_Connection_String_Is_Allowed()
    {
        var services = ConfigureInfrastructure(
            isDevelopment: true,
            persistenceProvider: "Postgres",
            connectionString: ValidConnectionString);

        AssertPostgresRepositoriesRegistered(services);
    }

    [Fact]
    public void Development_Invalid_Provider_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => ConfigureInfrastructure(isDevelopment: true, persistenceProvider: "Sqlite"));

        Assert.Equal(
            "Unsupported Guildwise:PersistenceProvider value 'Sqlite'. Supported values: InMemory, Postgres.",
            exception.Message);
    }

    [Fact]
    public void NonDevelopment_Missing_Provider_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => ConfigureInfrastructure(isDevelopment: false));

        Assert.Equal("Guildwise:PersistenceProvider is required outside Development.", exception.Message);
    }

    [Fact]
    public void NonDevelopment_InMemory_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => ConfigureInfrastructure(isDevelopment: false, persistenceProvider: "InMemory"));

        Assert.Equal("InMemory persistence is not allowed outside Development.", exception.Message);
    }

    [Fact]
    public void NonDevelopment_Postgres_With_Missing_Connection_String_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => ConfigureInfrastructure(isDevelopment: false, persistenceProvider: "Postgres"));

        Assert.Equal(
            "ConnectionStrings:GuildwiseDatabase is required when using Postgres persistence.",
            exception.Message);
    }

    [Fact]
    public void NonDevelopment_Postgres_With_Blank_Connection_String_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => ConfigureInfrastructure(
                isDevelopment: false,
                persistenceProvider: "Postgres",
                connectionString: " "));

        Assert.Equal(
            "ConnectionStrings:GuildwiseDatabase is required when using Postgres persistence.",
            exception.Message);
    }

    [Fact]
    public void NonDevelopment_Postgres_With_Connection_String_Is_Allowed()
    {
        var services = ConfigureInfrastructure(
            isDevelopment: false,
            persistenceProvider: "Postgres",
            connectionString: ValidConnectionString);

        AssertPostgresRepositoriesRegistered(services);
    }

    [Fact]
    public void NonDevelopment_Invalid_Provider_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => ConfigureInfrastructure(isDevelopment: false, persistenceProvider: "Sqlite"));

        Assert.Equal(
            "Unsupported Guildwise:PersistenceProvider value 'Sqlite'. Supported values: InMemory, Postgres.",
            exception.Message);
    }

    private const string ValidConnectionString =
        "Host=localhost;Port=55432;Database=guildwise;Username=guildwise;Password=guildwise";

    private static ServiceCollection ConfigureInfrastructure(
        bool isDevelopment,
        string? persistenceProvider = null,
        string? connectionString = null)
    {
        var configurationValues = new Dictionary<string, string?>();
        if (persistenceProvider is not null)
        {
            configurationValues["Guildwise:PersistenceProvider"] = persistenceProvider;
        }

        if (connectionString is not null)
        {
            configurationValues["ConnectionStrings:GuildwiseDatabase"] = connectionString;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();
        var services = new ServiceCollection();

        services.AddConfiguredGuildwiseInfrastructure(configuration, isDevelopment);

        return services;
    }

    private static void AssertInMemoryRepositoriesRegistered(IServiceCollection services)
    {
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IGuildRepository)
                && descriptor.ImplementationType == typeof(InMemoryGuildRepository)
                && descriptor.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IPlayerRepository)
                && descriptor.ImplementationType == typeof(InMemoryPlayerRepository)
                && descriptor.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(ITransactionRunner)
                && descriptor.ImplementationType == typeof(InMemoryTransactionRunner)
                && descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    private static void AssertPostgresRepositoriesRegistered(IServiceCollection services)
    {
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IGuildRepository)
                && descriptor.ImplementationType == typeof(EfGuildRepository)
                && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IPlayerRepository)
                && descriptor.ImplementationType == typeof(EfPlayerRepository)
                && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(DbContextOptions<GuildwiseDbContext>)
                && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(ITransactionRunner)
                && descriptor.ImplementationType == typeof(EfTransactionRunner)
                && descriptor.Lifetime == ServiceLifetime.Scoped);
    }
}
