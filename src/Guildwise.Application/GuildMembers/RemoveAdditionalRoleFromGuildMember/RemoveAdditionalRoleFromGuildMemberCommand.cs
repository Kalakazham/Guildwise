using Guildwise.Domain;

namespace Guildwise.Application.GuildMembers.RemoveAdditionalRoleFromGuildMember;

public sealed record RemoveAdditionalRoleFromGuildMemberCommand(
    Guid GuildId,
    Guid PlayerId,
    AdditionalGuildRole Role);
