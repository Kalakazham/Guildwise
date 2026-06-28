using Guildwise.Application.Abstractions.Persistence;

namespace Guildwise.Application.Guilds.DeleteGuild;

public sealed class DeleteGuildHandler
{
    private readonly IGuildRepository _guildRepository;

    public DeleteGuildHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task HandleAsync(DeleteGuildCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        await _guildRepository.RemoveAsync(command.GuildId, cancellationToken);
    }
}
