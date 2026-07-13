using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.RaidTeamManagement;
using Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;
using Guildwise.Application.RaidTeams.RemovePlayerFromRaidTeam;
using Guildwise.Application.RaidTeamManagement.GetRaidTeamManagementOverview;
using Guildwise.Domain;
using Microsoft.AspNetCore.Components;

namespace Guildwise.Web.Components.Pages;

public partial class RaidTeamManagement
{
    private RaidTeamManagementOverviewDto? overview;
    private bool isLoading = true;
    private string? loadFailure;
    private bool actionInProgress;
    private string? actionMessage;
    private string actionStatus = "success";
    private Guid? selectedGuildId;
    private Guid? selectedRaidTeamId;
    private RaidTeamManagementGuildDto? SelectedGuild
        => overview?.Guilds.FirstOrDefault(guild => guild.Id == selectedGuildId);

    private RaidTeamManagementTeamDto? SelectedTeam
        => SelectedGuild?.Teams.FirstOrDefault(team => team.Id == selectedRaidTeamId);

    protected override async Task OnInitializedAsync()
    {
        await LoadRaidTeamsAsync();
    }

    private async Task LoadRaidTeamsAsync()
    {
        isLoading = true;
        loadFailure = null;

        try
        {
            overview = await GetRaidTeamManagementOverviewHandler.HandleAsync(new GetRaidTeamManagementOverviewQuery());
            EnsureSelection();
        }
        catch
        {
            overview = null;
            selectedGuildId = null;
            selectedRaidTeamId = null;
            loadFailure = "An unexpected technical failure occurred while loading raid team management data.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OnGuildChanged(ChangeEventArgs args)
    {
        if (!Guid.TryParse(args.Value?.ToString(), out var guildId))
        {
            return;
        }

        selectedGuildId = guildId;
        selectedRaidTeamId = SelectedGuild?.Teams.Count > 0 ? SelectedGuild.Teams[0].Id : null;
        actionMessage = null;
    }

    private void SelectTeam(Guid teamId)
    {
        selectedRaidTeamId = teamId;
        actionMessage = null;
    }

    private void EnsureSelection()
    {
        if (overview is null || overview.Guilds.Count == 0)
        {
            selectedGuildId = null;
            selectedRaidTeamId = null;
            return;
        }

        if (selectedGuildId is null || overview.Guilds.All(guild => guild.Id != selectedGuildId.Value))
        {
            selectedGuildId = overview.Guilds[0].Id;
        }

        var selectedGuild = SelectedGuild;
        if (selectedGuild is null || selectedGuild.Teams.Count == 0)
        {
            selectedRaidTeamId = null;
            return;
        }

        if (selectedRaidTeamId is null || selectedGuild.Teams.All(team => team.Id != selectedRaidTeamId.Value))
        {
            selectedRaidTeamId = selectedGuild.Teams[0].Id;
        }
    }

    private bool TryGetSelectedContext(out Guid guildId, out Guid raidTeamId)
    {
        guildId = selectedGuildId ?? Guid.Empty;
        raidTeamId = selectedRaidTeamId ?? Guid.Empty;

        if (guildId != Guid.Empty && raidTeamId != Guid.Empty)
        {
            return true;
        }

        ShowFailure(new Failure(FailureType.Validation, "Select a guild and raid team before changing team members."));
        return false;
    }

    private void ShowSuccess(string message)
    {
        actionMessage = message;
        actionStatus = "success";
    }

    private void ShowFailure(Failure failure)
    {
        actionMessage = $"{FormatFailureType(failure.Type)}: {failure.Message}";
        actionStatus = "error";
    }

    private static string FormatFailureType(FailureType type)
        => type switch
        {
            FailureType.NotFound => "Not found",
            FailureType.Validation => "Validation",
            FailureType.Conflict => "Conflict",
            FailureType.BusinessRule => "Cannot complete action",
            _ => "Failure"
        };

}
