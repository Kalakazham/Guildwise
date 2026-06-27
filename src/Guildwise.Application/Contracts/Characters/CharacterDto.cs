using Guildwise.Domain;

namespace Guildwise.Application.Contracts.Characters;

public sealed record CharacterDto(
    Guid Id,
    Guid PlayerId,
    string Name,
    string Region,
    string Realm,
    CharacterClass CharacterClass,
    CharacterSpecialization Specialization,
    CharacterRole Role);
