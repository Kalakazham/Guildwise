using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.RaidTeams;

namespace Guildwise.Application.RaidTeams.RemovePlayerFromRaidTeam;

public sealed class RemovePlayerFromRaidTeamHandler
{
    private readonly IGuildRepository _guildRepository;
    private readonly IPlayerRepository _playerRepository;

    public RemovePlayerFromRaidTeamHandler(IGuildRepository guildRepository, IPlayerRepository playerRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<Result<RaidTeamDto>> HandleAsync(
        RemovePlayerFromRaidTeamCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetByIdAsync(command.GuildId, cancellationToken);
        if (guild is null)
        {
            return Result<RaidTeamDto>.NotFound($"Guild '{command.GuildId}' was not found.");
        }

        var raidTeam = guild.RaidTeams.FirstOrDefault(existing => existing.Id == command.RaidTeamId);
        if (raidTeam is null)
        {
            return Result<RaidTeamDto>.NotFound($"RaidTeam '{command.RaidTeamId}' was not found.");
        }

        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result<RaidTeamDto>.NotFound($"Player '{command.PlayerId}' was not found.");
        }

        if (raidTeam.Members.All(member => member.PlayerId != player.Id))
        {
            return Result<RaidTeamDto>.NotFound($"Player '{command.PlayerId}' was not found in this raid team.");
        }

        guild.RemovePlayerFromRaidTeam(raidTeam, command.PlayerId);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return Result<RaidTeamDto>.Success(DtoMapper.ToDto(raidTeam));
    }
}
