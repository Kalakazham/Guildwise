using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.GuildMembers;
using Guildwise.Domain;

namespace Guildwise.Application.GuildMembers.RemoveAdditionalRoleFromGuildMember;

public sealed class RemoveAdditionalRoleFromGuildMemberHandler
{
    private readonly IGuildRepository _guildRepository;

    public RemoveAdditionalRoleFromGuildMemberHandler(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
    }

    public async Task<Result<GuildMemberDto>> HandleAsync(
        RemoveAdditionalRoleFromGuildMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetByIdAsync(command.GuildId, cancellationToken);
        if (guild is null)
        {
            return Result<GuildMemberDto>.NotFound($"Guild '{command.GuildId}' was not found.");
        }

        var member = guild.Members.FirstOrDefault(existing => existing.PlayerId == command.PlayerId);
        if (member is null)
        {
            return Result<GuildMemberDto>.NotFound($"GuildMember '{command.PlayerId}' was not found.");
        }

        if (EqualityComparer<AdditionalGuildRole>.Default.Equals(command.Role, default)
            || !Enum.IsDefined(command.Role))
        {
            return Result<GuildMemberDto>.Validation("Additional guild role is required.");
        }

        member.RemoveAdditionalRole(command.Role);
        await _guildRepository.SaveChangesAsync(cancellationToken);
        return Result<GuildMemberDto>.Success(DtoMapper.ToDto(member));
    }
}
