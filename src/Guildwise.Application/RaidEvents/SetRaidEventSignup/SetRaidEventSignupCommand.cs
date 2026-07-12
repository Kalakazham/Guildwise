using Guildwise.Domain;

namespace Guildwise.Application.RaidEvents.SetRaidEventSignup;

public sealed record SetRaidEventSignupCommand(
    Guid RaidEventId,
    Guid PlayerId,
    RaidEventSignupStatus Status);
