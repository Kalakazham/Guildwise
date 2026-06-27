namespace Guildwise.Application.Contracts.RaidTeams;

public sealed record RaidTeamMemberDto(
    Guid Id,
    Guid RaidTeamId,
    Guid PlayerId);
