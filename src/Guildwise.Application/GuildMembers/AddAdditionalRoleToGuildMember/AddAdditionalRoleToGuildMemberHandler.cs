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

    public async Task<GuildMemberDto> HandleAsync(
        AddAdditionalRoleToGuildMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetGuildOrThrowAsync(command.GuildId, cancellationToken);
        var member = guild.GetGuildMemberOrThrow(command.PlayerId);
        member.AddAdditionalRole(command.Role);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return DtoMapper.ToDto(member);
    }
}
