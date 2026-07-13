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

public sealed class UpdateRaidEventUseCaseTests
{
    [Fact]
    public async Task UpdateRaidEvent_With_Valid_Event_Returns_Updated_Dto()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");
        var newGuild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Updated Guild", "EU", "Silvermoon")));
        var newRaidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(newGuild.Id, "Team Two")));
        var startTime = new DateTimeOffset(2026, 7, 13, 20, 30, 0, TimeSpan.FromHours(2));
        var endTime = startTime.AddHours(3);

        var updated = AssertSuccess(await context.UpdateRaidEventHandler.HandleAsync(new UpdateRaidEventCommand(
            raidEvent.Id,
            newGuild.Id,
            newRaidTeam.Id,
            " Manaforge Omega ",
            startTime,
            endTime,
            " Manaforge Omega ",
            RaidDifficulty.Mythic,
            "  Bring cauldrons. ")));

        Assert.Equal(raidEvent.Id, updated.Id);
        Assert.Equal(newGuild.Id, updated.GuildId);
        Assert.Equal(newRaidTeam.Id, updated.RaidTeamId);
        Assert.Equal("Manaforge Omega", updated.Title);
        Assert.Equal(TimeSpan.Zero, updated.StartTime.Offset);
        Assert.Equal(startTime.ToUniversalTime(), updated.StartTime);
        Assert.NotNull(updated.EndTime);
        Assert.Equal(TimeSpan.Zero, updated.EndTime.Value.Offset);
        Assert.Equal(endTime.ToUniversalTime(), updated.EndTime.Value);
        Assert.Equal("Manaforge Omega", updated.InstanceName);
        Assert.Equal(RaidDifficulty.Mythic, updated.Difficulty);
        Assert.Equal(RaidEventStatus.Scheduled, updated.Status);
        Assert.Equal("Bring cauldrons.", updated.Notes);
    }

    [Fact]
    public async Task UpdateRaidEvent_When_Event_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.UpdateRaidEventHandler.HandleAsync(new UpdateRaidEventCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));

        AssertFailure(result, FailureType.NotFound, "RaidEvent");
    }

    [Fact]
    public async Task UpdateRaidEvent_When_Guild_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");

        var result = await context.UpdateRaidEventHandler.HandleAsync(new UpdateRaidEventCommand(
            raidEvent.Id,
            Guid.NewGuid(),
            raidEvent.RaidTeamId,
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Fact]
    public async Task UpdateRaidEvent_When_RaidTeam_Is_Missing_Or_Wrong_Guild_Returns_NotFound()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");
        var otherGuild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Other", "EU", "Silvermoon")));
        var otherRaidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(otherGuild.Id, "Other Team")));

        var result = await context.UpdateRaidEventHandler.HandleAsync(new UpdateRaidEventCommand(
            raidEvent.Id,
            raidEvent.GuildId,
            otherRaidTeam.Id,
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));

        AssertFailure(result, FailureType.NotFound, "RaidTeam");
    }

    [Fact]
    public async Task UpdateRaidEvent_When_Event_Is_Cancelled_Returns_BusinessRule()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");
        AssertSuccess(await context.CancelRaidEventHandler.HandleAsync(new CancelRaidEventCommand(raidEvent.Id)));

        var result = await context.UpdateRaidEventHandler.HandleAsync(new UpdateRaidEventCommand(
            raidEvent.Id,
            raidEvent.GuildId,
            raidEvent.RaidTeamId,
            "Updated",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));

        AssertFailure(result, FailureType.BusinessRule, "Cancelled");
    }

    [Fact]
    public async Task UpdateRaidEvent_With_Invalid_TimeRange_Returns_Validation()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");
        var startTime = DateTimeOffset.UtcNow.AddDays(1);

        var result = await context.UpdateRaidEventHandler.HandleAsync(new UpdateRaidEventCommand(
            raidEvent.Id,
            raidEvent.GuildId,
            raidEvent.RaidTeamId,
            "Updated",
            startTime,
            startTime,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));

        AssertFailure(result, FailureType.Validation, "after");
    }
}
