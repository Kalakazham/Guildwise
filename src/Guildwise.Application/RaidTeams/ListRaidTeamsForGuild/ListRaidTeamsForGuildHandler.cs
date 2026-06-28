using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.RaidTeams;

namespace Guildwise.Application.RaidTeams.ListRaidTeamsForGuild;

public sealed class ListRaidTeamsForGuildHandler
{
    private readonly IGuildRepository _guildRepository;

    public ListRaidTeamsForGuildHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task<IReadOnlyList<RaidTeamDto>> HandleAsync(
        ListRaidTeamsForGuildQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var guild = await _guildRepository.GetGuildOrThrowAsync(query.GuildId, cancellationToken);
        return guild.RaidTeams.Select(DtoMapper.ToDto).ToList();
    }
}
