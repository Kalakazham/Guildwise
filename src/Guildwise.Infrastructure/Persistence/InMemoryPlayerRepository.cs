using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;

namespace Guildwise.Infrastructure.Persistence;

public sealed class InMemoryPlayerRepository : IPlayerRepository
{
    private readonly Dictionary<Guid, Player> _players = new();
    private readonly object _syncRoot = new();

    public Player? GetById(Guid id)
    {
        lock (_syncRoot)
        {
            return _players.TryGetValue(id, out var player) ? player : null;
        }
    }

    public IReadOnlyCollection<Player> List()
    {
        lock (_syncRoot)
        {
            return _players.Values.ToList();
        }
    }

    public void Add(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        lock (_syncRoot)
        {
            _players.Add(player.Id, player);
        }
    }

    public void Remove(Guid id)
    {
        lock (_syncRoot)
        {
            _players.Remove(id);
        }
    }
}
