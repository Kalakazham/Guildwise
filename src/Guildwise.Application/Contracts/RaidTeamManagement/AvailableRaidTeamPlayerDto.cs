using Guildwise.Domain;

namespace Guildwise.Application.Contracts.RaidTeamManagement;

public sealed record AvailableRaidTeamPlayerDto(
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
    GuildRank? GuildRank,
    IReadOnlyList<AdditionalGuildRole> AdditionalRoles,
    IReadOnlyList<Guid> RaidTeamIds,
    IReadOnlyList<string> RaidTeamNames);
