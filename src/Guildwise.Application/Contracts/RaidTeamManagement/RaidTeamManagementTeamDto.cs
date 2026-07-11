namespace Guildwise.Application.Contracts.RaidTeamManagement;

public sealed record RaidTeamManagementTeamDto(
    Guid Id,
    Guid GuildId,
    string Name,
    int MemberCount,
    RaidTeamCompositionDto Composition,
    IReadOnlyList<RaidTeamManagementMemberDto> Members);
