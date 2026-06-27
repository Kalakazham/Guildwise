namespace Guildwise.Domain;

public sealed class RaidTeamMember
{
    public Guid Id { get; } = Guid.NewGuid();

    public Guid RaidTeamId { get; }

    public Guid PlayerId { get; }

    public RaidTeamMember(Guid raidTeamId, Guid playerId)
    {
        if (raidTeamId == Guid.Empty)
        {
            throw new ArgumentException("raidTeamId is required.", nameof(raidTeamId));
        }

        if (playerId == Guid.Empty)
        {
            throw new ArgumentException("playerId is required.", nameof(playerId));
        }

        RaidTeamId = raidTeamId;
        PlayerId = playerId;
    }
}
