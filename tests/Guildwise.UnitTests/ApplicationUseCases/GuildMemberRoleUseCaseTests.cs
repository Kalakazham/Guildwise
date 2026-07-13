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

public sealed class GuildMemberRoleUseCaseTests
{
    [Fact]
    public async Task GuildMember_Roles_Are_Managed_Through_Application_Handler()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        AssertSuccess(await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Officer)));
        var added = AssertSuccess(await context.AddAdditionalRoleHandler.HandleAsync(new AddAdditionalRoleToGuildMemberCommand(
            guild.Id,
            player.Id,
            AdditionalGuildRole.RaidLead)));
        var removed = AssertSuccess(await context.RemoveAdditionalRoleHandler.HandleAsync(new RemoveAdditionalRoleFromGuildMemberCommand(
            guild.Id,
            player.Id,
            AdditionalGuildRole.RaidLead)));

        Assert.Single(added.AdditionalRoles);
        Assert.Empty(removed.AdditionalRoles);
    }

    [Fact]
    public async Task AddAdditionalRole_When_Role_Is_Already_Assigned_Returns_Conflict()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));
        AssertSuccess(await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member)));
        AssertSuccess(await context.AddAdditionalRoleHandler.HandleAsync(new AddAdditionalRoleToGuildMemberCommand(
            guild.Id,
            player.Id,
            AdditionalGuildRole.RaidLead)));

        var result = await context.AddAdditionalRoleHandler.HandleAsync(new AddAdditionalRoleToGuildMemberCommand(
            guild.Id,
            player.Id,
            AdditionalGuildRole.RaidLead));

        AssertFailure(result, FailureType.Conflict, "duplicate");
    }

    [Fact]
    public async Task AddAdditionalRole_When_Guild_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.AddAdditionalRoleHandler.HandleAsync(new AddAdditionalRoleToGuildMemberCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AdditionalGuildRole.RaidLead));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Fact]
    public async Task AddAdditionalRole_When_GuildMember_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        var result = await context.AddAdditionalRoleHandler.HandleAsync(new AddAdditionalRoleToGuildMemberCommand(
            guild.Id,
            Guid.NewGuid(),
            AdditionalGuildRole.RaidLead));

        AssertFailure(result, FailureType.NotFound, "GuildMember");
    }

    [Fact]
    public async Task AddAdditionalRole_With_Invalid_Role_Returns_Validation()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));
        AssertSuccess(await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member)));

        var result = await context.AddAdditionalRoleHandler.HandleAsync(new AddAdditionalRoleToGuildMemberCommand(
            guild.Id,
            player.Id,
            (AdditionalGuildRole)999));

        AssertFailure(result, FailureType.Validation, "role");
    }

    [Fact]
    public async Task RemoveAdditionalRole_When_Guild_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.RemoveAdditionalRoleHandler.HandleAsync(new RemoveAdditionalRoleFromGuildMemberCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AdditionalGuildRole.RaidLead));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Fact]
    public async Task RemoveAdditionalRole_When_GuildMember_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        var result = await context.RemoveAdditionalRoleHandler.HandleAsync(new RemoveAdditionalRoleFromGuildMemberCommand(
            guild.Id,
            Guid.NewGuid(),
            AdditionalGuildRole.RaidLead));

        AssertFailure(result, FailureType.NotFound, "GuildMember");
    }

    [Fact]
    public async Task RemoveAdditionalRole_With_Invalid_Role_Returns_Validation()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));
        AssertSuccess(await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member)));

        var result = await context.RemoveAdditionalRoleHandler.HandleAsync(new RemoveAdditionalRoleFromGuildMemberCommand(
            guild.Id,
            player.Id,
            (AdditionalGuildRole)999));

        AssertFailure(result, FailureType.Validation, "role");
    }
}
