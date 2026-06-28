using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
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

    public async Task<Result<GuildDto>> HandleAsync(
        CreateGuildCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result<GuildDto>.Validation("Guild name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Region))
        {
            return Result<GuildDto>.Validation("Guild region is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Realm))
        {
            return Result<GuildDto>.Validation("Guild realm is required.");
        }

        var guild = Guild.Create(command.Name, command.Region, command.Realm);
        await _guildRepository.AddAsync(guild, cancellationToken);
        return Result<GuildDto>.Success(DtoMapper.ToDto(guild));
    }
}
