namespace Guildwise.Application.RaidTeams.CreateRaidTeam;

public sealed record CreateRaidTeamCommand(Guid GuildId, string Name);
