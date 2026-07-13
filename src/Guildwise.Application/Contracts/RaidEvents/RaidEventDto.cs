using Guildwise.Domain;

namespace Guildwise.Application.Contracts.RaidEvents;

public sealed record RaidEventDto(
    Guid Id,
    Guid GuildId,
    Guid RaidTeamId,
    string Title,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    string InstanceName,
    RaidDifficulty Difficulty,
    RaidEventStatus Status,
    string Notes);
