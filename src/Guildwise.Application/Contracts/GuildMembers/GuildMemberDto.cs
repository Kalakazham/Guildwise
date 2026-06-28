using Guildwise.Domain;

namespace Guildwise.Application.Contracts.GuildMembers;

public sealed record GuildMemberDto(
    Guid Id,
    Guid GuildId,
    Guid PlayerId,
    GuildRank Rank,
    IReadOnlyList<AdditionalGuildRole> AdditionalRoles);
