using Guildwise.Domain;

namespace Guildwise.Application.Abstractions.Persistence;

public interface IPlayerRepository
{
    Player? GetById(Guid id);

    IReadOnlyCollection<Player> List();

    void Add(Player player);

    void Remove(Guid id);
}
