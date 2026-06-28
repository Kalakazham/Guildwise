using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Characters;

namespace Guildwise.Application.Characters.UpdateCharacter;

public sealed class UpdateCharacterHandler
{
    private readonly IPlayerRepository _playerRepository;

    public UpdateCharacterHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<CharacterDto> HandleAsync(
        UpdateCharacterCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetPlayerOrThrowAsync(command.PlayerId, cancellationToken);
        player.UpdateCharacter(
            command.CharacterId,
            command.Name,
            command.Region,
            command.Realm,
            command.CharacterClass,
            command.Specialization,
            command.Role);
        await _playerRepository.SaveChangesAsync(cancellationToken);

        return DtoMapper.ToDto(player.GetCharacterOrThrow(command.CharacterId));
    }
}
