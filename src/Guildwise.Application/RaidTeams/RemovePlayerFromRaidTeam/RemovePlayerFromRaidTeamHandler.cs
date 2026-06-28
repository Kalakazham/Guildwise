using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.RaidTeams;

namespace Guildwise.Application.RaidTeams.RemovePlayerFromRaidTeam;

public sealed class RemovePlayerFromRaidTeamHandler
{
    private readonly IGuildRepository _guildRepository;

    public RemovePlayerFromRaidTeamHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task<RaidTeamDto> HandleAsync(
        RemovePlayerFromRaidTeamCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetGuildOrThrowAsync(command.GuildId, cancellationToken);
        var raidTeam = guild.GetRaidTeamOrThrow(command.RaidTeamId);
        guild.RemovePlayerFromRaidTeam(raidTeam, command.PlayerId);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return DtoMapper.ToDto(raidTeam);
    }
}
