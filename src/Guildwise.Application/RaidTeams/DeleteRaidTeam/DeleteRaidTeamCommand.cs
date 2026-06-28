namespace Guildwise.Application.RaidTeams.DeleteRaidTeam;

public sealed record DeleteRaidTeamCommand(Guid GuildId, Guid RaidTeamId);
