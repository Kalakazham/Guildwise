using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.RaidTeams;

namespace Guildwise.Application.RaidTeams.GetRaidTeam;

public sealed class GetRaidTeamHandler
{
    private readonly IGuildRepository _guildRepository;

    public GetRaidTeamHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task<RaidTeamDto?> HandleAsync(
        GetRaidTeamQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var guild = await _guildRepository.GetByIdAsync(query.GuildId, cancellationToken);
        var raidTeam = guild?.RaidTeams.FirstOrDefault(existing => existing.Id == query.RaidTeamId);

        return raidTeam is null ? null : DtoMapper.ToDto(raidTeam);
    }
}
