using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;

namespace Guildwise.Infrastructure.Persistence;

public sealed class EfGuildRepository : IGuildRepository
{
    private readonly GuildwiseDbContext _dbContext;

    public EfGuildRepository(GuildwiseDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<Guild?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => GuildsWithRoster()
            .SingleOrDefaultAsync(guild => guild.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Guild>> ListAsync(CancellationToken cancellationToken = default)
        => await GuildsWithRoster()
            .OrderBy(guild => guild.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Guild guild, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(guild);

        await _dbContext.Guilds.AddAsync(guild, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var guild = await GetByIdAsync(id, cancellationToken);
        if (guild is null)
        {
            return;
        }

        _dbContext.Guilds.Remove(guild);
        await SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<Guild> GuildsWithRoster()
        => _dbContext.Guilds
            .Include(guild => guild.Members)
            .Include(guild => guild.RaidTeams)
            .ThenInclude(raidTeam => raidTeam.Members)
            .AsSplitQuery();
}
