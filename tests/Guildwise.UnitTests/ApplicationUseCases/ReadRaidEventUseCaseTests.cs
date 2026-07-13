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

public sealed class ReadRaidEventUseCaseTests
{
    [Fact]
    public async Task GetRaidEvent_Returns_Dto_For_Existing_Event()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");

        var loaded = await context.GetRaidEventHandler.HandleAsync(new GetRaidEventQuery(raidEvent.Id));

        Assert.NotNull(loaded);
        Assert.Equal(raidEvent.Id, loaded.Id);
        Assert.Equal("Raid Night", loaded.Title);
        Assert.Equal(RaidEventStatus.Scheduled, loaded.Status);
    }

    [Fact]
    public async Task GetRaidEvent_When_Missing_Returns_Null()
    {
        var context = new TestContext();

        var loaded = await context.GetRaidEventHandler.HandleAsync(new GetRaidEventQuery(Guid.NewGuid()));

        Assert.Null(loaded);
    }

    [Fact]
    public async Task ListRaidEvents_Returns_Stored_Events()
    {
        var context = new TestContext();
        await context.CreateReadyRaidEventAsync("Team One", "First");
        await context.CreateReadyRaidEventAsync("Team Two", "Second");

        var raidEvents = await context.ListRaidEventsHandler.HandleAsync(new ListRaidEventsQuery());

        Assert.Equal(2, raidEvents.Count);
        Assert.Contains(raidEvents, raidEvent => raidEvent.Title == "First");
        Assert.Contains(raidEvents, raidEvent => raidEvent.Title == "Second");
        Assert.All(raidEvents, raidEvent => Assert.Equal(RaidEventStatus.Scheduled, raidEvent.Status));
    }

    [Fact]
    public async Task ListRaidEvents_Respects_Guild_And_RaidTeam_Filters()
    {
        var context = new TestContext();
        var first = await context.CreateReadyRaidEventAsync("Team One", "First");
        var second = await context.CreateReadyRaidEventAsync("Team Two", "Second");

        var byGuild = await context.ListRaidEventsHandler.HandleAsync(new ListRaidEventsQuery(first.GuildId));
        var byRaidTeam = await context.ListRaidEventsHandler.HandleAsync(new ListRaidEventsQuery(null, second.RaidTeamId));
        var byGuildAndRaidTeam = await context.ListRaidEventsHandler.HandleAsync(new ListRaidEventsQuery(first.GuildId, second.RaidTeamId));

        Assert.Single(byGuild);
        Assert.Equal(first.Id, byGuild.Single().Id);
        Assert.Single(byRaidTeam);
        Assert.Equal(second.Id, byRaidTeam.Single().Id);
        Assert.Empty(byGuildAndRaidTeam);
    }
}
