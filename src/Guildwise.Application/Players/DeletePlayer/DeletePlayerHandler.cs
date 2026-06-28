using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;

namespace Guildwise.Application.Players.DeletePlayer;

public sealed class DeletePlayerHandler
{
    private readonly IGuildRepository _guildRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ITransactionRunner _transactionRunner;

    public DeletePlayerHandler(
        IGuildRepository guildRepository,
        IPlayerRepository playerRepository,
        ITransactionRunner transactionRunner)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _transactionRunner = transactionRunner ?? throw new ArgumentNullException(nameof(transactionRunner));
    }

    public async Task<Result> HandleAsync(DeletePlayerCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result.NotFound($"Player '{command.PlayerId}' was not found.");
        }

        await _transactionRunner.ExecuteAsync(async ct =>
        {
            var guilds = await _guildRepository.ListAsync(ct);
            foreach (var guild in guilds)
            {
                guild.RemoveMember(command.PlayerId);
            }

            await _guildRepository.SaveChangesAsync(ct);
            await _playerRepository.RemoveAsync(command.PlayerId, ct);
        }, cancellationToken);

        return Result.Success();
    }
}
