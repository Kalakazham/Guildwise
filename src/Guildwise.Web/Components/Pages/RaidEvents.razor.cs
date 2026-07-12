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
    private IReadOnlyList<RaidEventDto> raidEvents = [];
    private IReadOnlyList<GuildDto> guilds = [];
    private IReadOnlyList<PlayerDto> players = [];
    private IReadOnlyList<RaidEventSignupDto> selectedEventSignups = [];
    private Guid? loadedSignupRaidEventId;
    private bool signupsLoading;
    private string? signupLoadFailure;
    private Guid? selectedRaidEventId;
    private bool isLoading;
    private bool isEditingSelectedEvent;
    private string? loadFailure;

    private List<RaidEventDto> OrderedRaidEvents => raidEvents
        .OrderBy(raidEvent => raidEvent.StartTime)
        .ThenBy(raidEvent => raidEvent.Title, StringComparer.OrdinalIgnoreCase)
        .ThenBy(raidEvent => raidEvent.Id)
        .ToList();

    private RaidEventDto? SelectedEvent
        => selectedRaidEventId.HasValue
            ? raidEvents.FirstOrDefault(raidEvent => raidEvent.Id == selectedRaidEventId.Value)
            : null;

    private bool HasAnyRaidTeams => guilds.Any(guild => guild.RaidTeams.Count > 0);

    private int RaidTeamsWithEventsCount => raidEvents
        .Select(raidEvent => raidEvent.RaidTeamId)
        .Distinct()
        .Count();

    private IReadOnlyList<RaidTeamOption> RaidTeamOptions => guilds
        .SelectMany(guild => guild.RaidTeams.Select(raidTeam => new RaidTeamOption(
            guild.Id,
            raidTeam.Id,
            raidTeam.Name,
            guild.Name,
            guild.Region,
            guild.Realm)))
        .OrderBy(option => option.GuildName, StringComparer.OrdinalIgnoreCase)
        .ThenBy(option => option.RaidTeamName, StringComparer.OrdinalIgnoreCase)
        .ToList();

    protected override async Task OnInitializedAsync()
    {
        await LoadRaidEventsAsync();
    }
    private async Task LoadRaidEventsAsync()
    {
        isLoading = true;
        loadFailure = null;

        try
        {
            guilds = await ListGuildsHandler.HandleAsync(new ListGuildsQuery());
            players = await ListPlayersHandler.HandleAsync(new ListPlayersQuery());
            raidEvents = await ListRaidEventsHandler.HandleAsync(new ListRaidEventsQuery());
            EnsureSelection();
            EnsureEditStateIsValid();
            await LoadSelectedEventSignupsAsync(selectedRaidEventId);
        }
        catch (Exception exception)
        {
            loadFailure = exception.Message;
            selectedEventSignups = [];
            loadedSignupRaidEventId = null;
            signupLoadFailure = null;
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadRaidEventsAsync(Guid? preferredRaidEventId)
    {
        selectedRaidEventId = preferredRaidEventId;
        await LoadRaidEventsAsync();
    }

    private async Task SelectEventAsync(Guid raidEventId)
    {
        isEditingSelectedEvent = false;
        selectedRaidEventId = raidEventId;
        await LoadSelectedEventSignupsAsync(raidEventId);
    }

    private async Task LoadSelectedEventSignupsAsync(Guid? raidEventId)
    {
        if (!raidEventId.HasValue)
        {
            selectedEventSignups = [];
            loadedSignupRaidEventId = null;
            signupLoadFailure = null;
            signupsLoading = false;
            return;
        }

        var requestedRaidEventId = raidEventId.Value;
        signupsLoading = true;
        signupLoadFailure = null;

        try
        {
            var signups = await ListRaidEventSignupsHandler.HandleAsync(new ListRaidEventSignupsQuery(requestedRaidEventId));
            if (selectedRaidEventId == requestedRaidEventId)
            {
                selectedEventSignups = signups;
                loadedSignupRaidEventId = requestedRaidEventId;
            }
        }
        catch (Exception exception)
        {
            if (selectedRaidEventId == requestedRaidEventId)
            {
                selectedEventSignups = [];
                loadedSignupRaidEventId = null;
                signupLoadFailure = exception.Message;
            }
        }
        finally
        {
            if (selectedRaidEventId == requestedRaidEventId)
            {
                signupsLoading = false;
            }
        }
    }

    private void StartEdit(RaidEventDto raidEvent)
    {
        if (raidEvent.Status != RaidEventStatus.Scheduled)
        {
            return;
        }

        selectedRaidEventId = raidEvent.Id;
        isEditingSelectedEvent = true;
    }

    private void DiscardEditChanges()
    {
        isEditingSelectedEvent = false;
    }

    private void EnsureSelection()
    {
        if (raidEvents.Count == 0)
        {
            selectedRaidEventId = null;
            return;
        }

        if (!selectedRaidEventId.HasValue || raidEvents.All(raidEvent => raidEvent.Id != selectedRaidEventId.Value))
        {
            selectedRaidEventId = OrderedRaidEvents[0].Id;
        }
    }

    private void EnsureEditStateIsValid()
    {
        if (!isEditingSelectedEvent)
        {
            return;
        }

        var selectedEvent = SelectedEvent;
        if (selectedEvent is null || selectedEvent.Status != RaidEventStatus.Scheduled)
        {
            isEditingSelectedEvent = false;
        }
    }

    private GuildDto? FindGuild(RaidEventDto raidEvent)
        => guilds.FirstOrDefault(guild => guild.Id == raidEvent.GuildId);

    private RaidTeamDto? FindRaidTeam(RaidEventDto raidEvent)
        => FindGuild(raidEvent)?.RaidTeams.FirstOrDefault(raidTeam => raidTeam.Id == raidEvent.RaidTeamId);
}

