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

    public async Task<IReadOnlyList<GuildDto>> HandleAsync(
        ListGuildsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var guilds = await _guildRepository.ListAsync(cancellationToken);
        return guilds.Select(DtoMapper.ToDto).ToList();
    }
}
