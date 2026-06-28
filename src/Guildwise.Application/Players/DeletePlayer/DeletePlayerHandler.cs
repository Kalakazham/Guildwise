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

    public async Task HandleAsync(DeletePlayerCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetPlayerOrThrowAsync(command.PlayerId, cancellationToken);

        var guilds = await _guildRepository.ListAsync(cancellationToken);
        foreach (var guild in guilds)
        {
            guild.RemoveMember(player.Id);
        }

        await _guildRepository.SaveChangesAsync(cancellationToken);
        await _playerRepository.RemoveAsync(command.PlayerId, cancellationToken);
    }
}
