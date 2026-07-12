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
public sealed class GuildUseCaseTests
{
    [Fact]
    public async Task CreateGuild_Stores_Guild_And_Returns_Dto()
    {
        var context = new TestContext();

        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        Assert.Equal("Guildwise", guild.Name);
        Assert.Single(await context.GuildRepository.ListAsync());
        Assert.Equal(guild.Id, (await context.GetGuildHandler.HandleAsync(new GetGuildQuery(guild.Id)))?.Id);
    }

    [Fact]
    public async Task DeleteGuild_Removes_Guild()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        AssertSuccess(await context.DeleteGuildHandler.HandleAsync(new DeleteGuildCommand(guild.Id)));

        Assert.Null(await context.GetGuildHandler.HandleAsync(new GetGuildQuery(guild.Id)));
        Assert.Empty(await context.ListGuildsHandler.HandleAsync(new ListGuildsQuery()));
    }

    [Fact]
    public async Task CreateGuild_With_Blank_Name_Returns_Validation()
    {
        var context = new TestContext();

        var result = await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand(" ", "EU", "Draenor"));

        AssertFailure(result, FailureType.Validation, "name");
    }

    [Fact]
    public async Task DeleteGuild_When_Guild_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.DeleteGuildHandler.HandleAsync(new DeleteGuildCommand(Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Theory]
    [InlineData("", "EU", "Draenor", "name")]
    [InlineData("Guildwise", "", "Draenor", "region")]
    [InlineData("Guildwise", "EU", "", "realm")]
    public async Task CreateGuild_With_Blank_Values_Returns_Validation(
        string name,
        string region,
        string realm,
        string expectedMessage)
    {
        var context = new TestContext();

        var result = await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand(name, region, realm));

        AssertFailure(result, FailureType.Validation, expectedMessage);
    }

    [Fact]
    public async Task UpdateGuild_When_Guild_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.UpdateGuildHandler.HandleAsync(new UpdateGuildCommand(
            Guid.NewGuid(),
            "Guildwise",
            "EU",
            "Draenor"));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Fact]
    public async Task UpdateGuild_With_Blank_Region_Returns_Validation()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        var result = await context.UpdateGuildHandler.HandleAsync(new UpdateGuildCommand(
            guild.Id,
            "Guildwise",
            " ",
            "Draenor"));

        AssertFailure(result, FailureType.Validation, "region");
    }
}
