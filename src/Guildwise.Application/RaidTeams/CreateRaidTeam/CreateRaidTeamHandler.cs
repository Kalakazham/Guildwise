using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.RaidTeams;

namespace Guildwise.Application.RaidTeams.CreateRaidTeam;

public sealed class CreateRaidTeamHandler
{
    private readonly IGuildRepository _guildRepository;

    public CreateRaidTeamHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task<Result<RaidTeamDto>> HandleAsync(
        CreateRaidTeamCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetByIdAsync(command.GuildId, cancellationToken);
        if (guild is null)
        {
            return Result<RaidTeamDto>.NotFound($"Guild '{command.GuildId}' was not found.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result<RaidTeamDto>.Validation("Raid team name is required.");
        }

        var normalizedName = command.Name.Trim();
        if (guild.RaidTeams.Any(raidTeam => string.Equals(raidTeam.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<RaidTeamDto>.Conflict("Raid team name must be unique within the guild.");
        }

        var raidTeam = guild.CreateRaidTeam(command.Name);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return Result<RaidTeamDto>.Success(DtoMapper.ToDto(raidTeam));
    }
}
