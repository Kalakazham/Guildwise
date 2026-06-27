namespace Guildwise.Application.Characters.DeleteCharacter;

public sealed record DeleteCharacterCommand(Guid PlayerId, Guid CharacterId);
