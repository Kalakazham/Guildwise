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

    public async Task<RaidTeamDto> HandleAsync(
        UpdateRaidTeamCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetGuildOrThrowAsync(command.GuildId, cancellationToken);
        var raidTeam = guild.GetRaidTeamOrThrow(command.RaidTeamId);
        guild.RenameRaidTeam(raidTeam, command.Name);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return DtoMapper.ToDto(raidTeam);
    }
}
