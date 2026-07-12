using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;

namespace Guildwise.Infrastructure.Persistence;

public sealed class EfRaidEventRepository : IRaidEventRepository
{
    private readonly GuildwiseDbContext _dbContext;

    public EfRaidEventRepository(GuildwiseDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<RaidEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.RaidEvents
            .SingleOrDefaultAsync(raidEvent => raidEvent.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<RaidEvent>> ListAsync(CancellationToken cancellationToken = default)
        => await OrderedRaidEvents()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<RaidEvent>> ListForGuildAsync(
        Guid guildId,
        CancellationToken cancellationToken = default)
        => await OrderedRaidEvents()
            .Where(raidEvent => raidEvent.GuildId == guildId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<RaidEvent>> ListForRaidTeamAsync(
        Guid raidTeamId,
        CancellationToken cancellationToken = default)
        => await OrderedRaidEvents()
            .Where(raidEvent => raidEvent.RaidTeamId == raidTeamId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(RaidEvent raidEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(raidEvent);

        await _dbContext.RaidEvents.AddAsync(raidEvent, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    private IOrderedQueryable<RaidEvent> OrderedRaidEvents()
        => _dbContext.RaidEvents
            .OrderBy(raidEvent => raidEvent.StartTime)
            .ThenBy(raidEvent => raidEvent.Title)
            .ThenBy(raidEvent => raidEvent.Id);
}
