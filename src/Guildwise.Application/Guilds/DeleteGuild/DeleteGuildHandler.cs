using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common.Results;

namespace Guildwise.Application.Guilds.DeleteGuild;

public sealed class DeleteGuildHandler
{
    private readonly IGuildRepository _guildRepository;

    public DeleteGuildHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task<Result> HandleAsync(DeleteGuildCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetByIdAsync(command.GuildId, cancellationToken);
        if (guild is null)
        {
            return Result.NotFound($"Guild '{command.GuildId}' was not found.");
        }

        await _guildRepository.RemoveAsync(command.GuildId, cancellationToken);
        return Result.Success();
    }
}
