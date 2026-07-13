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
    private async Task AddPlayerToTeamAsync(AvailableRaidTeamPlayerDto player)
    {
        if (!TryGetSelectedContext(out var guildId, out var raidTeamId))
        {
            return;
        }

        actionInProgress = true;

        try
        {
            var result = await AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(
                guildId,
                raidTeamId,
                player.PlayerId));

            if (result.IsFailure)
            {
                ShowFailure(result.Failure!);
                return;
            }

            var teamName = SelectedTeam?.Name ?? "the selected raid team";
            ShowSuccess($"Added {player.PlayerDisplayName} to {teamName}.");
            await LoadRaidTeamsAsync();
        }
        catch
        {
            loadFailure = "An unexpected technical failure occurred while adding a raid team member.";
        }
        finally
        {
            actionInProgress = false;
        }
    }

    private async Task RemovePlayerFromTeamAsync(RaidTeamManagementMemberDto member)
    {
        if (!TryGetSelectedContext(out var guildId, out var raidTeamId))
        {
            return;
        }

        actionInProgress = true;

        try
        {
            var result = await RemovePlayerFromRaidTeamHandler.HandleAsync(new RemovePlayerFromRaidTeamCommand(
                guildId,
                raidTeamId,
                member.PlayerId));

            if (result.IsFailure)
            {
                ShowFailure(result.Failure!);
                return;
            }

            var teamName = SelectedTeam?.Name ?? "the selected raid team";
            ShowSuccess($"Removed {member.PlayerDisplayName} from {teamName}.");
            await LoadRaidTeamsAsync();
        }
        catch
        {
            loadFailure = "An unexpected technical failure occurred while removing a raid team member.";
        }
        finally
        {
            actionInProgress = false;
        }
    }

}
