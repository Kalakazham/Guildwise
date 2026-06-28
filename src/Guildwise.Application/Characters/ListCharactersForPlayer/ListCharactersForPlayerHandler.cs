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

    public async Task<IReadOnlyList<CharacterDto>> HandleAsync(
        ListCharactersForPlayerQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var player = await _playerRepository.GetPlayerOrThrowAsync(query.PlayerId, cancellationToken);
        return player.Characters.Select(DtoMapper.ToDto).ToList();
    }
}
