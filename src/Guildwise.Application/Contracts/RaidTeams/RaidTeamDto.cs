namespace Guildwise.Application.Contracts.RaidTeams;

public sealed record RaidTeamDto(
    Guid Id,
    Guid GuildId,
    string Name,
    IReadOnlyList<RaidTeamMemberDto> Members);
