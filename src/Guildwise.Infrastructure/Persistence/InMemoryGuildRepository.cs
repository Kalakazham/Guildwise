using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;

namespace Guildwise.Infrastructure.Persistence;

public sealed class InMemoryGuildRepository : IGuildRepository
{
    private readonly Dictionary<Guid, Guild> _guilds = new();
    private readonly object _syncRoot = new();

    public Guild? GetById(Guid id)
    {
        lock (_syncRoot)
        {
            return _guilds.TryGetValue(id, out var guild) ? guild : null;
        }
    }

    public IReadOnlyCollection<Guild> List()
    {
        lock (_syncRoot)
        {
            return _guilds.Values.ToList();
        }
    }

    public void Add(Guild guild)
    {
        ArgumentNullException.ThrowIfNull(guild);

        lock (_syncRoot)
        {
            _guilds.Add(guild.Id, guild);
        }
    }

    public void Remove(Guid id)
    {
        lock (_syncRoot)
        {
            _guilds.Remove(id);
        }
    }
}
