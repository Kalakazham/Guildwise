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

public sealed class CreateRaidEventUseCaseTests
{
    [Fact]
    public async Task CreateRaidEvent_Stores_Event_And_Returns_Dto()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));
        var startTime = new DateTimeOffset(2026, 7, 13, 20, 30, 0, TimeSpan.FromHours(2));
        var endTime = startTime.AddHours(3);

        var raidEvent = AssertSuccess(await context.CreateRaidEventHandler.HandleAsync(new CreateRaidEventCommand(
            guild.Id,
            raidTeam.Id,
            " Liberation of Undermine ",
            startTime,
            endTime,
            " Liberation of Undermine ",
            RaidDifficulty.Heroic,
            "  Bring flasks. ")));

        Assert.Equal(guild.Id, raidEvent.GuildId);
        Assert.Equal(raidTeam.Id, raidEvent.RaidTeamId);
        Assert.Equal("Liberation of Undermine", raidEvent.Title);
        Assert.Equal(TimeSpan.Zero, raidEvent.StartTime.Offset);
        Assert.Equal(startTime.ToUniversalTime(), raidEvent.StartTime);
        Assert.NotNull(raidEvent.EndTime);
        Assert.Equal(TimeSpan.Zero, raidEvent.EndTime.Value.Offset);
        Assert.Equal(endTime.ToUniversalTime(), raidEvent.EndTime.Value);
        Assert.Equal("Liberation of Undermine", raidEvent.InstanceName);
        Assert.Equal(RaidDifficulty.Heroic, raidEvent.Difficulty);
        Assert.Equal(RaidEventStatus.Scheduled, raidEvent.Status);
        Assert.Equal("Bring flasks.", raidEvent.Notes);

        var stored = await context.RaidEventRepository.GetByIdAsync(raidEvent.Id);
        Assert.NotNull(stored);
        Assert.Equal(raidEvent.Id, stored.Id);
        Assert.Equal(TimeSpan.Zero, stored.StartTime.Offset);
        Assert.Equal(startTime.ToUniversalTime(), stored.StartTime);
        Assert.Equal(RaidEventStatus.Scheduled, stored.Status);
    }

    [Fact]
    public async Task CreateRaidEvent_When_Guild_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.CreateRaidEventHandler.HandleAsync(new CreateRaidEventCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Fact]
    public async Task CreateRaidEvent_When_RaidTeam_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        var result = await context.CreateRaidEventHandler.HandleAsync(new CreateRaidEventCommand(
            guild.Id,
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));

        AssertFailure(result, FailureType.NotFound, "RaidTeam");
    }

    [Fact]
    public async Task CreateRaidEvent_When_RaidTeam_Belongs_To_Another_Guild_Returns_NotFound()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var otherGuild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Other", "EU", "Silvermoon")));
        var otherRaidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(otherGuild.Id, "Team One")));

        var result = await context.CreateRaidEventHandler.HandleAsync(new CreateRaidEventCommand(
            guild.Id,
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
    public async Task CreateRaidEvent_With_Invalid_Title_Returns_Validation()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));

        var result = await context.CreateRaidEventHandler.HandleAsync(new CreateRaidEventCommand(
            guild.Id,
            raidTeam.Id,
            " ",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));

        AssertFailure(result, FailureType.Validation, "title");
    }

    [Fact]
    public async Task CreateRaidEvent_With_Invalid_TimeRange_Returns_Validation()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));
        var startTime = DateTimeOffset.UtcNow.AddDays(1);

        var result = await context.CreateRaidEventHandler.HandleAsync(new CreateRaidEventCommand(
            guild.Id,
            raidTeam.Id,
            "Raid Night",
            startTime,
            startTime,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));

        AssertFailure(result, FailureType.Validation, "after");
    }
}
