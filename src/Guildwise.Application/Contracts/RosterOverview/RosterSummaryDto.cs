namespace Guildwise.Application.Contracts.RosterOverview;

public sealed record RosterSummaryDto(
    int GuildCount,
    int PlayerCount,
    int CharacterCount,
    int RaidTeamCount,
    int GuildMemberCount,
    int RaidRosterMemberCount,
    int PlayersWithMainCharacterCount);
