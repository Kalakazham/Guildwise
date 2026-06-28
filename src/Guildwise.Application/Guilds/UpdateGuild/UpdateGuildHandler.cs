using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.Guilds;

namespace Guildwise.Application.Guilds.UpdateGuild;

public sealed class UpdateGuildHandler
{
    private readonly IGuildRepository _guildRepository;

    public UpdateGuildHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task<Result<GuildDto>> HandleAsync(
        UpdateGuildCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetByIdAsync(command.GuildId, cancellationToken);
        if (guild is null)
        {
            return Result<GuildDto>.NotFound($"Guild '{command.GuildId}' was not found.");
        }

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

        guild.Update(command.Name, command.Region, command.Realm);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return Result<GuildDto>.Success(DtoMapper.ToDto(guild));
    }
}
