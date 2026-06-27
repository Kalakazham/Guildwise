using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.GuildMembers;

namespace Guildwise.Application.GuildMembers.AddAdditionalRoleToGuildMember;

public sealed class AddAdditionalRoleToGuildMemberHandler
{
    private readonly IGuildRepository _guildRepository;

    public AddAdditionalRoleToGuildMemberHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public GuildMemberDto Handle(AddAdditionalRoleToGuildMemberCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = _guildRepository.GetGuildOrThrow(command.GuildId);
        var member = guild.GetGuildMemberOrThrow(command.PlayerId);
        member.AddAdditionalRole(command.Role);
        return DtoMapper.ToDto(member);
    }
}
