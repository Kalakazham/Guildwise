using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.GuildMembers;

namespace Guildwise.Application.GuildMembers.RemoveAdditionalRoleFromGuildMember;

public sealed class RemoveAdditionalRoleFromGuildMemberHandler
{
    private readonly IGuildRepository _guildRepository;

    public RemoveAdditionalRoleFromGuildMemberHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public GuildMemberDto Handle(RemoveAdditionalRoleFromGuildMemberCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = _guildRepository.GetGuildOrThrow(command.GuildId);
        var member = guild.GetGuildMemberOrThrow(command.PlayerId);
        member.RemoveAdditionalRole(command.Role);
        _guildRepository.SaveChanges();
        return DtoMapper.ToDto(member);
    }
}
