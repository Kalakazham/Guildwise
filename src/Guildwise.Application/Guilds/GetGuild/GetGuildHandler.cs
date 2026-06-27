using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Guilds;

namespace Guildwise.Application.Guilds.GetGuild;

public sealed class GetGuildHandler
{
    private readonly IGuildRepository _guildRepository;

    public GetGuildHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public GuildDto? Handle(GetGuildQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return _guildRepository.GetById(query.GuildId) is { } guild
            ? DtoMapper.ToDto(guild)
            : null;
    }
}
