namespace Guildwise.Application.Contracts.RaidTeamManagement;

public sealed record RaidTeamManagementGuildDto(
    Guid Id,
    string Name,
    string Region,
    string Realm,
    int RaidTeamCount,
    int GuildMemberCount,
    int RaidMemberCount,
    int UnassignedGuildMemberCount,
    int PlayersWithoutMainCharacterCount,
    IReadOnlyList<AvailableRaidTeamPlayerDto> AvailablePlayers,
    IReadOnlyList<RaidTeamManagementTeamDto> Teams);
