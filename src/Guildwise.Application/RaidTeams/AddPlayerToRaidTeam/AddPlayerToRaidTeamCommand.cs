namespace Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;

public sealed record AddPlayerToRaidTeamCommand(Guid GuildId, Guid RaidTeamId, Guid PlayerId);
