using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.RaidTeams;

namespace Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;

public sealed class AddPlayerToRaidTeamHandler
{
    private readonly IGuildRepository _guildRepository;
    private readonly IPlayerRepository _playerRepository;

    public AddPlayerToRaidTeamHandler(IGuildRepository guildRepository, IPlayerRepository playerRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public RaidTeamDto Handle(AddPlayerToRaidTeamCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = _guildRepository.GetGuildOrThrow(command.GuildId);
        var raidTeam = guild.GetRaidTeamOrThrow(command.RaidTeamId);
        var player = _playerRepository.GetPlayerOrThrow(command.PlayerId);
        guild.AddPlayerToRaidTeam(raidTeam, player);
        return DtoMapper.ToDto(raidTeam);
    }
}
