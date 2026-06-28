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

    public async Task<GuildDto?> HandleAsync(GetGuildQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await _guildRepository.GetByIdAsync(query.GuildId, cancellationToken) is { } guild
            ? DtoMapper.ToDto(guild)
            : null;
    }
}
