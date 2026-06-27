namespace Guildwise.Domain;

public sealed class Character
{
    public Guid Id { get; } = Guid.NewGuid();

    public Guid? PlayerId { get; private set; }

    public string Name { get; private set; }

    public string Region { get; private set; }

    public string Realm { get; private set; }

    public CharacterClass CharacterClass { get; private set; }

    public CharacterSpecialization Specialization { get; private set; }

    public CharacterRole Role { get; private set; }

    private Character(
        string name,
        string region,
        string realm,
        CharacterClass characterClass,
        CharacterSpecialization specialization,
        CharacterRole role)
    {
        Name = DomainGuard.Required(name, nameof(name));
        Region = DomainGuard.Required(region, nameof(region));
        Realm = DomainGuard.Required(realm, nameof(realm));
        DomainGuard.RequiredEnum(characterClass, nameof(characterClass));
        DomainGuard.RequiredEnum(specialization, nameof(specialization));
        DomainGuard.RequiredEnum(role, nameof(role));

        CharacterClass = characterClass;
        Specialization = specialization;
        Role = role;
    }

    public static Character Create(
        string name,
        string region,
        string realm,
        CharacterClass characterClass,
        CharacterSpecialization specialization,
        CharacterRole role)
        => new(name, region, realm, characterClass, specialization, role);

    public void Update(
        string name,
        string region,
        string realm,
        CharacterClass characterClass,
        CharacterSpecialization specialization,
        CharacterRole role)
    {
        Name = DomainGuard.Required(name, nameof(name));
        Region = DomainGuard.Required(region, nameof(region));
        Realm = DomainGuard.Required(realm, nameof(realm));
        DomainGuard.RequiredEnum(characterClass, nameof(characterClass));
        DomainGuard.RequiredEnum(specialization, nameof(specialization));
        DomainGuard.RequiredEnum(role, nameof(role));

        CharacterClass = characterClass;
        Specialization = specialization;
        Role = role;
    }

    internal void AssignToPlayer(Guid playerId)
    {
        if (playerId == Guid.Empty)
        {
            throw new ArgumentException("playerId is required.", nameof(playerId));
        }

        if (PlayerId.HasValue && PlayerId.Value != playerId)
        {
            throw new InvalidOperationException("Character already belongs to another player.");
        }

        PlayerId = playerId;
    }

    internal void ClearPlayer()
    {
        PlayerId = null;
    }

    internal bool HasSameIdentity(Character other)
        => string.Equals(Normalize(Region), Normalize(other.Region), StringComparison.OrdinalIgnoreCase)
            && string.Equals(Normalize(Realm), Normalize(other.Realm), StringComparison.OrdinalIgnoreCase)
            && string.Equals(Normalize(Name), Normalize(other.Name), StringComparison.OrdinalIgnoreCase);

    internal static bool HasSameIdentity(string name, string region, string realm, Character other)
        => string.Equals(Normalize(region), Normalize(other.Region), StringComparison.OrdinalIgnoreCase)
            && string.Equals(Normalize(realm), Normalize(other.Realm), StringComparison.OrdinalIgnoreCase)
            && string.Equals(Normalize(name), Normalize(other.Name), StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string value)
        => value.Trim();
}
