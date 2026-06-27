using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.RaidTeams;

namespace Guildwise.Application.RaidTeams.CreateRaidTeam;

public sealed class CreateRaidTeamHandler
{
    private readonly IGuildRepository _guildRepository;

    public CreateRaidTeamHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public RaidTeamDto Handle(CreateRaidTeamCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = _guildRepository.GetGuildOrThrow(command.GuildId);
        var raidTeam = guild.CreateRaidTeam(command.Name);
        return DtoMapper.ToDto(raidTeam);
    }
}
