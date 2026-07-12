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
public sealed class RaidEventSignupUseCaseTests
{
    [Fact]
    public async Task SetRaidEventSignup_For_Scheduled_Event_And_GuildMember_Succeeds()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");
        var member = await context.CreateReadyGuildMemberAsync(raidEvent.GuildId, "Myrmi", CharacterRole.Damage);

        var signup = AssertSuccess(await context.SetRaidEventSignupHandler.HandleAsync(new SetRaidEventSignupCommand(
            raidEvent.Id,
            member.PlayerId,
            RaidEventSignupStatus.Signed)));

        Assert.Equal(raidEvent.Id, signup.RaidEventId);
        Assert.Equal(member.PlayerId, signup.PlayerId);
        Assert.Equal("Myrmi", signup.PlayerDisplayName);
        Assert.Equal(RaidEventSignupStatus.Signed, signup.Status);
        Assert.Equal(member.CharacterId, signup.MainCharacterId);
        Assert.Equal("Myrmimain", signup.MainCharacterName);
        Assert.Equal(CharacterClass.Paladin, signup.CharacterClass);
        Assert.Equal(CharacterSpecialization.PaladinRetribution, signup.Specialization);
        Assert.Equal(CharacterRole.Damage, signup.Role);
        Assert.True(signup.HasMainCharacter);
        Assert.Equal(GuildRank.Member, signup.GuildRank);
        Assert.Empty(signup.AdditionalRoles);
    }

    [Fact]
    public async Task SetRaidEventSignup_Updates_Existing_Status_For_Same_Player()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");
        var member = await context.CreateReadyGuildMemberAsync(raidEvent.GuildId, "Myrmi", CharacterRole.Damage);
        AssertSuccess(await context.SetRaidEventSignupHandler.HandleAsync(new SetRaidEventSignupCommand(
            raidEvent.Id,
            member.PlayerId,
            RaidEventSignupStatus.Signed)));

        var updated = AssertSuccess(await context.SetRaidEventSignupHandler.HandleAsync(new SetRaidEventSignupCommand(
            raidEvent.Id,
            member.PlayerId,
            RaidEventSignupStatus.Declined)));

        Assert.Equal(RaidEventSignupStatus.Declined, updated.Status);
        var signups = await context.ListRaidEventSignupsHandler.HandleAsync(new ListRaidEventSignupsQuery(raidEvent.Id));
        var signup = Assert.Single(signups);
        Assert.Equal(RaidEventSignupStatus.Declined, signup.Status);
    }

    [Fact]
    public async Task SetRaidEventSignup_When_Event_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var result = await context.SetRaidEventSignupHandler.HandleAsync(new SetRaidEventSignupCommand(
            Guid.NewGuid(),
            player.Id,
            RaidEventSignupStatus.Signed));

        AssertFailure(result, FailureType.NotFound, "RaidEvent");
    }

    [Fact]
    public async Task SetRaidEventSignup_When_Player_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");

        var result = await context.SetRaidEventSignupHandler.HandleAsync(new SetRaidEventSignupCommand(
            raidEvent.Id,
            Guid.NewGuid(),
            RaidEventSignupStatus.Signed));

        AssertFailure(result, FailureType.NotFound, "Player");
    }

    [Fact]
    public async Task SetRaidEventSignup_When_Player_Is_Not_In_Event_Guild_Returns_BusinessRule()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var result = await context.SetRaidEventSignupHandler.HandleAsync(new SetRaidEventSignupCommand(
            raidEvent.Id,
            player.Id,
            RaidEventSignupStatus.Signed));

        AssertFailure(result, FailureType.BusinessRule, "guild member");
    }

    [Fact]
    public async Task SetRaidEventSignup_When_Event_Is_Cancelled_Returns_BusinessRule()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");
        var member = await context.CreateReadyGuildMemberAsync(raidEvent.GuildId, "Myrmi", CharacterRole.Damage);
        AssertSuccess(await context.CancelRaidEventHandler.HandleAsync(new CancelRaidEventCommand(raidEvent.Id)));

        var result = await context.SetRaidEventSignupHandler.HandleAsync(new SetRaidEventSignupCommand(
            raidEvent.Id,
            member.PlayerId,
            RaidEventSignupStatus.Signed));

        AssertFailure(result, FailureType.BusinessRule, "scheduled");
    }

    [Fact]
    public async Task SetRaidEventSignup_With_Invalid_Input_Returns_Validation()
    {
        var context = new TestContext();

        var missingEvent = await context.SetRaidEventSignupHandler.HandleAsync(new SetRaidEventSignupCommand(
            Guid.Empty,
            Guid.NewGuid(),
            RaidEventSignupStatus.Signed));
        var missingPlayer = await context.SetRaidEventSignupHandler.HandleAsync(new SetRaidEventSignupCommand(
            Guid.NewGuid(),
            Guid.Empty,
            RaidEventSignupStatus.Signed));
        var invalidStatus = await context.SetRaidEventSignupHandler.HandleAsync(new SetRaidEventSignupCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            RaidEventSignupStatus.Unknown));

        AssertFailure(missingEvent, FailureType.Validation, "Raid event");
        AssertFailure(missingPlayer, FailureType.Validation, "Player");
        AssertFailure(invalidStatus, FailureType.Validation, "status");
    }

    [Fact]
    public async Task ListRaidEventSignups_Returns_Dtos_With_Player_MainCharacter_And_GuildMember_Context()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");
        var member = await context.CreateReadyGuildMemberAsync(raidEvent.GuildId, "Myrmi", CharacterRole.Damage);
        AssertSuccess(await context.AddAdditionalRoleHandler.HandleAsync(new AddAdditionalRoleToGuildMemberCommand(
            raidEvent.GuildId,
            member.PlayerId,
            AdditionalGuildRole.RaidLead)));
        AssertSuccess(await context.SetRaidEventSignupHandler.HandleAsync(new SetRaidEventSignupCommand(
            raidEvent.Id,
            member.PlayerId,
            RaidEventSignupStatus.Tentative)));

        var signups = await context.ListRaidEventSignupsHandler.HandleAsync(new ListRaidEventSignupsQuery(raidEvent.Id));

        var signup = Assert.Single(signups);
        Assert.Equal(raidEvent.Id, signup.RaidEventId);
        Assert.Equal(member.PlayerId, signup.PlayerId);
        Assert.Equal("Myrmi", signup.PlayerDisplayName);
        Assert.Equal(RaidEventSignupStatus.Tentative, signup.Status);
        Assert.Equal(member.CharacterId, signup.MainCharacterId);
        Assert.Equal("Myrmimain", signup.MainCharacterName);
        Assert.Equal(CharacterClass.Paladin, signup.CharacterClass);
        Assert.Equal(CharacterSpecialization.PaladinRetribution, signup.Specialization);
        Assert.Equal(CharacterRole.Damage, signup.Role);
        Assert.True(signup.HasMainCharacter);
        Assert.Equal(GuildRank.Member, signup.GuildRank);
        Assert.Equal([AdditionalGuildRole.RaidLead], signup.AdditionalRoles);
    }

    [Fact]
    public async Task ListRaidEventSignups_When_Event_Is_Missing_Or_Has_No_Signups_Returns_Empty()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");

        var missingEventSignups = await context.ListRaidEventSignupsHandler.HandleAsync(
            new ListRaidEventSignupsQuery(Guid.NewGuid()));
        var emptyEventSignups = await context.ListRaidEventSignupsHandler.HandleAsync(
            new ListRaidEventSignupsQuery(raidEvent.Id));

        Assert.Empty(missingEventSignups);
        Assert.Empty(emptyEventSignups);
    }
}
