using Guildwise.Application.Contracts.Characters;
using Guildwise.Application.Contracts.GuildMembers;
using Guildwise.Application.Contracts.Guilds;
using Guildwise.Application.Contracts.Players;
using Guildwise.Application.Contracts.RaidTeams;
using Guildwise.Domain;

namespace Guildwise.Application.Common;

internal static class DtoMapper
{
    internal static GuildDto ToDto(Guild guild)
        => new(
            guild.Id,
            guild.Name,
            guild.Region,
            guild.Realm,
            guild.RaidTeams.Select(ToDto).ToList(),
            guild.Members.Select(ToDto).ToList());

    internal static PlayerDto ToDto(Player player)
        => new(
            player.Id,
            player.DisplayName,
            player.MainCharacterId,
            player.Characters.Select(ToDto).ToList());

    internal static CharacterDto ToDto(Character character)
        => new(
            character.Id,
            character.PlayerId,
            character.Name,
            character.Region,
            character.Realm,
            character.CharacterClass,
            character.Specialization,
            character.Role);

    internal static RaidTeamDto ToDto(RaidTeam raidTeam)
        => new(
            raidTeam.Id,
            raidTeam.GuildId,
            raidTeam.Name,
            raidTeam.Members.Select(ToDto).ToList());

    internal static GuildMemberDto ToDto(GuildMember member)
        => new(
            member.Id,
            member.GuildId,
            member.PlayerId,
            member.Rank,
            member.AdditionalRoles.ToList());

    internal static RaidTeamMemberDto ToDto(RaidTeamMember member)
        => new(
            member.Id,
            member.RaidTeamId,
            member.PlayerId);
}
