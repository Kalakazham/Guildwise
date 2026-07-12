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
    private async Task CreateGuild()
    {
        await RunActionAsync(async () =>
        {
            await RunResultActionAsync(
                () => CreateGuildHandler.HandleAsync(new CreateGuildCommand(guildName, guildRegion, guildRealm)),
                guild => $"Created guild {guild.Name}.",
                guild => selectedGuildId = guild.Id.ToString());
        });
    }

    private async Task CreatePlayer()
    {
        await RunActionAsync(async () =>
        {
            await RunResultActionAsync(
                () => CreatePlayerHandler.HandleAsync(new CreatePlayerCommand(playerName)),
                player => $"Created player {player.DisplayName}.",
                player => selectedPlayerId = player.Id.ToString());
        });
    }

    private async Task CreateCharacter()
    {
        await RunActionAsync(async () =>
        {
            if (!TryReadGuid(selectedPlayerId, "Select a player first.", out var playerId))
            {
                return;
            }

            var preset = characterPresets.Single(existing => existing.Name == selectedCharacterPresetName);
            var createCharacterResult = await CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
                playerId,
                characterName,
                characterRegion,
                characterRealm,
                preset.CharacterClass,
                preset.Specialization,
                preset.Role));

            if (createCharacterResult.IsFailure)
            {
                ShowFailure(createCharacterResult.Failure!);
                return;
            }

            var character = GetSuccessfulValue(createCharacterResult);
            var setMainResult = await SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(playerId, character.Id));
            if (setMainResult.IsFailure)
            {
                ShowFailure(setMainResult.Failure!);
                await RefreshDataAsync();
                return;
            }

            statusMessage = $"Created {character.Name} and set it as main.";
            statusKind = "success";
            await RefreshDataAsync();
        });
    }

    private async Task CreateRaidTeam()
    {
        await RunActionAsync(async () =>
        {
            if (!TryReadGuid(selectedGuildId, "Select a guild first.", out var guildId))
            {
                return;
            }

            await RunResultActionAsync(
                () => CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guildId, raidTeamName)),
                raidTeam => $"Created raid team {raidTeam.Name}.",
                raidTeam => selectedRaidTeamId = raidTeam.Id.ToString());
        });
    }

    private async Task AddPlayerToGuild()
    {
        await RunActionAsync(async () =>
        {
            if (!TryReadGuid(selectedGuildId, "Select a guild first.", out var guildId)
                || !TryReadGuid(selectedPlayerId, "Select a player first.", out var playerId))
            {
                return;
            }

            await RunResultActionAsync(
                () => AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guildId, playerId, GuildRank.Member)),
                "Added player to guild.");
        });
    }

    private async Task AddPlayerToRaidTeam()
    {
        await RunActionAsync(async () =>
        {
            if (!TryReadGuid(selectedGuildId, "Select a guild first.", out var guildId)
                || !TryReadGuid(selectedRaidTeamId, "Select a raid team first.", out var raidTeamId)
                || !TryReadGuid(selectedPlayerId, "Select a player first.", out var playerId))
            {
                return;
            }

            await RunResultActionAsync(
                () => AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guildId, raidTeamId, playerId)),
                "Added player to raid team.");
        });
    }

}

