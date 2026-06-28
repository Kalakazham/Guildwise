using Guildwise.Domain;

namespace Guildwise.Application.Characters.CreateCharacter;

public sealed record CreateCharacterCommand(
    Guid PlayerId,
    string Name,
    string Region,
    string Realm,
    CharacterClass CharacterClass,
    CharacterSpecialization Specialization,
    CharacterRole Role);
