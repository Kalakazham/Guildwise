namespace Guildwise.Application.RaidEvents.ListRaidEvents;

public sealed record ListRaidEventsQuery(Guid? GuildId = null, Guid? RaidTeamId = null);
