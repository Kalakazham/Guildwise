using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;
using Guildwise.Infrastructure;
using Guildwise.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Guildwise.IntegrationTests;

public sealed class InMemoryStorageTests
{
    [Fact]
    public void GuildRepository_Adds_And_Retrieves_Guild()
    {
        var repository = new InMemoryGuildRepository();
        var guild = Guild.Create("Guildwise", "EU", "Draenor");

        repository.Add(guild);

        Assert.Same(guild, repository.GetById(guild.Id));
    }

    [Fact]
    public void GuildRepository_Lists_Guilds()
    {
        var repository = new InMemoryGuildRepository();
        var firstGuild = Guild.Create("Guildwise", "EU", "Draenor");
        var secondGuild = Guild.Create("Night Shift", "US", "Area 52");

        repository.Add(firstGuild);
        repository.Add(secondGuild);

        var guilds = repository.List();

        Assert.Collection(
            guilds.OrderBy(guild => guild.Name),
            guild => Assert.Same(firstGuild, guild),
            guild => Assert.Same(secondGuild, guild));
    }

    [Fact]
    public void GuildRepository_Removes_Guild()
    {
        var repository = new InMemoryGuildRepository();
        var guild = Guild.Create("Guildwise", "EU", "Draenor");

        repository.Add(guild);
        repository.Remove(guild.Id);

        Assert.Null(repository.GetById(guild.Id));
        Assert.Empty(repository.List());
    }

    [Fact]
    public void PlayerRepository_Adds_And_Retrieves_Player()
    {
        var repository = new InMemoryPlayerRepository();
        var player = Player.Create("Myrmi");

        repository.Add(player);

        Assert.Same(player, repository.GetById(player.Id));
    }

    [Fact]
    public void PlayerRepository_Lists_Players()
    {
        var repository = new InMemoryPlayerRepository();
        var firstPlayer = Player.Create("Myrmi");
        var secondPlayer = Player.Create("Alysa");

        repository.Add(firstPlayer);
        repository.Add(secondPlayer);

        var players = repository.List();

        Assert.Collection(
            players.OrderBy(player => player.DisplayName),
            player => Assert.Same(secondPlayer, player),
            player => Assert.Same(firstPlayer, player));
    }

    [Fact]
    public void PlayerRepository_Removes_Player()
    {
        var repository = new InMemoryPlayerRepository();
        var player = Player.Create("Myrmi");

        repository.Add(player);
        repository.Remove(player.Id);

        Assert.Null(repository.GetById(player.Id));
        Assert.Empty(repository.List());
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
    }
}
