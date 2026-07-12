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

public sealed class CancelRaidEventUseCaseTests
{
    [Fact]
    public async Task CancelRaidEvent_With_Valid_Event_Returns_Cancelled_Dto()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");

        var cancelled = AssertSuccess(await context.CancelRaidEventHandler.HandleAsync(new CancelRaidEventCommand(raidEvent.Id)));

        Assert.Equal(raidEvent.Id, cancelled.Id);
        Assert.Equal(RaidEventStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public async Task CancelRaidEvent_When_Event_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.CancelRaidEventHandler.HandleAsync(new CancelRaidEventCommand(Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "RaidEvent");
    }

    [Fact]
    public async Task CancelRaidEvent_When_Already_Cancelled_Returns_Success()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");
        AssertSuccess(await context.CancelRaidEventHandler.HandleAsync(new CancelRaidEventCommand(raidEvent.Id)));

        var secondCancel = AssertSuccess(await context.CancelRaidEventHandler.HandleAsync(new CancelRaidEventCommand(raidEvent.Id)));

        Assert.Equal(raidEvent.Id, secondCancel.Id);
        Assert.Equal(RaidEventStatus.Cancelled, secondCancel.Status);
    }
}

