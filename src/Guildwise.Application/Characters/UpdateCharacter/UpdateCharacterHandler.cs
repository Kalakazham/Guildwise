using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Characters;

namespace Guildwise.Application.Characters.UpdateCharacter;

public sealed class UpdateCharacterHandler
{
    private readonly IPlayerRepository _playerRepository;

    public UpdateCharacterHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public CharacterDto Handle(UpdateCharacterCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = _playerRepository.GetPlayerOrThrow(command.PlayerId);
        player.UpdateCharacter(
            command.CharacterId,
            command.Name,
            command.Region,
            command.Realm,
            command.CharacterClass,
            command.Specialization,
            command.Role);

        return DtoMapper.ToDto(player.GetCharacterOrThrow(command.CharacterId));
    }
}
