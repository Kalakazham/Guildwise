using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.RaidTeams;

namespace Guildwise.Application.RaidTeams.UpdateRaidTeam;

public sealed class UpdateRaidTeamHandler
{
    private readonly IGuildRepository _guildRepository;

    public UpdateRaidTeamHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task<Result<RaidTeamDto>> HandleAsync(
        UpdateRaidTeamCommand command,
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

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result<RaidTeamDto>.Validation("Raid team name is required.");
        }

        var normalizedName = command.Name.Trim();
        if (guild.RaidTeams
            .Where(existing => existing.Id != command.RaidTeamId)
            .Any(existing => string.Equals(existing.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<RaidTeamDto>.Conflict("Raid team name must be unique within the guild.");
        }

        guild.RenameRaidTeam(raidTeam, command.Name);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return Result<RaidTeamDto>.Success(DtoMapper.ToDto(raidTeam));
    }
}
