namespace Guildwise.Domain;

public sealed class RaidEventSignup
{
    public Guid Id { get; } = Guid.NewGuid();

    public Guid RaidEventId { get; }

    public Guid PlayerId { get; }

    public RaidEventSignupStatus Status { get; private set; }

    internal RaidEventSignup(Guid raidEventId, Guid playerId, RaidEventSignupStatus status)
    {
        if (raidEventId == Guid.Empty)
        {
            throw new ArgumentException("raidEventId is required.", nameof(raidEventId));
        }

        if (playerId == Guid.Empty)
        {
            throw new ArgumentException("playerId is required.", nameof(playerId));
        }

        DomainGuard.RequiredEnum(status, nameof(status));

        RaidEventId = raidEventId;
        PlayerId = playerId;
        Status = status;
    }

    internal void UpdateStatus(RaidEventSignupStatus status)
    {
        DomainGuard.RequiredEnum(status, nameof(status));

        Status = status;
    }
}
