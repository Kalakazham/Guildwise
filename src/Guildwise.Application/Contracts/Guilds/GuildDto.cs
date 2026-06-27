using Guildwise.Application.Contracts.GuildMembers;
using Guildwise.Application.Contracts.RaidTeams;

namespace Guildwise.Application.Contracts.Guilds;

public sealed record GuildDto(
    Guid Id,
    string Name,
    string Region,
    string Realm,
    IReadOnlyList<RaidTeamDto> RaidTeams,
    IReadOnlyList<GuildMemberDto> Members);
