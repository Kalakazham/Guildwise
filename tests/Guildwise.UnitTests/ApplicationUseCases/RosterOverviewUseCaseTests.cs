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
public sealed class RosterOverviewUseCaseTests
{
    [Fact]
    public async Task GetRosterOverview_When_No_Data_Exists_Returns_Empty_Collections()
    {
        var context = new TestContext();

        var overview = await context.GetRosterOverviewHandler.HandleAsync(new GetRosterOverviewQuery());

        Assert.Equal(0, overview.Summary.GuildCount);
        Assert.Equal(0, overview.Summary.PlayerCount);
        Assert.Empty(overview.Guilds);
        Assert.Empty(overview.Members);
    }

    [Fact]
    public async Task GetRosterOverview_Returns_Summary_And_Roster_Members()
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
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage)));

        AssertSuccess(await context.SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id)));
        AssertSuccess(await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member)));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));
        AssertSuccess(await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id)));

        var overview = await context.GetRosterOverviewHandler.HandleAsync(new GetRosterOverviewQuery());

        Assert.Equal(1, overview.Summary.GuildCount);
        Assert.Equal(1, overview.Summary.PlayerCount);
        Assert.Equal(1, overview.Summary.CharacterCount);
        Assert.Equal(1, overview.Summary.RaidTeamCount);
        Assert.Equal(1, overview.Summary.GuildMemberCount);
        Assert.Equal(1, overview.Summary.RaidRosterMemberCount);
        Assert.Equal(1, overview.Summary.PlayersWithMainCharacterCount);

        var guildSummary = Assert.Single(overview.Guilds);
        Assert.Equal("Guildwise", guildSummary.Name);
        Assert.Equal(1, guildSummary.RaidTeamCount);
        Assert.Equal(1, guildSummary.MemberCount);

        var member = Assert.Single(overview.Members);
        Assert.Equal(player.Id, member.PlayerId);
        Assert.Equal("Myrmi", member.PlayerDisplayName);
        Assert.Equal(character.Id, member.MainCharacterId);
        Assert.Equal("Alysa", member.MainCharacterName);
        Assert.Equal(CharacterClass.Paladin, member.CharacterClass);
        Assert.Equal(CharacterRole.Damage, member.Role);
        Assert.True(member.HasMainCharacter);
        Assert.True(member.IsGuildMember);
        Assert.Equal(GuildRank.Member, member.GuildRank);
        Assert.Equal("Team One", Assert.Single(member.RaidTeamNames));
    }
}
