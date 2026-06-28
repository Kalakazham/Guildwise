using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;

namespace Guildwise.Application.Characters.DeleteCharacter;

public sealed class DeleteCharacterHandler
{
    private readonly IPlayerRepository _playerRepository;

    public DeleteCharacterHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<Result> HandleAsync(DeleteCharacterCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result.NotFound($"Player '{command.PlayerId}' was not found.");
        }

        if (player.Characters.All(character => character.Id != command.CharacterId))
        {
            return Result.NotFound($"Character '{command.CharacterId}' was not found.");
        }

        player.RemoveCharacter(command.CharacterId);
        await _playerRepository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
