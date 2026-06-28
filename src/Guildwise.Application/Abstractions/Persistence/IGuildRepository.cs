using Guildwise.Domain;

namespace Guildwise.Application.Abstractions.Persistence;

public interface IGuildRepository
{
    Guild? GetById(Guid id);

    IReadOnlyCollection<Guild> List();

    void Add(Guild guild);

    void Remove(Guid id);

    void SaveChanges();
}
