using Guildwise.Domain;

namespace Guildwise.Application.Contracts.RaidEvents;

public sealed record RaidEventSignupDto(
    Guid RaidEventId,
    Guid PlayerId,
    string PlayerDisplayName,
    RaidEventSignupStatus Status,
    Guid? MainCharacterId,
    string? MainCharacterName,
    CharacterClass? CharacterClass,
    CharacterSpecialization? Specialization,
    CharacterRole? Role,
    bool HasMainCharacter,
    GuildRank? GuildRank,
    IReadOnlyList<AdditionalGuildRole> AdditionalRoles);
