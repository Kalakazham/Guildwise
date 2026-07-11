using Guildwise.Domain;

namespace Guildwise.Application.Contracts.RosterOverview;

public sealed record RosterMemberDto(
    Guid PlayerId,
    string PlayerDisplayName,
    Guid? MainCharacterId,
    string? MainCharacterName,
    string? CharacterRegion,
    string? CharacterRealm,
    CharacterClass? CharacterClass,
    CharacterSpecialization? Specialization,
    CharacterRole? Role,
    bool HasMainCharacter,
    bool IsGuildMember,
    GuildRank? GuildRank,
    IReadOnlyList<AdditionalGuildRole> AdditionalRoles,
    IReadOnlyList<string> RaidTeamNames);
