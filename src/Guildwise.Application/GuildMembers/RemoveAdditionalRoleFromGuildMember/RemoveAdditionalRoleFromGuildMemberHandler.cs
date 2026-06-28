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

    public async Task<GuildMemberDto> HandleAsync(
        RemoveAdditionalRoleFromGuildMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetGuildOrThrowAsync(command.GuildId, cancellationToken);
        var member = guild.GetGuildMemberOrThrow(command.PlayerId);
        member.RemoveAdditionalRole(command.Role);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return DtoMapper.ToDto(member);
    }
}
