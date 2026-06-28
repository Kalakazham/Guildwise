using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;

namespace Guildwise.Application.RaidTeams.DeleteRaidTeam;

public sealed class DeleteRaidTeamHandler
{
    private readonly IGuildRepository _guildRepository;

    public DeleteRaidTeamHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task<Result> HandleAsync(DeleteRaidTeamCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetByIdAsync(command.GuildId, cancellationToken);
        if (guild is null)
        {
            return Result.NotFound($"Guild '{command.GuildId}' was not found.");
        }

        if (guild.RaidTeams.All(raidTeam => raidTeam.Id != command.RaidTeamId))
        {
            return Result.NotFound($"RaidTeam '{command.RaidTeamId}' was not found.");
        }

        guild.RemoveRaidTeam(command.RaidTeamId);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
