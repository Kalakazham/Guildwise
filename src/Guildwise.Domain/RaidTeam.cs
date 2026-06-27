namespace Guildwise.Domain;

public sealed class RaidTeam
{
    private readonly List<RaidTeamMember> _members = new();

    public Guid Id { get; } = Guid.NewGuid();

    public Guid GuildId { get; private set; }

    public string Name { get; private set; }

    public IReadOnlyCollection<RaidTeamMember> Members => _members;

    private RaidTeam(string name)
    {
        Name = DomainGuard.Required(name, nameof(name));
    }

    public static RaidTeam Create(string name)
        => new(name);

    public void Rename(string name)
    {
        Name = DomainGuard.Required(name, nameof(name));
    }

    public void AddPlayer(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!player.MainCharacterId.HasValue)
        {
            throw new InvalidOperationException("Player must have a main character before joining a raid team.");
        }

        if (_members.Any(member => member.PlayerId == player.Id))
        {
            throw new InvalidOperationException("Player is already a member of this raid team.");
        }

        _members.Add(new RaidTeamMember(Id, player.Id));
    }

    public void RemovePlayer(Guid playerId)
    {
        _members.RemoveAll(member => member.PlayerId == playerId);
    }

    internal void AssignToGuild(Guid guildId)
    {
        if (guildId == Guid.Empty)
        {
            throw new ArgumentException("guildId is required.", nameof(guildId));
        }

        if (GuildId != Guid.Empty && GuildId != guildId)
        {
            throw new InvalidOperationException("Raid team already belongs to another guild.");
        }

        GuildId = guildId;
    }
}
