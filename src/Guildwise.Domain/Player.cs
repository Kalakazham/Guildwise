namespace Guildwise.Domain;

public sealed class Player
{
    private readonly List<Character> _characters = new();

    public Guid Id { get; } = Guid.NewGuid();

    public string DisplayName { get; private set; }

    public Guid? MainCharacterId { get; private set; }

    public IReadOnlyCollection<Character> Characters => _characters;

    private Player(string displayName)
    {
        DisplayName = DomainGuard.Required(displayName, nameof(displayName));
    }

    public static Player Create(string displayName)
        => new(displayName);

    public void Rename(string displayName)
    {
        DisplayName = DomainGuard.Required(displayName, nameof(displayName));
    }

    public Character AddCharacter(
        string name,
        string region,
        string realm,
        CharacterClass characterClass,
        CharacterSpecialization specialization,
        CharacterRole role)
    {
        if (_characters.Any(existing => Character.HasSameIdentity(name, region, realm, existing)))
        {
            throw new InvalidOperationException("Duplicate character for this player.");
        }

        var character = Character.Create(Id, name, region, realm, characterClass, specialization, role);
        _characters.Add(character);
        return character;
    }

    public void UpdateCharacter(
        Guid characterId,
        string name,
        string region,
        string realm,
        CharacterClass characterClass,
        CharacterSpecialization specialization,
        CharacterRole role)
    {
        var character = _characters.FirstOrDefault(existing => existing.Id == characterId);
        if (character is null)
        {
            throw new InvalidOperationException("Character must belong to this player.");
        }

        if (_characters
            .Where(existing => existing.Id != characterId)
            .Any(existing => Character.HasSameIdentity(name, region, realm, existing)))
        {
            throw new InvalidOperationException("Duplicate character for this player.");
        }

        character.Update(name, region, realm, characterClass, specialization, role);
    }

    public void SetMainCharacter(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);

        if (!_characters.Any(existing => existing.Id == character.Id))
        {
            throw new InvalidOperationException("Main character must belong to this player.");
        }

        MainCharacterId = character.Id;
    }

    public void RemoveCharacter(Guid characterId)
    {
        var character = _characters.FirstOrDefault(existing => existing.Id == characterId);
        if (character is null)
        {
            return;
        }

        _characters.Remove(character);

        if (MainCharacterId == character.Id)
        {
            MainCharacterId = null;
        }
    }
}
