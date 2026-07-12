using Guildwise.Domain;

namespace Guildwise.Application.RaidEvents.UpdateRaidEvent;

public sealed record UpdateRaidEventCommand(
    Guid RaidEventId,
    Guid GuildId,
    Guid RaidTeamId,
    string Title,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    string InstanceName,
    RaidDifficulty Difficulty,
    string? Notes);
