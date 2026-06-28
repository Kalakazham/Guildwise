using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Players;

namespace Guildwise.Application.Players.ListPlayers;

public sealed class ListPlayersHandler
{
    private readonly IPlayerRepository _playerRepository;

    public ListPlayersHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<IReadOnlyList<PlayerDto>> HandleAsync(
        ListPlayersQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var players = await _playerRepository.ListAsync(cancellationToken);
        return players.Select(DtoMapper.ToDto).ToList();
    }
}
