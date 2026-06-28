using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;

namespace Guildwise.Infrastructure.Persistence;

public sealed class EfPlayerRepository : IPlayerRepository
{
    private readonly GuildwiseDbContext _dbContext;

    public EfPlayerRepository(GuildwiseDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Player? GetById(Guid id)
        => PlayersWithCharacters()
            .SingleOrDefault(player => player.Id == id);

    public IReadOnlyCollection<Player> List()
        => PlayersWithCharacters()
            .OrderBy(player => player.DisplayName)
            .ToList();

    public void Add(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var mainCharacterId = player.MainCharacterId;
        _dbContext.Players.Add(player);

        if (mainCharacterId.HasValue)
        {
            var mainCharacterProperty = _dbContext.Entry(player)
                .Property(existing => existing.MainCharacterId);
            mainCharacterProperty.CurrentValue = null;
            SaveChanges();

            mainCharacterProperty.CurrentValue = mainCharacterId;
            mainCharacterProperty.IsModified = true;
            SaveChanges();
            return;
        }

        SaveChanges();
    }

    public void Remove(Guid id)
    {
        var player = GetById(id);
        if (player is null)
        {
            return;
        }

        if (player.MainCharacterId.HasValue)
        {
            var mainCharacterProperty = _dbContext.Entry(player)
                .Property(existing => existing.MainCharacterId);
            mainCharacterProperty.CurrentValue = null;
            mainCharacterProperty.IsModified = true;
            SaveChanges();
        }

        _dbContext.Players.Remove(player);
        SaveChanges();
    }

    public void SaveChanges()
        => _dbContext.SaveChanges();

    private IQueryable<Player> PlayersWithCharacters()
        => _dbContext.Players
            .Include(player => player.Characters)
            .AsSplitQuery();
}
