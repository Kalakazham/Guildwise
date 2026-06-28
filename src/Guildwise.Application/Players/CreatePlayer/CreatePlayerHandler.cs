using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Players;
using Guildwise.Domain;

namespace Guildwise.Application.Players.CreatePlayer;

public sealed class CreatePlayerHandler
{
    private readonly IPlayerRepository _playerRepository;

    public CreatePlayerHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public PlayerDto Handle(CreatePlayerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = Player.Create(command.DisplayName);
        _playerRepository.Add(player);
        return DtoMapper.ToDto(player);
    }
}
