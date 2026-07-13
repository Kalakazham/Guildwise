using Guildwise.Application.Contracts.RosterOverview;
using Guildwise.Application.RosterOverview.GetRosterOverview;
using Guildwise.Domain;
using Microsoft.AspNetCore.Components;

namespace Guildwise.Web.Components.Pages;

public partial class RosterOverview
{
    private List<RosterMemberDto> GetVisibleMembers()
    {
        if (overview is null)
        {
            return [];
        }

        var members = overview.Members
            .Where(MatchesSearch)
            .Where(MatchesClass)
            .Where(MatchesRole)
            .Where(MatchesGuildMembership)
            .Where(MatchesMainCharacter)
            .Where(MatchesRaidTeam);

        return ApplySort(members).ToList();
    }

    private bool MatchesSearch(RosterMemberDto member)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        var search = searchText.Trim();
        return ContainsSearch(member.PlayerDisplayName, search)
            || ContainsSearch(member.MainCharacterName, search)
            || ContainsSearch(member.CharacterRealm, search)
            || member.RaidTeamNames.Any(team => ContainsSearch(team, search));
    }

    private bool MatchesClass(RosterMemberDto member)
        => selectedClass == AllFilterValue
            || member.CharacterClass?.ToString() == selectedClass;

    private bool MatchesRole(RosterMemberDto member)
        => selectedRole == AllFilterValue
            || member.Role?.ToString() == selectedRole;

    private bool MatchesGuildMembership(RosterMemberDto member)
        => selectedGuildMembership switch
        {
            GuildMembersFilterValue => member.IsGuildMember,
            NotInGuildFilterValue => !member.IsGuildMember,
            _ => true
        };

    private bool MatchesMainCharacter(RosterMemberDto member)
        => selectedMainCharacter switch
        {
            HasMainFilterValue => member.HasMainCharacter,
            MissingMainFilterValue => !member.HasMainCharacter,
            _ => true
        };

    private bool MatchesRaidTeam(RosterMemberDto member)
        => selectedRaidTeam switch
        {
            NoRaidTeamFilterValue => member.RaidTeamNames.Count == 0,
            AllFilterValue => true,
            _ => member.RaidTeamNames.Contains(selectedRaidTeam, StringComparer.OrdinalIgnoreCase)
        };

    private IOrderedEnumerable<RosterMemberDto> ApplySort(IEnumerable<RosterMemberDto> members)
        => selectedSort switch
        {
            SortMainCharacterName => members
                .OrderBy(member => member.MainCharacterName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerDisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerId),
            SortClass => members
                .OrderBy(member => FormatClassForSort(member), StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerDisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerId),
            SortRole => members
                .OrderBy(member => FormatRoleForSort(member), StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerDisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerId),
            SortGuildRank => members
                .OrderBy(member => FormatGuildRankForSort(member), StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerDisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerId),
            SortRaidTeamCount => members
                .OrderBy(member => member.RaidTeamNames.Count)
                .ThenBy(member => member.PlayerDisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerId),
            SortRaidTeamName => members
                .OrderBy(member => member.RaidTeamNames.Count > 0 ? member.RaidTeamNames[0] : "zzzzzz", StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerDisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerId),
            _ => members
                .OrderBy(member => member.PlayerDisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.PlayerId)
        };

    private void ResetRosterControls()
    {
        searchText = string.Empty;
        selectedClass = AllFilterValue;
        selectedRole = AllFilterValue;
        selectedGuildMembership = AllFilterValue;
        selectedMainCharacter = AllFilterValue;
        selectedRaidTeam = AllFilterValue;
        selectedSort = SortPlayerName;
    }

    private static bool ContainsSearch(string? value, string search)
        => !string.IsNullOrWhiteSpace(value)
            && value.Contains(search, StringComparison.OrdinalIgnoreCase);

    private static string FormatClassForSort(RosterMemberDto member)
        => member.CharacterClass.HasValue
            ? RosterDisplayText.CharacterClass(member.CharacterClass.Value)
            : "zzzzzz";

    private static string FormatRoleForSort(RosterMemberDto member)
        => member.Role.HasValue
            ? RosterDisplayText.CharacterRole(member.Role.Value)
            : "zzzzzz";

    private static string FormatGuildRankForSort(RosterMemberDto member)
        => member.GuildRank.HasValue
            ? RosterDisplayText.GuildRank(member.GuildRank.Value)
            : "zzzzzz";
}
