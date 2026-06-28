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

    public Guild? GetById(Guid id)
        => GuildsWithRoster()
            .SingleOrDefault(guild => guild.Id == id);

    public IReadOnlyCollection<Guild> List()
        => GuildsWithRoster()
            .OrderBy(guild => guild.Name)
            .ToList();

    public void Add(Guild guild)
    {
        ArgumentNullException.ThrowIfNull(guild);

        _dbContext.Guilds.Add(guild);
        SaveChanges();
    }

    public void Remove(Guid id)
    {
        var guild = GetById(id);
        if (guild is null)
        {
            return;
        }

        _dbContext.Guilds.Remove(guild);
        SaveChanges();
    }

    public void SaveChanges()
        => _dbContext.SaveChanges();

    private IQueryable<Guild> GuildsWithRoster()
        => _dbContext.Guilds
            .Include(guild => guild.Members)
            .Include(guild => guild.RaidTeams)
            .ThenInclude(raidTeam => raidTeam.Members)
            .AsSplitQuery();
}
