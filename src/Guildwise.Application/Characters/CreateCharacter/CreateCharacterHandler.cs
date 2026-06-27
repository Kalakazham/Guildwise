using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Characters;

namespace Guildwise.Application.Characters.CreateCharacter;

public sealed class CreateCharacterHandler
{
    private readonly IPlayerRepository _playerRepository;

    public CreateCharacterHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public CharacterDto Handle(CreateCharacterCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = _playerRepository.GetPlayerOrThrow(command.PlayerId);
        var character = player.AddCharacter(
            command.Name,
            command.Region,
            command.Realm,
            command.CharacterClass,
            command.Specialization,
            command.Role);

        return DtoMapper.ToDto(character);
    }
}
