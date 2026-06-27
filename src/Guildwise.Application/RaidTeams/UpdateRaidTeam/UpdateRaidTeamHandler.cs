using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.RaidTeams;

namespace Guildwise.Application.RaidTeams.UpdateRaidTeam;

public sealed class UpdateRaidTeamHandler
{
    private readonly IGuildRepository _guildRepository;

    public UpdateRaidTeamHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public RaidTeamDto Handle(UpdateRaidTeamCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = _guildRepository.GetGuildOrThrow(command.GuildId);
        var raidTeam = guild.GetRaidTeamOrThrow(command.RaidTeamId);
        guild.RenameRaidTeam(raidTeam, command.Name);
        return DtoMapper.ToDto(raidTeam);
    }
}
