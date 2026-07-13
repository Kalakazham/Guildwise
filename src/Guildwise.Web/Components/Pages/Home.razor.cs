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
    private readonly IReadOnlyList<CharacterPreset> characterPresets =
    [
        new("Retribution Paladin", CharacterClass.Paladin, CharacterSpecialization.PaladinRetribution, CharacterRole.Damage),
        new("Protection Warrior", CharacterClass.Warrior, CharacterSpecialization.WarriorProtection, CharacterRole.Tank),
        new("Restoration Shaman", CharacterClass.Shaman, CharacterSpecialization.ShamanRestoration, CharacterRole.Healer),
        new("Frost Mage", CharacterClass.Mage, CharacterSpecialization.MageFrost, CharacterRole.Damage)
    ];

    private List<GuildDto> guilds = [];
    private List<PlayerDto> players = [];
    private List<RaidTeamDto> raidTeams = [];

    private string guildName = "Guildwise";
    private string guildRegion = "EU";
    private string guildRealm = "Draenor";
    private string playerName = "Myrmi";
    private string characterName = "Alysa";
    private string characterRegion = "EU";
    private string characterRealm = "Draenor";
    private string raidTeamName = "Team One";
    private string selectedCharacterPresetName = "Retribution Paladin";
    private string? selectedGuildId;
    private string? selectedPlayerId;
    private string? selectedRaidTeamId;
    private string? statusMessage;
    private string statusKind = "info";

    protected override async Task OnInitializedAsync()
    {
        await RefreshDataAsync();
    }
}
