using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
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

    public async Task<GuildMemberDto> HandleAsync(
        AddPlayerToGuildCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetGuildOrThrowAsync(command.GuildId, cancellationToken);
        var player = await _playerRepository.GetPlayerOrThrowAsync(command.PlayerId, cancellationToken);
        var member = guild.AddMember(player, command.Rank);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return DtoMapper.ToDto(member);
    }
}
