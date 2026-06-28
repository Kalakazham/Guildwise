using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Players;

namespace Guildwise.Application.Players.UpdatePlayer;

public sealed class UpdatePlayerHandler
{
    private readonly IPlayerRepository _playerRepository;

    public UpdatePlayerHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public PlayerDto Handle(UpdatePlayerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = _playerRepository.GetPlayerOrThrow(command.PlayerId);
        player.Rename(command.DisplayName);
        _playerRepository.SaveChanges();
        return DtoMapper.ToDto(player);
    }
}
