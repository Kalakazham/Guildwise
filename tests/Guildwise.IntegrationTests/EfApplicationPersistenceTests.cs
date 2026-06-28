using Guildwise.Application.Characters.CreateCharacter;
using Guildwise.Application.Characters.SetMainCharacter;
using Guildwise.Application.GuildMembers.AddPlayerToGuild;
using Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;
using Guildwise.Application.RaidTeams.CreateRaidTeam;
using Guildwise.Domain;
using Guildwise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Guildwise.IntegrationTests;

public sealed class EfApplicationPersistenceTests
{
    private const string ConnectionString =
        "Host=localhost;Port=55432;Database=guildwise;Username=guildwise;Password=guildwise";

    [Fact]
    public void CreateCharacter_Persists_Character_For_Existing_Player()
    {
        EnsureDatabaseMigrated();
        var player = AddPlayer();
        var characterName = UniqueName("Alysa");

        using (var actContext = CreateDbContext())
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

        using var assertContext = CreateDbContext();
        var loaded = new EfPlayerRepository(assertContext).GetById(player.Id);
        var character = Assert.Single(loaded!.Characters);
        Assert.Equal(characterName, character.Name);
    }

    [Fact]
    public void SetMainCharacter_Persists_Main_Character_Assignment()
    {
        EnsureDatabaseMigrated();
        var player = AddPlayerWithCharacter();
        var character = player.Characters.Single();

        using (var actContext = CreateDbContext())
        {
            var handler = new SetMainCharacterHandler(new EfPlayerRepository(actContext));

            handler.Handle(new SetMainCharacterCommand(player.Id, character.Id));
        }

        using var assertContext = CreateDbContext();
        var loaded = new EfPlayerRepository(assertContext).GetById(player.Id);
        Assert.Equal(character.Id, loaded!.MainCharacterId);
    }

    [Fact]
    public void CreateRaidTeam_Persists_Raid_Team_For_Existing_Guild()
    {
        EnsureDatabaseMigrated();
        var guild = AddGuild();
        var raidTeamName = UniqueName("RaidTeam");

        using (var actContext = CreateDbContext())
        {
            var handler = new CreateRaidTeamHandler(new EfGuildRepository(actContext));

            handler.Handle(new CreateRaidTeamCommand(guild.Id, raidTeamName));
        }

        using var assertContext = CreateDbContext();
        var loaded = new EfGuildRepository(assertContext).GetById(guild.Id);
        var raidTeam = Assert.Single(loaded!.RaidTeams);
        Assert.Equal(raidTeamName, raidTeam.Name);
    }

    [Fact]
    public void AddPlayerToGuild_Persists_Guild_Membership()
    {
        EnsureDatabaseMigrated();
        var guild = AddGuild();
        var player = AddPlayer();

        using (var actContext = CreateDbContext())
        {
            var guildRepository = new EfGuildRepository(actContext);
            var playerRepository = new EfPlayerRepository(actContext);
            var handler = new AddPlayerToGuildHandler(guildRepository, playerRepository);

            handler.Handle(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member));
        }

        using var assertContext = CreateDbContext();
        var loaded = new EfGuildRepository(assertContext).GetById(guild.Id);
        var member = Assert.Single(loaded!.Members);
        Assert.Equal(player.Id, member.PlayerId);
    }

    [Fact]
    public void AddPlayerToRaidTeam_Persists_Raid_Team_Membership()
    {
        EnsureDatabaseMigrated();
        var player = AddPlayerWithMainCharacter();
        var guild = AddGuildWithMemberAndRaidTeam(player);
        var raidTeam = guild.RaidTeams.Single();

        using (var actContext = CreateDbContext())
        {
            var guildRepository = new EfGuildRepository(actContext);
            var playerRepository = new EfPlayerRepository(actContext);
            var handler = new AddPlayerToRaidTeamHandler(guildRepository, playerRepository);

            handler.Handle(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id));
        }

        using var assertContext = CreateDbContext();
        var loaded = new EfGuildRepository(assertContext).GetById(guild.Id);
        var loadedRaidTeam = Assert.Single(loaded!.RaidTeams);
        var raidTeamMember = Assert.Single(loadedRaidTeam.Members);
        Assert.Equal(player.Id, raidTeamMember.PlayerId);
    }

    private static Player AddPlayer()
    {
        var player = Player.Create(UniqueName("Player"));

        using var context = CreateDbContext();
        new EfPlayerRepository(context).Add(player);

        return player;
    }

    private static Player AddPlayerWithCharacter()
    {
        var player = Player.Create(UniqueName("Player"));
        player.AddCharacter(
            UniqueName("Alysa"),
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);

        using var context = CreateDbContext();
        new EfPlayerRepository(context).Add(player);

        return player;
    }

    private static Player AddPlayerWithMainCharacter()
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

        using var context = CreateDbContext();
        new EfPlayerRepository(context).Add(player);

        return player;
    }

    private static Guild AddGuild()
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");

        using var context = CreateDbContext();
        new EfGuildRepository(context).Add(guild);

        return guild;
    }

    private static Guild AddGuildWithMemberAndRaidTeam(Player player)
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");
        guild.AddMember(player, GuildRank.Member);
        guild.CreateRaidTeam(UniqueName("RaidTeam"));

        using var context = CreateDbContext();
        new EfGuildRepository(context).Add(guild);

        return guild;
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
