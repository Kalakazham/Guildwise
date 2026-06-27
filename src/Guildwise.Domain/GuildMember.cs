namespace Guildwise.Domain;

public sealed class GuildMember
{
    private readonly HashSet<AdditionalGuildRole> _additionalRoles = new();

    public Guid Id { get; } = Guid.NewGuid();

    public Guid GuildId { get; }

    public Guid PlayerId { get; }

    public GuildRank Rank { get; private set; }

    public IReadOnlyCollection<AdditionalGuildRole> AdditionalRoles => _additionalRoles;

    public GuildMember(Guid guildId, Guid playerId, GuildRank rank)
    {
        if (guildId == Guid.Empty)
        {
            throw new ArgumentException("guildId is required.", nameof(guildId));
        }

        if (playerId == Guid.Empty)
        {
            throw new ArgumentException("playerId is required.", nameof(playerId));
        }

        DomainGuard.RequiredEnum(rank, nameof(rank));

        GuildId = guildId;
        PlayerId = playerId;
        Rank = rank;
    }

    public void AddAdditionalRole(AdditionalGuildRole role)
    {
        DomainGuard.RequiredEnum(role, nameof(role));

        if (!_additionalRoles.Add(role))
        {
            throw new InvalidOperationException("Duplicate additional role.");
        }
    }

    public void RemoveAdditionalRole(AdditionalGuildRole role)
    {
        _additionalRoles.Remove(role);
    }
}
