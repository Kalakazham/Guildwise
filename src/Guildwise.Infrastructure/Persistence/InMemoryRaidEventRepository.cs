using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;

namespace Guildwise.Infrastructure.Persistence;

public sealed class InMemoryRaidEventRepository : IRaidEventRepository
{
    private readonly Dictionary<Guid, RaidEvent> _raidEvents = new();
    private readonly object _syncRoot = new();

    public Task<RaidEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            return Task.FromResult(_raidEvents.TryGetValue(id, out var raidEvent) ? raidEvent : null);
        }
    }

    public Task<IReadOnlyCollection<RaidEvent>> ListAsync(CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            return Task.FromResult<IReadOnlyCollection<RaidEvent>>(OrderedRaidEvents().ToList());
        }
    }

    public Task<IReadOnlyCollection<RaidEvent>> ListForGuildAsync(
        Guid guildId,
        CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            return Task.FromResult<IReadOnlyCollection<RaidEvent>>(OrderedRaidEvents()
                .Where(raidEvent => raidEvent.GuildId == guildId)
                .ToList());
        }
    }

    public Task<IReadOnlyCollection<RaidEvent>> ListForRaidTeamAsync(
        Guid raidTeamId,
        CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            return Task.FromResult<IReadOnlyCollection<RaidEvent>>(OrderedRaidEvents()
                .Where(raidEvent => raidEvent.RaidTeamId == raidTeamId)
                .ToList());
        }
    }

    public Task AddAsync(RaidEvent raidEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(raidEvent);

        lock (_syncRoot)
        {
            _raidEvents.Add(raidEvent.Id, raidEvent);
        }

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    private IEnumerable<RaidEvent> OrderedRaidEvents()
        => _raidEvents.Values
            .OrderBy(raidEvent => raidEvent.StartTime)
            .ThenBy(raidEvent => raidEvent.Title)
            .ThenBy(raidEvent => raidEvent.Id);
}
