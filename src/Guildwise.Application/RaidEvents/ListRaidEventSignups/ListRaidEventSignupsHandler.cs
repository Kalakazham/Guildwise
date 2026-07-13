using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.RaidEvents;

namespace Guildwise.Application.RaidEvents.ListRaidEventSignups;

public sealed class ListRaidEventSignupsHandler
{
    private readonly IGuildRepository _guildRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IRaidEventRepository _raidEventRepository;

    public ListRaidEventSignupsHandler(
        IGuildRepository guildRepository,
        IPlayerRepository playerRepository,
        IRaidEventRepository raidEventRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _raidEventRepository = raidEventRepository ?? throw new ArgumentNullException(nameof(raidEventRepository));
    }

    public async Task<IReadOnlyList<RaidEventSignupDto>> HandleAsync(
        ListRaidEventSignupsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.RaidEventId == Guid.Empty)
        {
            return [];
        }

        var raidEvent = await _raidEventRepository.GetByIdAsync(query.RaidEventId, cancellationToken);
        if (raidEvent is null || raidEvent.Signups.Count == 0)
        {
            return [];
        }

        var players = await _playerRepository.ListAsync(cancellationToken);
        var playersById = players.ToDictionary(player => player.Id);
        var guild = await _guildRepository.GetByIdAsync(raidEvent.GuildId, cancellationToken);

        return raidEvent.Signups
            .OrderBy(signup => playersById.TryGetValue(signup.PlayerId, out var player) ? player.DisplayName : string.Empty)
            .ThenBy(signup => signup.PlayerId)
            .Select(signup =>
            {
                if (!playersById.TryGetValue(signup.PlayerId, out var player))
                {
                    return null;
                }

                var guildMember = guild?.Members.FirstOrDefault(member => member.PlayerId == signup.PlayerId);
                return DtoMapper.ToDto(raidEvent, signup, player, guildMember);
            })
            .OfType<RaidEventSignupDto>()
            .ToList();
    }
}
