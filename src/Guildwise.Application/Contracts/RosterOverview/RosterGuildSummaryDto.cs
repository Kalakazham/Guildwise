namespace Guildwise.Application.Contracts.RosterOverview;

public sealed record RosterGuildSummaryDto(
    Guid Id,
    string Name,
    string Region,
    string Realm,
    int RaidTeamCount,
    int MemberCount);
