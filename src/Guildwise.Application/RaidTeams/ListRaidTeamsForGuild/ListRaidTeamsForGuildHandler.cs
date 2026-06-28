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

    public IReadOnlyList<RaidTeamDto> Handle(ListRaidTeamsForGuildQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return _guildRepository.GetGuildOrThrow(query.GuildId).RaidTeams.Select(DtoMapper.ToDto).ToList();
    }
}
