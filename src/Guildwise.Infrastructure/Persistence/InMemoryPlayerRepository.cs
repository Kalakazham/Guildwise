using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;

namespace Guildwise.Infrastructure.Persistence;

public sealed class InMemoryPlayerRepository : IPlayerRepository
{
    private readonly Dictionary<Guid, Player> _players = new();
    private readonly object _syncRoot = new();

    public Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            return Task.FromResult(_players.TryGetValue(id, out var player) ? player : null);
        }
    }

    public Task<IReadOnlyCollection<Player>> ListAsync(CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            return Task.FromResult<IReadOnlyCollection<Player>>(_players.Values.ToList());
        }
    }

    public Task AddAsync(Player player, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        lock (_syncRoot)
        {
            _players.Add(player.Id, player);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            _players.Remove(id);
        }

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
