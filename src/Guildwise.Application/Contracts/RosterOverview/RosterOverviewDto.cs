namespace Guildwise.Application.Contracts.RosterOverview;

public sealed record RosterOverviewDto(
    RosterSummaryDto Summary,
    IReadOnlyList<RosterGuildSummaryDto> Guilds,
    IReadOnlyList<RosterMemberDto> Members);
