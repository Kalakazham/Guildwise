using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Characters.CreateCharacter;
using Guildwise.Application.Characters.DeleteCharacter;
using Guildwise.Application.Characters.GetCharacter;
using Guildwise.Application.Characters.ListCharacters;
using Guildwise.Application.Characters.ListCharactersForPlayer;
using Guildwise.Application.Characters.SetMainCharacter;
using Guildwise.Application.Characters.UpdateCharacter;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.Characters;
using Guildwise.Application.Contracts.GuildMembers;
using Guildwise.Application.Contracts.Guilds;
using Guildwise.Application.Contracts.Players;
using Guildwise.Application.Contracts.RaidEvents;
using Guildwise.Application.Contracts.RaidTeams;
using Guildwise.Application.GuildMembers.AddAdditionalRoleToGuildMember;
using Guildwise.Application.GuildMembers.AddPlayerToGuild;
using Guildwise.Application.GuildMembers.RemoveAdditionalRoleFromGuildMember;
using Guildwise.Application.Guilds.CreateGuild;
using Guildwise.Application.Guilds.DeleteGuild;
using Guildwise.Application.Guilds.GetGuild;
using Guildwise.Application.Guilds.ListGuilds;
using Guildwise.Application.Guilds.UpdateGuild;
using Guildwise.Application.Players.CreatePlayer;
using Guildwise.Application.Players.DeletePlayer;
using Guildwise.Application.Players.GetPlayer;
using Guildwise.Application.Players.ListPlayers;
using Guildwise.Application.Players.UpdatePlayer;
using Guildwise.Application.RaidEvents.CancelRaidEvent;
using Guildwise.Application.RaidEvents.CreateRaidEvent;
using Guildwise.Application.RaidEvents.GetRaidEvent;
using Guildwise.Application.RaidEvents.ListRaidEventSignups;
using Guildwise.Application.RaidEvents.ListRaidEvents;
using Guildwise.Application.RaidEvents.SetRaidEventSignup;
using Guildwise.Application.RaidEvents.UpdateRaidEvent;
using Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;
using Guildwise.Application.RaidTeams.CreateRaidTeam;
using Guildwise.Application.RaidTeams.DeleteRaidTeam;
using Guildwise.Application.RaidTeams.GetRaidTeam;
using Guildwise.Application.RaidTeams.ListRaidTeamsForGuild;
using Guildwise.Application.RaidTeams.RemovePlayerFromRaidTeam;
using Guildwise.Application.RaidTeams.UpdateRaidTeam;
using Guildwise.Application.RaidTeamManagement.GetRaidTeamManagementOverview;
using Guildwise.Application.RosterOverview.GetRosterOverview;
using Guildwise.Domain;
using static Guildwise.UnitTests.ApplicationUseCaseTestSupport;
namespace Guildwise.UnitTests;
public sealed class RaidTeamManagementOverviewUseCaseTests
{
    [Fact]
    public async Task GetRaidTeamManagementOverview_When_No_Data_Exists_Returns_Empty_Collections()
    {
        var context = new TestContext();

        var overview = await context.GetRaidTeamManagementOverviewHandler.HandleAsync(new GetRaidTeamManagementOverviewQuery());

        Assert.Empty(overview.Guilds);
    }

    [Fact]
    public async Task GetRaidTeamManagementOverview_Returns_Guild_Context_When_Guild_Has_No_RaidTeams()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        var overview = await context.GetRaidTeamManagementOverviewHandler.HandleAsync(new GetRaidTeamManagementOverviewQuery());

