using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;

namespace Guildwise.Application.RaidTeams.DeleteRaidTeam;

public sealed class DeleteRaidTeamHandler
{
    private readonly IGuildRepository _guildRepository;

    public DeleteRaidTeamHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task HandleAsync(DeleteRaidTeamCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetGuildOrThrowAsync(command.GuildId, cancellationToken);
        guild.GetRaidTeamOrThrow(command.RaidTeamId);
        guild.RemoveRaidTeam(command.RaidTeamId);
        await _guildRepository.SaveChangesAsync(cancellationToken);
    }
}
