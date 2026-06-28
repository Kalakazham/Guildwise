using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
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

    public async Task<Result<RaidTeamDto>> HandleAsync(
        AddPlayerToRaidTeamCommand command,
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

        if (guild.Members.All(member => member.PlayerId != player.Id))
        {
            return Result<RaidTeamDto>.BusinessRule("Player must be a guild member before joining a raid team.");
        }

        if (!player.MainCharacterId.HasValue)
        {
            return Result<RaidTeamDto>.BusinessRule("Player must have a main character before joining a raid team.");
        }

        if (raidTeam.Members.Any(member => member.PlayerId == player.Id))
        {
            return Result<RaidTeamDto>.Conflict("Player is already a member of this raid team.");
        }

        guild.AddPlayerToRaidTeam(raidTeam, player);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return Result<RaidTeamDto>.Success(DtoMapper.ToDto(raidTeam));
    }
}
