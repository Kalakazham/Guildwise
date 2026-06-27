using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Characters;

namespace Guildwise.Application.Characters.ListCharacters;

public sealed class ListCharactersHandler
{
    private readonly IPlayerRepository _playerRepository;

    public ListCharactersHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public IReadOnlyList<CharacterDto> Handle(ListCharactersQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return _playerRepository.List().SelectMany(player => player.Characters.Select(DtoMapper.ToDto)).ToList();
    }
}
