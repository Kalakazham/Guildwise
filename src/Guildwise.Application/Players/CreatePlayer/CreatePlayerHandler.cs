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

    public async Task<PlayerDto> HandleAsync(
        CreatePlayerCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = Player.Create(command.DisplayName);
        await _playerRepository.AddAsync(player, cancellationToken);
        return DtoMapper.ToDto(player);
    }
}
