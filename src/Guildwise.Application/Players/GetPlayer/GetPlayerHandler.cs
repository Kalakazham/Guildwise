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

    public PlayerDto? Handle(GetPlayerQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return _playerRepository.GetById(query.PlayerId) is { } player
            ? DtoMapper.ToDto(player)
            : null;
    }
}
