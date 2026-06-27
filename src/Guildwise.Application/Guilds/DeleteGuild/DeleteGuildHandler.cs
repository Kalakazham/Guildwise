using Guildwise.Application.Abstractions.Persistence;

namespace Guildwise.Application.Guilds.DeleteGuild;

public sealed class DeleteGuildHandler
{
    private readonly IGuildRepository _guildRepository;

    public DeleteGuildHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public void Handle(DeleteGuildCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        _guildRepository.Remove(command.GuildId);
    }
}
