using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Characters;

namespace Guildwise.Application.Characters.CreateCharacter;

public sealed class CreateCharacterHandler
{
    private readonly IPlayerRepository _playerRepository;

    public CreateCharacterHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<CharacterDto> HandleAsync(
        CreateCharacterCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetPlayerOrThrowAsync(command.PlayerId, cancellationToken);
        var character = player.AddCharacter(
            command.Name,
            command.Region,
            command.Realm,
            command.CharacterClass,
            command.Specialization,
            command.Role);
        await _playerRepository.SaveChangesAsync(cancellationToken);

        return DtoMapper.ToDto(character);
    }
}
