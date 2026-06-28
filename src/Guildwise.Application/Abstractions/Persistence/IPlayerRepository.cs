using Guildwise.Domain;

namespace Guildwise.Application.Abstractions.Persistence;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Player>> ListAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Player player, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
