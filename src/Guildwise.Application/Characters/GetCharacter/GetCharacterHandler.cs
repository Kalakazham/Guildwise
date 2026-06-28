using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Characters;

namespace Guildwise.Application.Characters.GetCharacter;

public sealed class GetCharacterHandler
{
    private readonly IPlayerRepository _playerRepository;

    public GetCharacterHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<CharacterDto?> HandleAsync(
        GetCharacterQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var player = await _playerRepository.GetByIdAsync(query.PlayerId, cancellationToken);
        var character = player?.Characters.FirstOrDefault(existing => existing.Id == query.CharacterId);

        return character is null ? null : DtoMapper.ToDto(character);
    }
}
