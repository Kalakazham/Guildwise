using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Guilds;

namespace Guildwise.Application.Guilds.UpdateGuild;

public sealed class UpdateGuildHandler
{
    private readonly IGuildRepository _guildRepository;

    public UpdateGuildHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task<GuildDto> HandleAsync(
        UpdateGuildCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetGuildOrThrowAsync(command.GuildId, cancellationToken);
        guild.Update(command.Name, command.Region, command.Realm);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return DtoMapper.ToDto(guild);
    }
}
