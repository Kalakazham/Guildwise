using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;

namespace Guildwise.Application.Characters.DeleteCharacter;

public sealed class DeleteCharacterHandler
{
    private readonly IPlayerRepository _playerRepository;

    public DeleteCharacterHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task HandleAsync(DeleteCharacterCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetPlayerOrThrowAsync(command.PlayerId, cancellationToken);
        player.GetCharacterOrThrow(command.CharacterId);
        player.RemoveCharacter(command.CharacterId);
        await _playerRepository.SaveChangesAsync(cancellationToken);
    }
}
