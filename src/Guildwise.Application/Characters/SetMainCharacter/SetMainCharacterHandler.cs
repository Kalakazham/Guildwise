using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Players;

namespace Guildwise.Application.Characters.SetMainCharacter;

public sealed class SetMainCharacterHandler
{
    private readonly IPlayerRepository _playerRepository;

    public SetMainCharacterHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public PlayerDto Handle(SetMainCharacterCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = _playerRepository.GetPlayerOrThrow(command.PlayerId);
        var character = player.GetCharacterOrThrow(command.CharacterId);
        player.SetMainCharacter(character);
        _playerRepository.SaveChanges();
        return DtoMapper.ToDto(player);
    }
}
