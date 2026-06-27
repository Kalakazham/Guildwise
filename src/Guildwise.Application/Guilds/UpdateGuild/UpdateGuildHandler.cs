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

    public GuildDto Handle(UpdateGuildCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = _guildRepository.GetGuildOrThrow(command.GuildId);
        guild.Update(command.Name, command.Region, command.Realm);
        return DtoMapper.ToDto(guild);
    }
}
