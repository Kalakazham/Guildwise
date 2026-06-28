using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.Players;

namespace Guildwise.Application.Players.UpdatePlayer;

public sealed class UpdatePlayerHandler
{
    private readonly IPlayerRepository _playerRepository;

    public UpdatePlayerHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<Result<PlayerDto>> HandleAsync(
        UpdatePlayerCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result<PlayerDto>.NotFound($"Player '{command.PlayerId}' was not found.");
        }

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            return Result<PlayerDto>.Validation("Player display name is required.");
        }

        player.Rename(command.DisplayName);
        await _playerRepository.SaveChangesAsync(cancellationToken);
        return Result<PlayerDto>.Success(DtoMapper.ToDto(player));
    }
}
