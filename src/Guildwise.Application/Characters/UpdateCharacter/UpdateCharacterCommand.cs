using Guildwise.Domain;

namespace Guildwise.Application.Characters.UpdateCharacter;

public sealed record UpdateCharacterCommand(
    Guid PlayerId,
    Guid CharacterId,
    string Name,
    string Region,
    string Realm,
    CharacterClass CharacterClass,
    CharacterSpecialization Specialization,
    CharacterRole Role);
