using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;
using Guildwise.Infrastructure;
using Guildwise.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Guildwise.IntegrationTests;

public sealed class InMemoryStorageTests
{
    [Fact]
    public async Task GuildRepository_Adds_And_Retrieves_Guild()
    {
        var repository = new InMemoryGuildRepository();
        var guild = Guild.Create("Guildwise", "EU", "Draenor");

        await repository.AddAsync(guild);

        Assert.Same(guild, await repository.GetByIdAsync(guild.Id));
    }

    [Fact]
    public async Task GuildRepository_Lists_Guilds()
    {
        var repository = new InMemoryGuildRepository();
        var firstGuild = Guild.Create("Guildwise", "EU", "Draenor");
        var secondGuild = Guild.Create("Night Shift", "US", "Area 52");

        await repository.AddAsync(firstGuild);
        await repository.AddAsync(secondGuild);

        var guilds = await repository.ListAsync();

        Assert.Collection(
            guilds.OrderBy(guild => guild.Name),
            guild => Assert.Same(firstGuild, guild),
            guild => Assert.Same(secondGuild, guild));
    }

    [Fact]
    public async Task GuildRepository_Removes_Guild()
    {
        var repository = new InMemoryGuildRepository();
        var guild = Guild.Create("Guildwise", "EU", "Draenor");

        await repository.AddAsync(guild);
        await repository.RemoveAsync(guild.Id);

        Assert.Null(await repository.GetByIdAsync(guild.Id));
        Assert.Empty(await repository.ListAsync());
    }

    [Fact]
    public async Task PlayerRepository_Adds_And_Retrieves_Player()
    {
        var repository = new InMemoryPlayerRepository();
        var player = Player.Create("Myrmi");

        await repository.AddAsync(player);

        Assert.Same(player, await repository.GetByIdAsync(player.Id));
    }

    [Fact]
    public async Task PlayerRepository_Lists_Players()
    {
        var repository = new InMemoryPlayerRepository();
        var firstPlayer = Player.Create("Myrmi");
        var secondPlayer = Player.Create("Alysa");

        await repository.AddAsync(firstPlayer);
        await repository.AddAsync(secondPlayer);

        var players = await repository.ListAsync();

        Assert.Collection(
            players.OrderBy(player => player.DisplayName),
            player => Assert.Same(secondPlayer, player),
            player => Assert.Same(firstPlayer, player));
    }

    [Fact]
    public async Task PlayerRepository_Removes_Player()
    {
        var repository = new InMemoryPlayerRepository();
        var player = Player.Create("Myrmi");

        await repository.AddAsync(player);
        await repository.RemoveAsync(player.Id);

        Assert.Null(await repository.GetByIdAsync(player.Id));
        Assert.Empty(await repository.ListAsync());
    }

    [Fact]
    public void AddInMemoryInfrastructure_Registers_InMemory_Aggregate_Root_Repositories()
    {
        var services = new ServiceCollection();

        services.AddInMemoryInfrastructure();

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

    [Fact]
    public void AddInfrastructure_Keeps_InMemory_Registration_As_Current_Default()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure();

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
}
