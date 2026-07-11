namespace Guildwise.Application.Contracts.RaidTeamManagement;

public sealed record RaidTeamManagementOverviewDto(
    IReadOnlyList<RaidTeamManagementGuildDto> Guilds);
