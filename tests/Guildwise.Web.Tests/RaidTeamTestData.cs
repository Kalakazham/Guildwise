using Guildwise.Application.Contracts.RaidTeamManagement;
using Guildwise.Domain;

namespace Guildwise.Web.Tests;

internal static class RaidTeamTestData
{
    public static RaidTeamManagementGuildDto Guild(
        Guid id,
        IReadOnlyList<AvailableRaidTeamPlayerDto> availablePlayers,
        IReadOnlyList<RaidTeamManagementTeamDto> teams)
        => new(
            id,
            $"Guild {id:N}"[..12],
            "EU",
            "Argent Dawn",
            teams.Count,
            availablePlayers.Count + teams.Sum(team => team.MemberCount),
            teams.Sum(team => team.MemberCount),
            availablePlayers.Count,
            availablePlayers.Count(player => !player.HasMainCharacter),
            availablePlayers,
            teams);

    public static RaidTeamManagementTeamDto Team(
        Guid id,
        Guid guildId,
        string name,
        IReadOnlyList<RaidTeamManagementMemberDto>? members = null,
        RaidTeamCompositionDto? composition = null)
    {
        var teamMembers = members ?? [];

        return new(
            id,
            guildId,
            name,
            teamMembers.Count,
            composition ?? new RaidTeamCompositionDto(
                teamMembers.Count(member => member.Role == CharacterRole.Tank),
                teamMembers.Count(member => member.Role == CharacterRole.Healer),
                teamMembers.Count(member => member.Role == CharacterRole.Damage)),
            teamMembers);
    }

    public static AvailableRaidTeamPlayerDto AvailablePlayer(
        Guid playerId,
        string playerDisplayName,
        string mainCharacterName,
        CharacterClass characterClass = CharacterClass.Mage,
        CharacterRole role = CharacterRole.Damage,
        IReadOnlyList<Guid>? raidTeamIds = null)
        => new(
            playerId,
            playerDisplayName,
            Guid.NewGuid(),
            mainCharacterName,
            "EU",
            $"{mainCharacterName} Realm",
            characterClass,
            CharacterSpecialization.Unknown,
            role,
            true,
            GuildRank.Member,
            [],
            raidTeamIds ?? [],
            []);

    public static RaidTeamManagementMemberDto Member(
        Guid playerId,
        string playerDisplayName,
        bool hasMainCharacter = true,
        CharacterRole? role = CharacterRole.Damage)
        => new(
            playerId,
            playerDisplayName,
            hasMainCharacter ? Guid.NewGuid() : null,
            hasMainCharacter ? $"{playerDisplayName} Main" : null,
            hasMainCharacter ? "EU" : null,
            hasMainCharacter ? "Argent Dawn" : null,
            hasMainCharacter ? CharacterClass.Mage : null,
            hasMainCharacter ? CharacterSpecialization.Unknown : null,
            role,
            hasMainCharacter,
            GuildRank.Member,
            []);
}
