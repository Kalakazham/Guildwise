using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;

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

    public async Task<Result> HandleAsync(DeletePlayerCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result.NotFound($"Player '{command.PlayerId}' was not found.");
        }

        var guilds = await _guildRepository.ListAsync(cancellationToken);
        foreach (var guild in guilds)
        {
            guild.RemoveMember(player.Id);
        }

        await _guildRepository.SaveChangesAsync(cancellationToken);
        await _playerRepository.RemoveAsync(command.PlayerId, cancellationToken);
        return Result.Success();
    }
}
