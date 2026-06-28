using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.GuildMembers;

namespace Guildwise.Application.GuildMembers.AddPlayerToGuild;

public sealed class AddPlayerToGuildHandler
{
    private readonly IGuildRepository _guildRepository;
    private readonly IPlayerRepository _playerRepository;

    public AddPlayerToGuildHandler(IGuildRepository guildRepository, IPlayerRepository playerRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<Result<GuildMemberDto>> HandleAsync(
        AddPlayerToGuildCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetByIdAsync(command.GuildId, cancellationToken);
        if (guild is null)
        {
            return Result<GuildMemberDto>.NotFound($"Guild '{command.GuildId}' was not found.");
        }

        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result<GuildMemberDto>.NotFound($"Player '{command.PlayerId}' was not found.");
        }

        if (EqualityComparer<Guildwise.Domain.GuildRank>.Default.Equals(command.Rank, default)
            || !Enum.IsDefined(command.Rank))
        {
            return Result<GuildMemberDto>.Validation("Guild rank is required.");
        }

        if (guild.Members.Any(member => member.PlayerId == player.Id))
        {
            return Result<GuildMemberDto>.Conflict("Player is already a guild member.");
        }

        var member = guild.AddMember(player, command.Rank);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return Result<GuildMemberDto>.Success(DtoMapper.ToDto(member));
    }
}
