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

    public async Task<IReadOnlyList<CharacterDto>> HandleAsync(
        ListCharactersQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var players = await _playerRepository.ListAsync(cancellationToken);
        return players.SelectMany(player => player.Characters.Select(DtoMapper.ToDto)).ToList();
    }
}
