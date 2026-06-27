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

    public void Handle(DeleteRaidTeamCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = _guildRepository.GetGuildOrThrow(command.GuildId);
        guild.GetRaidTeamOrThrow(command.RaidTeamId);
        guild.RemoveRaidTeam(command.RaidTeamId);
    }
}
