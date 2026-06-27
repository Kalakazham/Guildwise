using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Guilds;
using Guildwise.Domain;

namespace Guildwise.Application.Guilds.CreateGuild;

public sealed class CreateGuildHandler
{
    private readonly IGuildRepository _guildRepository;

    public CreateGuildHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public GuildDto Handle(CreateGuildCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = Guild.Create(command.Name, command.Region, command.Realm);
        _guildRepository.Add(guild);
        return DtoMapper.ToDto(guild);
    }
}
