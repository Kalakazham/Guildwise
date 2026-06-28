using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;
using Guildwise.Infrastructure;
using Guildwise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Guildwise.IntegrationTests;

public sealed class EfRepositoryTests
{
    private const string ConnectionString =
        "Host=localhost;Port=55432;Database=guildwise;Username=guildwise;Password=guildwise";

    [Fact]
    public void AddPostgresInfrastructure_Registers_Ef_Aggregate_Root_Repositories()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:GuildwiseDatabase"] = ConnectionString
            })
            .Build();
        var services = new ServiceCollection();

        services.AddPostgresInfrastructure(configuration);

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
    }

    [Fact]
    public void EfPlayerRepository_Saves_And_Loads_Player()
    {
        EnsureDatabaseMigrated();
        var displayName = UniqueName("Player");
        var player = Player.Create(displayName);

        using (var arrangeContext = CreateDbContext())
        {
            new EfPlayerRepository(arrangeContext).Add(player);
        }

        using var assertContext = CreateDbContext();
        var loaded = new EfPlayerRepository(assertContext).GetById(player.Id);

        Assert.NotNull(loaded);
        Assert.Equal(displayName, loaded.DisplayName);
        Assert.Null(loaded.MainCharacterId);
        Assert.Empty(loaded.Characters);
    }

    [Fact]
    public void EfPlayerRepository_Saves_And_Loads_Player_With_Characters_And_MainCharacter()
    {
        EnsureDatabaseMigrated();
        var player = Player.Create(UniqueName("Player"));
        var character = player.AddCharacter(
            UniqueName("Alysa"),
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);
        player.SetMainCharacter(character);

        using (var arrangeContext = CreateDbContext())
        {
            new EfPlayerRepository(arrangeContext).Add(player);
        }

        using var assertContext = CreateDbContext();
        var loaded = new EfPlayerRepository(assertContext).GetById(player.Id);

        Assert.NotNull(loaded);
        Assert.Equal(character.Id, loaded.MainCharacterId);
        var loadedCharacter = Assert.Single(loaded.Characters);
        Assert.Equal(character.Id, loadedCharacter.Id);
        Assert.Equal(CharacterClass.Paladin, loadedCharacter.CharacterClass);
        Assert.Equal(CharacterSpecialization.PaladinRetribution, loadedCharacter.Specialization);
        Assert.Equal(CharacterRole.Damage, loadedCharacter.Role);
    }

    [Fact]
    public void EfGuildRepository_Saves_And_Loads_Guild()
    {
        EnsureDatabaseMigrated();
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");

        using (var arrangeContext = CreateDbContext())
        {
            new EfGuildRepository(arrangeContext).Add(guild);
        }

        using var assertContext = CreateDbContext();
        var loaded = new EfGuildRepository(assertContext).GetById(guild.Id);

        Assert.NotNull(loaded);
        Assert.Equal(guild.Name, loaded.Name);
        Assert.Equal("EU", loaded.Region);
        Assert.Equal("Draenor", loaded.Realm);
        Assert.Empty(loaded.Members);
        Assert.Empty(loaded.RaidTeams);
    }

    [Fact]
    public void EfGuildRepository_Saves_And_Loads_Guild_Members_RaidTeams_And_RaidTeamMembers()
    {
        EnsureDatabaseMigrated();
        var player = Player.Create(UniqueName("Player"));
        var character = player.AddCharacter(
            UniqueName("Alysa"),
            "EU",
            "Draenor",
            CharacterClass.Shaman,
            CharacterSpecialization.ShamanRestoration,
            CharacterRole.Healer);
        player.SetMainCharacter(character);
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");
        var guildMember = guild.AddMember(player, GuildRank.Officer);
        guildMember.AddAdditionalRole(AdditionalGuildRole.RaidLead);
        guildMember.AddAdditionalRole(AdditionalGuildRole.Recruiter);
        var raidTeam = guild.CreateRaidTeam(UniqueName("RaidTeam"));
        guild.AddPlayerToRaidTeam(raidTeam, player);

        using (var playerContext = CreateDbContext())
        {
            new EfPlayerRepository(playerContext).Add(player);
        }

        using (var guildContext = CreateDbContext())
        {
            new EfGuildRepository(guildContext).Add(guild);
        }

        using var assertContext = CreateDbContext();
        var loaded = new EfGuildRepository(assertContext).GetById(guild.Id);

        Assert.NotNull(loaded);
        var loadedMember = Assert.Single(loaded.Members);
        Assert.Equal(player.Id, loadedMember.PlayerId);
        Assert.Equal(GuildRank.Officer, loadedMember.Rank);
        Assert.Equal(
            [AdditionalGuildRole.RaidLead, AdditionalGuildRole.Recruiter],
            loadedMember.AdditionalRoles.OrderBy(role => role).ToArray());

        var loadedRaidTeam = Assert.Single(loaded.RaidTeams);
        Assert.Equal(raidTeam.Name, loadedRaidTeam.Name);
        var loadedRaidTeamMember = Assert.Single(loadedRaidTeam.Members);
        Assert.Equal(player.Id, loadedRaidTeamMember.PlayerId);
    }

    private static GuildwiseDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GuildwiseDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new GuildwiseDbContext(options);
    }

    private static void EnsureDatabaseMigrated()
    {
        using var context = CreateDbContext();

        context.Database.Migrate();
    }

    private static string UniqueName(string prefix)
        => $"{prefix}{Guid.NewGuid():N}";
}
