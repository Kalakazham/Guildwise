using Guildwise.Domain;

namespace Guildwise.Application.Abstractions.Persistence;

public interface IGuildRepository
{
    Task<Guild?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Guild>> ListAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Guild guild, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