        var guildOverview = Assert.Single(overview.Guilds);
        Assert.Equal(guild.Id, guildOverview.Id);
        Assert.Equal("Guildwise", guildOverview.Name);
        Assert.Equal("EU", guildOverview.Region);
        Assert.Equal("Draenor", guildOverview.Realm);
        Assert.Equal(0, guildOverview.RaidTeamCount);
        Assert.Empty(guildOverview.AvailablePlayers);
        Assert.Empty(guildOverview.Teams);
    }

    [Fact]
    public async Task GetRaidTeamManagementOverview_Returns_Team_Members_And_Role_Composition()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));
        var character = AssertSuccess(await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinProtection,
            CharacterRole.Tank)));

        AssertSuccess(await context.SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id)));
        AssertSuccess(await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member)));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));
        AssertSuccess(await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id)));

        var overview = await context.GetRaidTeamManagementOverviewHandler.HandleAsync(new GetRaidTeamManagementOverviewQuery());

        var guildOverview = Assert.Single(overview.Guilds);
        Assert.Equal(1, guildOverview.RaidMemberCount);
        Assert.Equal(0, guildOverview.UnassignedGuildMemberCount);
        Assert.Equal(0, guildOverview.PlayersWithoutMainCharacterCount);

        var team = Assert.Single(guildOverview.Teams);
        Assert.Equal("Team One", team.Name);
        Assert.Equal(1, team.MemberCount);
        Assert.Equal(1, team.Composition.TankCount);
        Assert.Equal(0, team.Composition.HealerCount);
        Assert.Equal(0, team.Composition.DamageCount);

        var member = Assert.Single(team.Members);
        Assert.Equal(player.Id, member.PlayerId);
        Assert.Equal("Myrmi", member.PlayerDisplayName);
        Assert.Equal(character.Id, member.MainCharacterId);
        Assert.Equal("Alysa", member.MainCharacterName);
        Assert.Equal(CharacterClass.Paladin, member.CharacterClass);
        Assert.Equal(CharacterRole.Tank, member.Role);
        Assert.True(member.HasMainCharacter);
        Assert.Equal(GuildRank.Member, member.GuildRank);

        var availablePlayer = Assert.Single(guildOverview.AvailablePlayers);
        Assert.Equal(player.Id, availablePlayer.PlayerId);
        Assert.Equal("Myrmi", availablePlayer.PlayerDisplayName);
        Assert.True(availablePlayer.HasMainCharacter);
        Assert.Equal(raidTeam.Id, Assert.Single(availablePlayer.RaidTeamIds));
        Assert.Equal("Team One", Assert.Single(availablePlayer.RaidTeamNames));
    }

    [Fact]
    public async Task GetRaidTeamManagementOverview_Counts_Guild_Members_Without_RaidTeam_As_Unassigned()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var assignedPlayer = await context.CreateReadyGuildMemberAsync(guild.Id, "Assigned", CharacterRole.Damage);
        _ = await context.CreateReadyGuildMemberAsync(guild.Id, "Bench", CharacterRole.Healer);
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));
        AssertSuccess(await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, assignedPlayer.PlayerId)));

        var overview = await context.GetRaidTeamManagementOverviewHandler.HandleAsync(new GetRaidTeamManagementOverviewQuery());

        var guildOverview = Assert.Single(overview.Guilds);
        Assert.Equal(1, guildOverview.RaidMemberCount);
        Assert.Equal(1, guildOverview.UnassignedGuildMemberCount);
        Assert.Equal(2, guildOverview.AvailablePlayers.Count);

        var assigned = guildOverview.AvailablePlayers.Single(player => player.PlayerDisplayName == "Assigned");
        Assert.Equal("Team One", Assert.Single(assigned.RaidTeamNames));

        var bench = guildOverview.AvailablePlayers.Single(player => player.PlayerDisplayName == "Bench");
        Assert.Empty(bench.RaidTeamIds);
        Assert.Empty(bench.RaidTeamNames);
    }

    [Fact]
    public async Task GetRaidTeamManagementOverview_Shows_RaidTeam_Member_Without_Main_And_Excludes_From_Composition()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var memberSetup = await context.CreateReadyGuildMemberAsync(guild.Id, "Myrmi", CharacterRole.Damage);
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));
        AssertSuccess(await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, memberSetup.PlayerId)));

        var player = await context.PlayerRepository.GetByIdAsync(memberSetup.PlayerId);
        player!.RemoveCharacter(memberSetup.CharacterId);

        var overview = await context.GetRaidTeamManagementOverviewHandler.HandleAsync(new GetRaidTeamManagementOverviewQuery());

        var guildOverview = Assert.Single(overview.Guilds);
        Assert.Equal(1, guildOverview.PlayersWithoutMainCharacterCount);

        var availablePlayer = Assert.Single(guildOverview.AvailablePlayers);
        Assert.Equal(memberSetup.PlayerId, availablePlayer.PlayerId);
        Assert.False(availablePlayer.HasMainCharacter);
        Assert.Null(availablePlayer.MainCharacterId);
        Assert.Equal("Team One", Assert.Single(availablePlayer.RaidTeamNames));

        var team = Assert.Single(guildOverview.Teams);
        Assert.Equal(0, team.Composition.TankCount);
        Assert.Equal(0, team.Composition.HealerCount);
        Assert.Equal(0, team.Composition.DamageCount);

        var member = Assert.Single(team.Members);
        Assert.Equal(memberSetup.PlayerId, member.PlayerId);
        Assert.False(member.HasMainCharacter);
        Assert.Null(member.MainCharacterId);
        Assert.Null(member.Role);
    }
}
