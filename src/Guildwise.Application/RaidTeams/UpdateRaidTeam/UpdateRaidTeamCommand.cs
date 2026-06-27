namespace Guildwise.Application.RaidTeams.UpdateRaidTeam;

public sealed record UpdateRaidTeamCommand(Guid GuildId, Guid RaidTeamId, string Name);
