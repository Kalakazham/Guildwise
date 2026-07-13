using Guildwise.Application.Contracts.RosterOverview;
using Guildwise.Application.RosterOverview.GetRosterOverview;
using Guildwise.Domain;
using Microsoft.AspNetCore.Components;

namespace Guildwise.Web.Components.Pages;

public partial class RosterOverview
{
    private const string AllFilterValue = "all";
    private const string GuildMembersFilterValue = "guild-members";
    private const string NotInGuildFilterValue = "not-in-guild";
    private const string HasMainFilterValue = "has-main";
    private const string MissingMainFilterValue = "missing-main";
    private const string NoRaidTeamFilterValue = "no-raid-team";
    private const string SortPlayerName = "player-name";
    private const string SortMainCharacterName = "main-character-name";
    private const string SortClass = "class";
    private const string SortRole = "role";
    private const string SortGuildRank = "guild-rank";
    private const string SortRaidTeamCount = "raid-team-count";
    private const string SortRaidTeamName = "raid-team-name";

    private static readonly IReadOnlyList<CharacterRole> StableRoles =
    [
        CharacterRole.Tank,
        CharacterRole.Healer,
        CharacterRole.Damage
    ];

    private RosterOverviewDto? overview;
    private bool isLoading = true;
    private string? loadFailure;
    private string searchText = string.Empty;
    private string selectedClass = AllFilterValue;
    private string selectedRole = AllFilterValue;
    private string selectedGuildMembership = AllFilterValue;
    private string selectedMainCharacter = AllFilterValue;
    private string selectedRaidTeam = AllFilterValue;
    private string selectedSort = SortPlayerName;

    private bool HasActiveRosterControls
        => !string.IsNullOrWhiteSpace(searchText)
            || selectedClass != AllFilterValue
            || selectedRole != AllFilterValue
            || selectedGuildMembership != AllFilterValue
            || selectedMainCharacter != AllFilterValue
            || selectedRaidTeam != AllFilterValue
            || selectedSort != SortPlayerName;

    private IReadOnlyList<CharacterClass> AvailableClasses
        => overview?.Members
            .Where(member => member.CharacterClass.HasValue)
            .Select(member => member.CharacterClass!.Value)
            .Distinct()
            .OrderBy(RosterDisplayText.CharacterClass, StringComparer.OrdinalIgnoreCase)
            .ToList()
        ?? [];

    private IReadOnlyList<string> AvailableRaidTeams
        => overview?.Members
            .SelectMany(member => member.RaidTeamNames)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList()
        ?? [];

    protected override async Task OnInitializedAsync()
    {
        await LoadRosterAsync();
    }

    private async Task LoadRosterAsync()
    {
        isLoading = true;
        loadFailure = null;

        try
        {
            overview = await GetRosterOverviewHandler.HandleAsync(new GetRosterOverviewQuery());
        }
        catch
        {
            overview = null;
            loadFailure = "An unexpected technical failure occurred while loading the roster overview.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private static bool IsRosterEmpty(RosterOverviewDto roster)
        => roster.Summary.GuildCount == 0
            && roster.Summary.PlayerCount == 0
            && roster.Summary.CharacterCount == 0
            && roster.Summary.RaidTeamCount == 0;
}
