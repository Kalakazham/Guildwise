namespace Guildwise.Application.Characters.GetCharacter;

public sealed record GetCharacterQuery(Guid PlayerId, Guid CharacterId);
