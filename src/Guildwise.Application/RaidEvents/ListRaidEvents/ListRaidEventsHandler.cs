using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.RaidEvents;
using Guildwise.Domain;

namespace Guildwise.Application.RaidEvents.ListRaidEvents;

public sealed class ListRaidEventsHandler
{
    private readonly IRaidEventRepository _raidEventRepository;

    public ListRaidEventsHandler(IRaidEventRepository raidEventRepository)
    {
        _raidEventRepository = raidEventRepository ?? throw new ArgumentNullException(nameof(raidEventRepository));
    }

    public async Task<IReadOnlyList<RaidEventDto>> HandleAsync(
        ListRaidEventsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        IReadOnlyCollection<RaidEvent> raidEvents;
        if (query.RaidTeamId.HasValue)
        {
            raidEvents = await _raidEventRepository.ListForRaidTeamAsync(query.RaidTeamId.Value, cancellationToken);
        }
        else if (query.GuildId.HasValue)
        {
            raidEvents = await _raidEventRepository.ListForGuildAsync(query.GuildId.Value, cancellationToken);
        }
        else
        {
            raidEvents = await _raidEventRepository.ListAsync(cancellationToken);
        }

        if (query.GuildId.HasValue)
        {
            raidEvents = raidEvents
                .Where(raidEvent => raidEvent.GuildId == query.GuildId.Value)
                .ToList();
        }

        return raidEvents.Select(DtoMapper.ToDto).ToList();
    }
}
