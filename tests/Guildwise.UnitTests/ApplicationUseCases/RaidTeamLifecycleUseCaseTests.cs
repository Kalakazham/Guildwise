using Guildwise.Application.Common.Results;
using Guildwise.Application.Guilds.CreateGuild;
using Guildwise.Application.RaidTeams.CreateRaidTeam;
using Guildwise.Application.RaidTeams.DeleteRaidTeam;
using Guildwise.Application.RaidTeams.ListRaidTeamsForGuild;
using Guildwise.Application.RaidTeams.UpdateRaidTeam;
using static Guildwise.UnitTests.ApplicationUseCaseTestSupport;

namespace Guildwise.UnitTests;

public sealed class RaidTeamLifecycleUseCaseTests
{
    [Fact]
    public async Task CreateRaidTeamForUnknownGuild_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(
            Guid.NewGuid(),
            "Team One"));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Fact]
    public async Task CreateRaidTeam_With_Blank_Name_Returns_Validation()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        var result = await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, " "));

        AssertFailure(result, FailureType.Validation, "name");
    }

    [Fact]
    public async Task CreateRaidTeam_With_Duplicate_Name_Returns_Conflict()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));

        var result = await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, " team one "));

        AssertFailure(result, FailureType.Conflict, "raid team");
    }

    [Fact]
    public async Task RenameRaidTeam_And_DeleteRaidTeam_Work_Through_Guild()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));

        var renamed = AssertSuccess(await context.UpdateRaidTeamHandler.HandleAsync(new UpdateRaidTeamCommand(guild.Id, raidTeam.Id, "Team Two")));
        AssertSuccess(await context.DeleteRaidTeamHandler.HandleAsync(new DeleteRaidTeamCommand(guild.Id, raidTeam.Id)));

        Assert.Equal("Team Two", renamed.Name);
        Assert.Empty(await context.ListRaidTeamsForGuildHandler.HandleAsync(new ListRaidTeamsForGuildQuery(guild.Id)));
    }

    [Fact]
    public async Task UpdateRaidTeam_When_Guild_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.UpdateRaidTeamHandler.HandleAsync(new UpdateRaidTeamCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Team Two"));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Fact]
    public async Task UpdateRaidTeam_When_RaidTeam_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        var result = await context.UpdateRaidTeamHandler.HandleAsync(new UpdateRaidTeamCommand(
            guild.Id,
            Guid.NewGuid(),
            "Team Two"));

        AssertFailure(result, FailureType.NotFound, "RaidTeam");
    }

    [Fact]
    public async Task UpdateRaidTeam_With_Blank_Name_Returns_Validation()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));

        var result = await context.UpdateRaidTeamHandler.HandleAsync(new UpdateRaidTeamCommand(
            guild.Id,
            raidTeam.Id,
            " "));

        AssertFailure(result, FailureType.Validation, "name");
    }

    [Fact]
    public async Task UpdateRaidTeam_With_Duplicate_Name_Returns_Conflict()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team Two")));

        var result = await context.UpdateRaidTeamHandler.HandleAsync(new UpdateRaidTeamCommand(
            guild.Id,
            raidTeam.Id,
            " team one "));

        AssertFailure(result, FailureType.Conflict, "unique");
    }

    [Fact]
    public async Task DeleteRaidTeam_When_Guild_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.DeleteRaidTeamHandler.HandleAsync(new DeleteRaidTeamCommand(Guid.NewGuid(), Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Fact]
    public async Task DeleteRaidTeam_When_RaidTeam_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        var result = await context.DeleteRaidTeamHandler.HandleAsync(new DeleteRaidTeamCommand(guild.Id, Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "RaidTeam");
    }
}
