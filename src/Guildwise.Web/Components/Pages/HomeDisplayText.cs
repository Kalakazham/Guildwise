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
    private static CharacterDto? GetMainCharacter(PlayerDto? player)
        => player is null || !player.MainCharacterId.HasValue
            ? null
            : player.Characters.FirstOrDefault(character => character.Id == player.MainCharacterId);

    private static string FormatCharacter(CharacterDto? character)
        => character is null
            ? "No main character"
            : $"{character.Name} - {character.CharacterClass} {character.Specialization}";

    private sealed record CharacterPreset(
        string Name,
        CharacterClass CharacterClass,
        CharacterSpecialization Specialization,
        CharacterRole Role);
}
