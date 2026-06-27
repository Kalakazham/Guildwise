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

    public void Handle(DeleteCharacterCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = _playerRepository.GetPlayerOrThrow(command.PlayerId);
        player.GetCharacterOrThrow(command.CharacterId);
        player.RemoveCharacter(command.CharacterId);
    }
}
