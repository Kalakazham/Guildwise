using Guildwise.Application.Contracts.Characters;

namespace Guildwise.Application.Contracts.Players;

public sealed record PlayerDto(
    Guid Id,
    string DisplayName,
    Guid? MainCharacterId,
    IReadOnlyList<CharacterDto> Characters);
