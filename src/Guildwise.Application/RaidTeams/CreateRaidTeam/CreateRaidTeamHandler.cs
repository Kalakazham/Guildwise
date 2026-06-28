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

    public async Task<RaidTeamDto> HandleAsync(
        CreateRaidTeamCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetGuildOrThrowAsync(command.GuildId, cancellationToken);
        var raidTeam = guild.CreateRaidTeam(command.Name);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return DtoMapper.ToDto(raidTeam);
    }
}
