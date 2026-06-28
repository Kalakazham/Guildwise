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

    public async Task<RaidTeamDto> HandleAsync(
        AddPlayerToRaidTeamCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetGuildOrThrowAsync(command.GuildId, cancellationToken);
        var raidTeam = guild.GetRaidTeamOrThrow(command.RaidTeamId);
        var player = await _playerRepository.GetPlayerOrThrowAsync(command.PlayerId, cancellationToken);
        guild.AddPlayerToRaidTeam(raidTeam, player);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return DtoMapper.ToDto(raidTeam);
    }
}
