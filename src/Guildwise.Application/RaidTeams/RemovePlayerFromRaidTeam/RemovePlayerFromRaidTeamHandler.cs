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

    public RaidTeamDto Handle(RemovePlayerFromRaidTeamCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = _guildRepository.GetGuildOrThrow(command.GuildId);
        var raidTeam = guild.GetRaidTeamOrThrow(command.RaidTeamId);
        guild.RemovePlayerFromRaidTeam(raidTeam, command.PlayerId);
        return DtoMapper.ToDto(raidTeam);
    }
}
