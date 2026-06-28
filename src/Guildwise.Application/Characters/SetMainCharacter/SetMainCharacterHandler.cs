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

    public async Task<PlayerDto> HandleAsync(
        SetMainCharacterCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetPlayerOrThrowAsync(command.PlayerId, cancellationToken);
        var character = player.GetCharacterOrThrow(command.CharacterId);
        player.SetMainCharacter(character);
        await _playerRepository.SaveChangesAsync(cancellationToken);
        return DtoMapper.ToDto(player);
    }
}
