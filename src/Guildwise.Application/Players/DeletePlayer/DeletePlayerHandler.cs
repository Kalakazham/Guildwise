using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;

namespace Guildwise.Application.Players.DeletePlayer;

public sealed class DeletePlayerHandler
{
    private readonly IGuildRepository _guildRepository;
    private readonly IPlayerRepository _playerRepository;

    public DeletePlayerHandler(IGuildRepository guildRepository, IPlayerRepository playerRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public void Handle(DeletePlayerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = _playerRepository.GetPlayerOrThrow(command.PlayerId);

        foreach (var guild in _guildRepository.List())
        {
            guild.RemoveMember(player.Id);
        }

        _playerRepository.Remove(command.PlayerId);
    }
}
