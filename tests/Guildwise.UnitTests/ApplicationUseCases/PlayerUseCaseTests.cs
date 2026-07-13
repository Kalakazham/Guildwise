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

public sealed class PlayerUseCaseTests
{
    [Fact]
    public async Task UpdateUnknownPlayer_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.UpdatePlayerHandler.HandleAsync(new UpdatePlayerCommand(Guid.NewGuid(), "Myrmi"));

        AssertFailure(result, FailureType.NotFound, "Player");
    }

    [Fact]
    public async Task CreateAndUpdatePlayer_Works_Through_Application_Handler()
    {
        var context = new TestContext();

        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));
        var updated = AssertSuccess(await context.UpdatePlayerHandler.HandleAsync(new UpdatePlayerCommand(player.Id, "Myrmi Two")));

        Assert.Equal("Myrmi Two", updated.DisplayName);
        Assert.Equal("Myrmi Two", (await context.GetPlayerHandler.HandleAsync(new GetPlayerQuery(player.Id)))?.DisplayName);
    }

    [Fact]
    public async Task DeletePlayer_When_Player_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.DeletePlayerHandler.HandleAsync(new DeletePlayerCommand(Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "Player");
    }

    [Fact]
    public async Task DeletePlayer_When_Player_Is_Missing_Does_Not_Start_Transaction()
    {
        var context = new TestContext();

        await context.DeletePlayerHandler.HandleAsync(new DeletePlayerCommand(Guid.NewGuid()));

        Assert.Equal(0, context.TransactionRunner.ExecuteCalls);
    }

    [Fact]
    public async Task CreatePlayer_With_Blank_DisplayName_Returns_Validation()
    {
        var context = new TestContext();

        var result = await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand(" "));

        AssertFailure(result, FailureType.Validation, "display name");
    }

    [Fact]
    public async Task UpdatePlayer_With_Blank_DisplayName_Returns_Validation()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var result = await context.UpdatePlayerHandler.HandleAsync(new UpdatePlayerCommand(player.Id, " "));

        AssertFailure(result, FailureType.Validation, "display name");
    }
}
