using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.Guilds;
using Guildwise.Application.Contracts.Players;
using Guildwise.Application.Contracts.RaidEvents;
using Guildwise.Application.Contracts.RaidTeams;
using Guildwise.Application.Guilds.ListGuilds;
using Guildwise.Application.Players.ListPlayers;
using Guildwise.Application.RaidEvents.CancelRaidEvent;
using Guildwise.Application.RaidEvents.CreateRaidEvent;
using Guildwise.Application.RaidEvents.ListRaidEventSignups;
using Guildwise.Application.RaidEvents.ListRaidEvents;
using Guildwise.Application.RaidEvents.SetRaidEventSignup;
using Guildwise.Application.RaidEvents.UpdateRaidEvent;
using Guildwise.Domain;
using Guildwise.Web.Components.Pages.RaidEventComponents;
using Microsoft.AspNetCore.Components;

namespace Guildwise.Web.Components.Pages;

public partial class RaidEvents
{
    private async Task<RaidEventActionResult> CreateRaidEventAsync(RaidEventFormSubmission submission)
    {
        try
        {
            var command = new CreateRaidEventCommand(
                submission.GuildId,
                submission.RaidTeamId,
                submission.Title,
                submission.StartTime,
                submission.EndTime,
                submission.InstanceName,
                submission.Difficulty,
                submission.Notes);

            var result = await CreateRaidEventHandler.HandleAsync(command);
            if (result.IsFailure)
            {
                return ToActionFailure(result.Failure);
            }

            var createdEvent = result.Value ?? throw new InvalidOperationException("Create raid event returned no event.");
            await LoadRaidEventsAsync(createdEvent.Id);
            return RaidEventActionResult.Success($"Created {createdEvent.Title}.");
        }
        catch (Exception exception)
        {
            return RaidEventActionResult.Failure($"Unexpected error while creating raid event: {exception.Message}");
        }
    }

    private async Task<RaidEventActionResult> UpdateRaidEventAsync(RaidEventFormSubmission submission)
    {
        var selectedEvent = SelectedEvent;
        if (selectedEvent is null)
        {
            return RaidEventActionResult.Failure("Not found: Selected raid event was not found.");
        }

        try
        {
            var command = new UpdateRaidEventCommand(
                selectedEvent.Id,
                submission.GuildId,
                submission.RaidTeamId,
                submission.Title,
                submission.StartTime,
                submission.EndTime,
                submission.InstanceName,
                submission.Difficulty,
                submission.Notes);

            var result = await UpdateRaidEventHandler.HandleAsync(command);
            if (result.IsFailure)
            {
                return ToActionFailure(result.Failure);
            }

            var updatedEvent = result.Value ?? throw new InvalidOperationException("Update raid event returned no event.");
            isEditingSelectedEvent = false;
            await LoadRaidEventsAsync(updatedEvent.Id);
            return RaidEventActionResult.Success($"Updated {updatedEvent.Title}.");
        }
        catch (Exception exception)
        {
            return RaidEventActionResult.Failure($"Unexpected error while updating raid event: {exception.Message}");
        }
    }

    private async Task<RaidEventActionResult> CancelRaidEventAsync(RaidEventDto raidEvent)
    {
        try
        {
            var result = await CancelRaidEventHandler.HandleAsync(new CancelRaidEventCommand(raidEvent.Id));
            if (result.IsFailure)
            {
                return ToActionFailure(result.Failure);
            }

            var cancelledEvent = result.Value ?? throw new InvalidOperationException("Cancel raid event returned no event.");
            isEditingSelectedEvent = false;
            await LoadRaidEventsAsync(cancelledEvent.Id);
            return RaidEventActionResult.Success($"Cancelled {cancelledEvent.Title}.");
        }
        catch (Exception exception)
        {
            return RaidEventActionResult.Failure($"Unexpected error while cancelling raid event: {exception.Message}");
        }
    }

    private async Task<RaidEventActionResult> SetSignupStatusAsync(SignupStatusChangeRequest request)
    {
        try
        {
            var command = new SetRaidEventSignupCommand(request.RaidEventId, request.PlayerId, request.Status);
            var result = await SetRaidEventSignupHandler.HandleAsync(command);
            if (result.IsFailure)
            {
                return ToActionFailure(result.Failure);
            }

            var signup = result.Value ?? throw new InvalidOperationException("Set raid event signup returned no signup.");
            await LoadSelectedEventSignupsAsync(request.RaidEventId);
            await InvokeAsync(StateHasChanged);

            var action = request.CurrentStatus.HasValue ? "Changed" : "Set";
            return RaidEventActionResult.Success($"{action} {signup.PlayerDisplayName} to {RaidEventUiText.FormatSignupStatus(signup.Status)}.");
        }
        catch (Exception exception)
        {
            return RaidEventActionResult.Failure($"Unexpected error while setting signup status: {exception.Message}");
        }
    }

    private static RaidEventActionResult ToActionFailure(Failure? failure)
        => failure is null
            ? RaidEventActionResult.Failure("Failure: The action could not be completed.")
            : RaidEventActionResult.Failure(RaidEventUiText.FormatFailure(failure));
}
