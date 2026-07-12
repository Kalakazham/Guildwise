using Guildwise.Domain;

namespace Guildwise.Application.Abstractions.Persistence;

public interface IRaidEventRepository
{
    Task<RaidEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RaidEvent>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RaidEvent>> ListForGuildAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RaidEvent>> ListForRaidTeamAsync(Guid raidTeamId, CancellationToken cancellationToken = default);

    Task AddAsync(RaidEvent raidEvent, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
