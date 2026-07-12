using Guildwise.Domain;

namespace Guildwise.Application.RaidEvents.CreateRaidEvent;

public sealed record CreateRaidEventCommand(
    Guid GuildId,
    Guid RaidTeamId,
    string Title,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    string InstanceName,
    RaidDifficulty Difficulty,
    string? Notes);
