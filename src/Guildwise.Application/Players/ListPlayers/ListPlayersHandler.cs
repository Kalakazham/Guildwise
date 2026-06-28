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

    public IReadOnlyList<PlayerDto> Handle(ListPlayersQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return _playerRepository.List().Select(DtoMapper.ToDto).ToList();
    }
}
