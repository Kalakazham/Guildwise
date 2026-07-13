using System.Reflection;
using Guildwise.Domain;
namespace Guildwise.UnitTests;

internal static class DomainModelTestSupport
{
    public static Character CreateMainReadyCharacter(Player player)
        => player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);

    public static Player CreateRosterReadyMember(Guild guild)
    {
        var player = Player.Create("Myrmi");
        var character = CreateMainReadyCharacter(player);
        player.SetMainCharacter(character);
        guild.AddMember(player, GuildRank.Member);
        return player;
    }

    public static GuildMember CreateGuildMember()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var player = Player.Create("Myrmi");
        return guild.AddMember(player, GuildRank.Officer);
    }

    public static RaidEvent CreateRaidEvent()
        => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null);
}
