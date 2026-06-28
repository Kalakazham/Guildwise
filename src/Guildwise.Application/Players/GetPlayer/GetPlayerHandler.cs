using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Players;

namespace Guildwise.Application.Players.GetPlayer;

public sealed class GetPlayerHandler
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<PlayerDto?> HandleAsync(
        GetPlayerQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await _playerRepository.GetByIdAsync(query.PlayerId, cancellationToken) is { } player
            ? DtoMapper.ToDto(player)
            : null;
    }
}
