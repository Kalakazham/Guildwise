using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Characters;

namespace Guildwise.Application.Characters.ListCharactersForPlayer;

public sealed class ListCharactersForPlayerHandler
{
    private readonly IPlayerRepository _playerRepository;

    public ListCharactersForPlayerHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public IReadOnlyList<CharacterDto> Handle(ListCharactersForPlayerQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var player = _playerRepository.GetPlayerOrThrow(query.PlayerId);
        return player.Characters.Select(DtoMapper.ToDto).ToList();
    }
}
