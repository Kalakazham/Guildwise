using Guildwise.Application.Characters.CreateCharacter;
using Guildwise.Application.Characters.SetMainCharacter;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.Characters;
using Guildwise.Application.Contracts.Guilds;
using Guildwise.Application.Contracts.Players;
using Guildwise.Application.Contracts.RaidTeams;
using Guildwise.Application.GuildMembers.AddPlayerToGuild;
using Guildwise.Application.Guilds.CreateGuild;
using Guildwise.Application.Guilds.ListGuilds;
using Guildwise.Application.Players.CreatePlayer;
using Guildwise.Application.Players.ListPlayers;
using Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;
using Guildwise.Application.RaidTeams.CreateRaidTeam;
using Guildwise.Application.RaidTeams.ListRaidTeamsForGuild;
using Guildwise.Domain;
using Microsoft.AspNetCore.Components;

namespace Guildwise.Web.Components.Pages;

public partial class Home
{
    private async Task RunActionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch
        {
            statusMessage = "Unexpected failure while running the action.";
            statusKind = "error";
        }
    }

    private async Task RefreshDataAsync()
    {
        guilds = (await ListGuildsHandler.HandleAsync(new ListGuildsQuery())).ToList();
        players = (await ListPlayersHandler.HandleAsync(new ListPlayersQuery())).ToList();

        if (!Guid.TryParse(selectedGuildId, out var guildId) || guilds.All(guild => guild.Id != guildId))
        {
            selectedGuildId = guilds.Count == 0 ? null : guilds[0].Id.ToString();
            guildId = guilds.Count == 0 ? Guid.Empty : guilds[0].Id;
        }

        raidTeams = guildId == Guid.Empty
            ? []
            : (await ListRaidTeamsForGuildHandler.HandleAsync(new ListRaidTeamsForGuildQuery(guildId))).ToList();

        if (!Guid.TryParse(selectedPlayerId, out _) && players.Count > 0)
        {
            selectedPlayerId = players[0].Id.ToString();
        }

        if (!Guid.TryParse(selectedRaidTeamId, out var raidTeamId) || raidTeams.All(raidTeam => raidTeam.Id != raidTeamId))
        {
            selectedRaidTeamId = raidTeams.Count == 0 ? null : raidTeams[0].Id.ToString();
        }
    }

    private async Task RunResultActionAsync<T>(Func<Task<Result<T>>> action, string successMessage)
        => await RunResultActionAsync(action, _ => successMessage);

    private async Task RunResultActionAsync<T>(
        Func<Task<Result<T>>> action,
        Func<T, string> successMessage,
        Action<T>? onSuccess = null)
    {
        var result = await action();
        if (result.IsFailure)
        {
            ShowFailure(result.Failure!);
            return;
        }

        var value = GetSuccessfulValue(result);
        onSuccess?.Invoke(value);
        statusMessage = successMessage(value);
        statusKind = "success";
        await RefreshDataAsync();
    }

    private async Task RunResultActionAsync(Func<Task<Result>> action, string successMessage)
    {
        var result = await action();
        if (result.IsFailure)
        {
            ShowFailure(result.Failure!);
            return;
        }

        statusMessage = successMessage;
        statusKind = "success";
        await RefreshDataAsync();
    }

    private bool TryReadGuid(string? value, string message, out Guid id)
    {
        if (Guid.TryParse(value, out id))
        {
            return true;
        }

        ShowFailure(new Failure(FailureType.Validation, message));
        return false;
    }

    private void ShowFailure(Failure failure)
    {
        statusMessage = $"{FormatFailureType(failure.Type)}: {failure.Message}";
        statusKind = "error";
    }

    private static T GetSuccessfulValue<T>(Result<T> result)
        => result.IsSuccess && result.Value is not null
            ? result.Value
            : throw new InvalidOperationException("Successful application result did not include a value.");

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
