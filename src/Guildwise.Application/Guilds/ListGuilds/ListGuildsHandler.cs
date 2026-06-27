using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Guilds;

namespace Guildwise.Application.Guilds.ListGuilds;

public sealed class ListGuildsHandler
{
    private readonly IGuildRepository _guildRepository;

    public ListGuildsHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public IReadOnlyList<GuildDto> Handle(ListGuildsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return _guildRepository.List().Select(DtoMapper.ToDto).ToList();
    }
}
