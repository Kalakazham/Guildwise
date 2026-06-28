using Guildwise.Application.Characters.CreateCharacter;
using Guildwise.Application.Characters.SetMainCharacter;
using Guildwise.Application.GuildMembers.AddPlayerToGuild;
using Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;
using Guildwise.Application.RaidTeams.CreateRaidTeam;
using Guildwise.Domain;
using Guildwise.Infrastructure.Persistence;
using Guildwise.IntegrationTests.Persistence;

namespace Guildwise.IntegrationTests;

[Collection(PostgreSqlTestCollection.Name)]
public sealed class EfApplicationPersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlTestFixture _fixture;

    public EfApplicationPersistenceTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    public Task InitializeAsync()
        => _fixture.ResetDatabaseAsync();

    public Task DisposeAsync()
        => Task.CompletedTask;

    [Fact]
    public void CreateCharacter_Persists_Character_For_Existing_Player()
    {
        var player = AddPlayer();
        var characterName = UniqueName("Alysa");

        using (var actContext = _fixture.CreateDbContext())
        {
            var handler = new CreateCharacterHandler(new EfPlayerRepository(actContext));

            handler.Handle(new CreateCharacterCommand(
                player.Id,
                characterName,
                "EU",
                "Draenor",
                CharacterClass.Mage,
                CharacterSpecialization.MageFrost,
                CharacterRole.Damage));
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = new EfPlayerRepository(assertContext).GetById(player.Id);
        var character = Assert.Single(loaded!.Characters);
        Assert.Equal(characterName, character.Name);
    }

    [Fact]
    public void SetMainCharacter_Persists_Main_Character_Assignment()
    {
        var player = AddPlayerWithCharacter();
        var character = player.Characters.Single();

        using (var actContext = _fixture.CreateDbContext())
        {
            var handler = new SetMainCharacterHandler(new EfPlayerRepository(actContext));

            handler.Handle(new SetMainCharacterCommand(player.Id, character.Id));
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = new EfPlayerRepository(assertContext).GetById(player.Id);
        Assert.Equal(character.Id, loaded!.MainCharacterId);
    }

    [Fact]
    public void CreateRaidTeam_Persists_Raid_Team_For_Existing_Guild()
    {
        var guild = AddGuild();
        var raidTeamName = UniqueName("RaidTeam");

        using (var actContext = _fixture.CreateDbContext())
        {
            var handler = new CreateRaidTeamHandler(new EfGuildRepository(actContext));

            handler.Handle(new CreateRaidTeamCommand(guild.Id, raidTeamName));
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = new EfGuildRepository(assertContext).GetById(guild.Id);
        var raidTeam = Assert.Single(loaded!.RaidTeams);
        Assert.Equal(raidTeamName, raidTeam.Name);
    }

    [Fact]
    public void AddPlayerToGuild_Persists_Guild_Membership()
    {
        var guild = AddGuild();
        var player = AddPlayer();

        using (var actContext = _fixture.CreateDbContext())
        {
            var guildRepository = new EfGuildRepository(actContext);
            var playerRepository = new EfPlayerRepository(actContext);
            var handler = new AddPlayerToGuildHandler(guildRepository, playerRepository);

            handler.Handle(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member));
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = new EfGuildRepository(assertContext).GetById(guild.Id);
        var member = Assert.Single(loaded!.Members);
        Assert.Equal(player.Id, member.PlayerId);
    }

    [Fact]
    public void AddPlayerToRaidTeam_Persists_Raid_Team_Membership()
    {
        var player = AddPlayerWithMainCharacter();
        var guild = AddGuildWithMemberAndRaidTeam(player);
        var raidTeam = guild.RaidTeams.Single();

        using (var actContext = _fixture.CreateDbContext())
        {
            var guildRepository = new EfGuildRepository(actContext);
            var playerRepository = new EfPlayerRepository(actContext);
            var handler = new AddPlayerToRaidTeamHandler(guildRepository, playerRepository);

            handler.Handle(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id));
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = new EfGuildRepository(assertContext).GetById(guild.Id);
        var loadedRaidTeam = Assert.Single(loaded!.RaidTeams);
        var raidTeamMember = Assert.Single(loadedRaidTeam.Members);
        Assert.Equal(player.Id, raidTeamMember.PlayerId);
    }

    private Player AddPlayer()
    {
        var player = Player.Create(UniqueName("Player"));

        using var context = _fixture.CreateDbContext();
        new EfPlayerRepository(context).Add(player);

        return player;
    }

    private Player AddPlayerWithCharacter()
    {
        var player = Player.Create(UniqueName("Player"));
        player.AddCharacter(
            UniqueName("Alysa"),
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);

        using var context = _fixture.CreateDbContext();
        new EfPlayerRepository(context).Add(player);

        return player;
    }

    private Player AddPlayerWithMainCharacter()
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

        using var context = _fixture.CreateDbContext();
        new EfPlayerRepository(context).Add(player);

        return player;
    }

    private Guild AddGuild()
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");

        using var context = _fixture.CreateDbContext();
        new EfGuildRepository(context).Add(guild);

        return guild;
    }

    private Guild AddGuildWithMemberAndRaidTeam(Player player)
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");
        guild.AddMember(player, GuildRank.Member);
        guild.CreateRaidTeam(UniqueName("RaidTeam"));

        using var context = _fixture.CreateDbContext();
        new EfGuildRepository(context).Add(guild);

        return guild;
    }

    private static string UniqueName(string prefix)
        => $"{prefix}{Guid.NewGuid():N}";
}
