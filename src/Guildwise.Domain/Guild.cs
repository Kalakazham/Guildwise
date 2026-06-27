namespace Guildwise.Domain;

public sealed class Guild
{
    private readonly List<RaidTeam> _raidTeams = new();
    private readonly List<GuildMember> _members = new();

    public Guid Id { get; } = Guid.NewGuid();

    public string Name { get; private set; }

    public string Region { get; private set; }

    public string Realm { get; private set; }

    public IReadOnlyCollection<RaidTeam> RaidTeams => _raidTeams;

    public IReadOnlyCollection<GuildMember> Members => _members;

    private Guild(string name, string region, string realm)
    {
        Name = DomainGuard.Required(name, nameof(name));
        Region = DomainGuard.Required(region, nameof(region));
        Realm = DomainGuard.Required(realm, nameof(realm));
    }

    public static Guild Create(string name, string region, string realm)
        => new(name, region, realm);

    public void Update(string name, string region, string realm)
    {
        Name = DomainGuard.Required(name, nameof(name));
        Region = DomainGuard.Required(region, nameof(region));
        Realm = DomainGuard.Required(realm, nameof(realm));
    }

    public void AddRaidTeam(RaidTeam raidTeam)
    {
        ArgumentNullException.ThrowIfNull(raidTeam);

        if (_raidTeams.Any(existing => existing.Id == raidTeam.Id))
        {
            throw new InvalidOperationException("Raid team already belongs to this guild.");
        }

        if (_raidTeams.Any(existing => string.Equals(existing.Name, raidTeam.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Raid team name must be unique within the guild.");
        }

        raidTeam.AssignToGuild(Id);
        _raidTeams.Add(raidTeam);
    }

    public GuildMember AddMember(Player player, GuildRank rank)
    {
        ArgumentNullException.ThrowIfNull(player);
        DomainGuard.RequiredEnum(rank, nameof(rank));

        if (_members.Any(existing => existing.PlayerId == player.Id))
        {
            throw new InvalidOperationException("Player is already a guild member.");
        }

        var member = new GuildMember(Id, player.Id, rank);
        _members.Add(member);
        return member;
    }

    public void RemoveMember(Guid playerId)
    {
        _members.RemoveAll(member => member.PlayerId == playerId);
    }
}
