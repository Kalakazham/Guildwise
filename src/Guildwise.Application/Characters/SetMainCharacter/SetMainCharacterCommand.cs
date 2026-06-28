namespace Guildwise.Application.Characters.SetMainCharacter;

public sealed record SetMainCharacterCommand(Guid PlayerId, Guid CharacterId);
