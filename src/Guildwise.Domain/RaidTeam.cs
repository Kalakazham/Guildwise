namespace Guildwise.Domain;

public sealed class RaidTeam
{
    private readonly List<RaidTeamMember> _members = new();

    public Guid Id { get; } = Guid.NewGuid();

    public Guid GuildId { get; }

    public string Name { get; private set; }

    public IReadOnlyCollection<RaidTeamMember> Members => _members;

    internal RaidTeam(Guid guildId, string name)
    {
        if (guildId == Guid.Empty)
        {
            throw new ArgumentException("guildId is required.", nameof(guildId));
        }

        GuildId = guildId;
        Name = DomainGuard.Required(name, nameof(name));
    }

    internal void Rename(string name)
    {
        Name = DomainGuard.Required(name, nameof(name));
    }

    internal void AddPlayer(Guid playerId)
    {
        if (_members.Any(member => member.PlayerId == playerId))
        {
            throw new InvalidOperationException("Player is already a member of this raid team.");
        }

        _members.Add(new RaidTeamMember(Id, playerId));
    }

    internal void RemovePlayer(Guid playerId)
    {
        _members.RemoveAll(member => member.PlayerId == playerId);
    }
}
