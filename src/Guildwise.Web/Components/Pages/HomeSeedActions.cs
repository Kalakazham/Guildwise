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
    private async Task SeedSampleRoster()
    {
        await RunActionAsync(async () =>
        {
            var suffix = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var createGuildResult = await CreateGuildHandler.HandleAsync(new CreateGuildCommand($"Guildwise {suffix}", "EU", "Draenor"));
            if (createGuildResult.IsFailure)
            {
                ShowFailure(createGuildResult.Failure!);
                return;
            }

            var guild = GetSuccessfulValue(createGuildResult);
            var createPlayerResult = await CreatePlayerHandler.HandleAsync(new CreatePlayerCommand($"Myrmi {suffix}"));
            if (createPlayerResult.IsFailure)
            {
                ShowFailure(createPlayerResult.Failure!);
                await RefreshDataAsync();
                return;
            }

            var player = GetSuccessfulValue(createPlayerResult);
            var createCharacterResult = await CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
                player.Id,
                $"Alysa{suffix[^4..]}",
                "EU",
                "Draenor",
                CharacterClass.Paladin,
                CharacterSpecialization.PaladinRetribution,
                CharacterRole.Damage));

            if (createCharacterResult.IsFailure)
            {
                ShowFailure(createCharacterResult.Failure!);
                await RefreshDataAsync();
                return;
            }

            var character = GetSuccessfulValue(createCharacterResult);
            var setMainResult = await SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id));
            if (setMainResult.IsFailure)
            {
                ShowFailure(setMainResult.Failure!);
                await RefreshDataAsync();
                return;
            }

            var createRaidTeamResult = await CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One"));
            if (createRaidTeamResult.IsFailure)
            {
                ShowFailure(createRaidTeamResult.Failure!);
                await RefreshDataAsync();
                return;
            }

            var raidTeam = GetSuccessfulValue(createRaidTeamResult);
            var addToGuildResult = await AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member));
            if (addToGuildResult.IsFailure)
            {
                ShowFailure(addToGuildResult.Failure!);
                await RefreshDataAsync();
                return;
            }

            var addToRaidTeamResult = await AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id));
            if (addToRaidTeamResult.IsFailure)
            {
                ShowFailure(addToRaidTeamResult.Failure!);
                await RefreshDataAsync();
                return;
            }

            selectedGuildId = guild.Id.ToString();
            selectedPlayerId = player.Id.ToString();
            selectedRaidTeamId = raidTeam.Id.ToString();
            statusMessage = "Seeded a complete roster flow.";
            statusKind = "success";
            await RefreshDataAsync();
        });
    }

}

