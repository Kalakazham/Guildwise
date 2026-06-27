namespace Guildwise.Domain;

public sealed class Character
{
    public Guid Id { get; } = Guid.NewGuid();

    public Guid PlayerId { get; }

    public string Name { get; private set; }

    public string Region { get; private set; }

    public string Realm { get; private set; }

    public CharacterClass CharacterClass { get; private set; }

    public CharacterSpecialization Specialization { get; private set; }

    public CharacterRole Role { get; private set; }

    internal Character(
        Guid playerId,
        string name,
        string region,
        string realm,
        CharacterClass characterClass,
        CharacterSpecialization specialization,
        CharacterRole role)
    {
        if (playerId == Guid.Empty)
        {
            throw new ArgumentException("playerId is required.", nameof(playerId));
        }

        PlayerId = playerId;
        Name = DomainGuard.Required(name, nameof(name));
        Region = DomainGuard.Required(region, nameof(region));
        Realm = DomainGuard.Required(realm, nameof(realm));
        DomainGuard.RequiredEnum(characterClass, nameof(characterClass));
        DomainGuard.RequiredEnum(specialization, nameof(specialization));
        DomainGuard.RequiredEnum(role, nameof(role));
        EnsureSpecializationMatchesClass(characterClass, specialization);

        CharacterClass = characterClass;
        Specialization = specialization;
        Role = role;
    }

    internal static Character Create(
        Guid playerId,
        string name,
        string region,
        string realm,
        CharacterClass characterClass,
        CharacterSpecialization specialization,
        CharacterRole role)
        => new(playerId, name, region, realm, characterClass, specialization, role);

    internal void Update(
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
        EnsureSpecializationMatchesClass(characterClass, specialization);

        CharacterClass = characterClass;
        Specialization = specialization;
        Role = role;
    }

    internal static bool HasSameIdentity(string name, string region, string realm, Character other)
        => string.Equals(Normalize(region), Normalize(other.Region), StringComparison.OrdinalIgnoreCase)
            && string.Equals(Normalize(realm), Normalize(other.Realm), StringComparison.OrdinalIgnoreCase)
            && string.Equals(Normalize(name), Normalize(other.Name), StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string value)
        => value.Trim();

    private static void EnsureSpecializationMatchesClass(
        CharacterClass characterClass,
        CharacterSpecialization specialization)
    {
        var isValid = characterClass switch
        {
            CharacterClass.DeathKnight => specialization
                is CharacterSpecialization.DeathKnightBlood
                or CharacterSpecialization.DeathKnightFrost
                or CharacterSpecialization.DeathKnightUnholy,
            CharacterClass.DemonHunter => specialization
                is CharacterSpecialization.DemonHunterHavoc
                or CharacterSpecialization.DemonHunterVengeance,
            CharacterClass.Druid => specialization
                is CharacterSpecialization.DruidBalance
                or CharacterSpecialization.DruidFeral
                or CharacterSpecialization.DruidGuardian
                or CharacterSpecialization.DruidRestoration,
            CharacterClass.Evoker => specialization
                is CharacterSpecialization.EvokerAugmentation
                or CharacterSpecialization.EvokerDevastation
                or CharacterSpecialization.EvokerPreservation,
            CharacterClass.Hunter => specialization
                is CharacterSpecialization.HunterBeastMastery
                or CharacterSpecialization.HunterMarksmanship
                or CharacterSpecialization.HunterSurvival,
            CharacterClass.Mage => specialization
                is CharacterSpecialization.MageArcane
                or CharacterSpecialization.MageFire
                or CharacterSpecialization.MageFrost,
            CharacterClass.Monk => specialization
                is CharacterSpecialization.MonkBrewmaster
                or CharacterSpecialization.MonkMistweaver
                or CharacterSpecialization.MonkWindwalker,
            CharacterClass.Paladin => specialization
                is CharacterSpecialization.PaladinHoly
                or CharacterSpecialization.PaladinProtection
                or CharacterSpecialization.PaladinRetribution,
            CharacterClass.Priest => specialization
                is CharacterSpecialization.PriestDiscipline
                or CharacterSpecialization.PriestHoly
                or CharacterSpecialization.PriestShadow,
            CharacterClass.Rogue => specialization
                is CharacterSpecialization.RogueAssassination
                or CharacterSpecialization.RogueOutlaw
                or CharacterSpecialization.RogueSubtlety,
            CharacterClass.Shaman => specialization
                is CharacterSpecialization.ShamanElemental
                or CharacterSpecialization.ShamanEnhancement
                or CharacterSpecialization.ShamanRestoration,
            CharacterClass.Warlock => specialization
                is CharacterSpecialization.WarlockAffliction
                or CharacterSpecialization.WarlockDemonology
                or CharacterSpecialization.WarlockDestruction,
            CharacterClass.Warrior => specialization
                is CharacterSpecialization.WarriorArms
                or CharacterSpecialization.WarriorFury
                or CharacterSpecialization.WarriorProtection,
            _ => false
        };

        if (!isValid)
        {
            throw new InvalidOperationException("Character specialization must match character class.");
        }
    }
}
