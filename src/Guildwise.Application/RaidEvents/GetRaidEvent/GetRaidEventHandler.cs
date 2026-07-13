using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.RaidEvents;

namespace Guildwise.Application.RaidEvents.GetRaidEvent;

public sealed class GetRaidEventHandler
{
    private readonly IRaidEventRepository _raidEventRepository;

    public GetRaidEventHandler(IRaidEventRepository raidEventRepository)
    {
        _raidEventRepository = raidEventRepository ?? throw new ArgumentNullException(nameof(raidEventRepository));
    }

    public async Task<RaidEventDto?> HandleAsync(
        GetRaidEventQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await _raidEventRepository.GetByIdAsync(query.RaidEventId, cancellationToken) is { } raidEvent
            ? DtoMapper.ToDto(raidEvent)
            : null;
    }
}
