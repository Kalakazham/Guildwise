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

    public GuildMemberDto Handle(AddPlayerToGuildCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = _guildRepository.GetGuildOrThrow(command.GuildId);
        var player = _playerRepository.GetPlayerOrThrow(command.PlayerId);
        var member = guild.AddMember(player, command.Rank);
        _guildRepository.SaveChanges();
        return DtoMapper.ToDto(member);
    }
}
