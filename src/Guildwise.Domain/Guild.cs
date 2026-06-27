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

    public RaidTeam CreateRaidTeam(string name)
    {
        var normalizedName = DomainGuard.Required(name, nameof(name));

        if (_raidTeams.Any(existing => string.Equals(existing.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Raid team name must be unique within the guild.");
        }

        var raidTeam = new RaidTeam(Id, normalizedName);
        _raidTeams.Add(raidTeam);
        return raidTeam;
    }

    public void RenameRaidTeam(RaidTeam raidTeam, string name)
    {
        ArgumentNullException.ThrowIfNull(raidTeam);

        if (raidTeam.GuildId != Id || !_raidTeams.Any(existing => existing.Id == raidTeam.Id))
        {
            throw new InvalidOperationException("Raid team must belong to this guild.");
        }

        var normalizedName = DomainGuard.Required(name, nameof(name));

        if (_raidTeams
            .Where(existing => existing.Id != raidTeam.Id)
            .Any(existing => string.Equals(existing.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Raid team name must be unique within the guild.");
        }

        raidTeam.Rename(normalizedName);
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

        foreach (var raidTeam in _raidTeams)
        {
            raidTeam.RemovePlayer(playerId);
        }
    }

    public void AddPlayerToRaidTeam(RaidTeam raidTeam, Player player)
    {
        ArgumentNullException.ThrowIfNull(raidTeam);
        ArgumentNullException.ThrowIfNull(player);

        if (raidTeam.GuildId != Id)
        {
            throw new InvalidOperationException("Raid team must belong to this guild.");
        }

        if (!_members.Any(member => member.PlayerId == player.Id))
        {
            throw new InvalidOperationException("Player must be a guild member before joining a raid team.");
        }

        if (!player.MainCharacterId.HasValue)
        {
            throw new InvalidOperationException("Player must have a main character before joining a raid team.");
        }

        raidTeam.AddPlayer(player.Id);
    }

    public void RemovePlayerFromRaidTeam(RaidTeam raidTeam, Guid playerId)
    {
        ArgumentNullException.ThrowIfNull(raidTeam);

        if (raidTeam.GuildId != Id || !_raidTeams.Any(existing => existing.Id == raidTeam.Id))
        {
            throw new InvalidOperationException("Raid team must belong to this guild.");
        }

        raidTeam.RemovePlayer(playerId);
    }
}
