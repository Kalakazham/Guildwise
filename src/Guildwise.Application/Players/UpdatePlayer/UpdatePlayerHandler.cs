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

    public async Task<PlayerDto> HandleAsync(
        UpdatePlayerCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetPlayerOrThrowAsync(command.PlayerId, cancellationToken);
        player.Rename(command.DisplayName);
        await _playerRepository.SaveChangesAsync(cancellationToken);
        return DtoMapper.ToDto(player);
    }
}
