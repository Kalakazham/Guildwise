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

    public Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => PlayersWithCharacters()
            .SingleOrDefaultAsync(player => player.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Player>> ListAsync(CancellationToken cancellationToken = default)
        => await PlayersWithCharacters()
            .OrderBy(player => player.DisplayName)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Player player, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        var mainCharacterId = player.MainCharacterId;
        await _dbContext.Players.AddAsync(player, cancellationToken);

        if (mainCharacterId.HasValue)
        {
            await AddWithMainCharacterAsync(player, mainCharacterId.Value, cancellationToken);
            return;
        }

        await SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var player = await GetByIdAsync(id, cancellationToken);
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
            await SaveChangesAsync(cancellationToken);
        }

        _dbContext.Players.Remove(player);
        await SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<Player> PlayersWithCharacters()
        => _dbContext.Players
            .Include(player => player.Characters)
            .AsSplitQuery();

    private async Task AddWithMainCharacterAsync(
        Player player,
        Guid mainCharacterId,
        CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            await SaveWithMainCharacterWorkaroundAsync(player, mainCharacterId, cancellationToken);
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await SaveWithMainCharacterWorkaroundAsync(player, mainCharacterId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task SaveWithMainCharacterWorkaroundAsync(
        Player player,
        Guid mainCharacterId,
        CancellationToken cancellationToken)
    {
        var mainCharacterProperty = _dbContext.Entry(player)
            .Property(existing => existing.MainCharacterId);
        mainCharacterProperty.CurrentValue = null;
        await SaveChangesAsync(cancellationToken);

        mainCharacterProperty.CurrentValue = mainCharacterId;
        mainCharacterProperty.IsModified = true;
        await SaveChangesAsync(cancellationToken);
    }
}
