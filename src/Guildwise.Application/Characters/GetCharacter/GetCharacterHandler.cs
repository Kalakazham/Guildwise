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

    public CharacterDto? Handle(GetCharacterQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var player = _playerRepository.GetById(query.PlayerId);
        var character = player?.Characters.FirstOrDefault(existing => existing.Id == query.CharacterId);

        return character is null ? null : DtoMapper.ToDto(character);
    }
}
