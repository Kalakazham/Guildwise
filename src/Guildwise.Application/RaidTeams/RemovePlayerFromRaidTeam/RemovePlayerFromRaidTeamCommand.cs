namespace Guildwise.Application.RaidTeams.RemovePlayerFromRaidTeam;

public sealed record RemovePlayerFromRaidTeamCommand(Guid GuildId, Guid RaidTeamId, Guid PlayerId);
