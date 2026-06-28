using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;
using Guildwise.Infrastructure;
using Guildwise.Infrastructure.Persistence;
using Guildwise.IntegrationTests.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Guildwise.IntegrationTests;

[Collection(PostgreSqlTestCollection.Name)]
public sealed class EfRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlTestFixture _fixture;

    public EfRepositoryTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    public Task InitializeAsync()
        => _fixture.ResetDatabaseAsync();

    public Task DisposeAsync()
        => Task.CompletedTask;

    [Fact]
    public void AddPostgresInfrastructure_Registers_Ef_Aggregate_Root_Repositories()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:GuildwiseDatabase"] = _fixture.ConnectionString
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
    public async Task EfPlayerRepository_Saves_And_Loads_Player()
    {
        var displayName = UniqueName("Player");
        var player = Player.Create(displayName);

        using (var arrangeContext = _fixture.CreateDbContext())
        {
            await new EfPlayerRepository(arrangeContext).AddAsync(player);
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfPlayerRepository(assertContext).GetByIdAsync(player.Id);

        Assert.NotNull(loaded);
        Assert.Equal(displayName, loaded.DisplayName);
        Assert.Null(loaded.MainCharacterId);
        Assert.Empty(loaded.Characters);
    }

    [Fact]
    public async Task EfPlayerRepository_Saves_And_Loads_Player_With_Characters_And_MainCharacter()
    {
        var player = Player.Create(UniqueName("Player"));
        var character = player.AddCharacter(
            UniqueName("Alysa"),
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);
        player.SetMainCharacter(character);

        using (var arrangeContext = _fixture.CreateDbContext())
        {
            await new EfPlayerRepository(arrangeContext).AddAsync(player);
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfPlayerRepository(assertContext).GetByIdAsync(player.Id);

        Assert.NotNull(loaded);
        Assert.Equal(character.Id, loaded.MainCharacterId);
        var loadedCharacter = Assert.Single(loaded.Characters);
        Assert.Equal(character.Id, loadedCharacter.Id);
        Assert.Equal(CharacterClass.Paladin, loadedCharacter.CharacterClass);
        Assert.Equal(CharacterSpecialization.PaladinRetribution, loadedCharacter.Specialization);
        Assert.Equal(CharacterRole.Damage, loadedCharacter.Role);
    }

    [Fact]
    public async Task EfGuildRepository_Saves_And_Loads_Guild()
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");

        using (var arrangeContext = _fixture.CreateDbContext())
        {
            await new EfGuildRepository(arrangeContext).AddAsync(guild);
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfGuildRepository(assertContext).GetByIdAsync(guild.Id);

        Assert.NotNull(loaded);
        Assert.Equal(guild.Name, loaded.Name);
        Assert.Equal("EU", loaded.Region);
        Assert.Equal("Draenor", loaded.Realm);
        Assert.Empty(loaded.Members);
        Assert.Empty(loaded.RaidTeams);
    }

    [Fact]
    public async Task EfGuildRepository_Saves_And_Loads_Guild_Members_RaidTeams_And_RaidTeamMembers()
    {
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

        using (var playerContext = _fixture.CreateDbContext())
        {
            await new EfPlayerRepository(playerContext).AddAsync(player);
        }

        using (var guildContext = _fixture.CreateDbContext())
        {
            await new EfGuildRepository(guildContext).AddAsync(guild);
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfGuildRepository(assertContext).GetByIdAsync(guild.Id);

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

    private static string UniqueName(string prefix)
        => $"{prefix}{Guid.NewGuid():N}";
}
