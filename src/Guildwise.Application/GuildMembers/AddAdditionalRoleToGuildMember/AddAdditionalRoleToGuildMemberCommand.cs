using Guildwise.Domain;

namespace Guildwise.Application.GuildMembers.AddAdditionalRoleToGuildMember;

public sealed record AddAdditionalRoleToGuildMemberCommand(
    Guid GuildId,
    Guid PlayerId,
    AdditionalGuildRole Role);
