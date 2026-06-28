using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;

namespace Guildwise.Infrastructure.Persistence;

public sealed class InMemoryGuildRepository : IGuildRepository
{
    private readonly Dictionary<Guid, Guild> _guilds = new();
    private readonly object _syncRoot = new();

    public Task<Guild?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            return Task.FromResult(_guilds.TryGetValue(id, out var guild) ? guild : null);
        }
    }

    public Task<IReadOnlyCollection<Guild>> ListAsync(CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            return Task.FromResult<IReadOnlyCollection<Guild>>(_guilds.Values.ToList());
        }
    }

    public Task AddAsync(Guild guild, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(guild);

        lock (_syncRoot)
        {
            _guilds.Add(guild.Id, guild);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_syncRoot)
        {
            _guilds.Remove(id);
        }

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
