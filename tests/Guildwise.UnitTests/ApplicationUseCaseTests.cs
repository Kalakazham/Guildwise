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
using Guildwise.Application.RaidEvents.CreateRaidEvent;
using Guildwise.Application.RaidEvents.GetRaidEvent;
using Guildwise.Application.RaidEvents.ListRaidEvents;
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

namespace Guildwise.UnitTests;

public sealed class ApplicationUseCaseTests
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
    public async Task UpdateUnknownPlayer_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.UpdatePlayerHandler.HandleAsync(new UpdatePlayerCommand(Guid.NewGuid(), "Myrmi"));

        AssertFailure(result, FailureType.NotFound, "Player");
    }

    [Fact]
    public async Task CreateCharacterForUnknownPlayer_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            Guid.NewGuid(),
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage));

        AssertFailure(result, FailureType.NotFound, "Player");
    }

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
    public async Task AddPlayerToRaidTeam_When_Player_Is_Not_GuildMember_Fails()
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
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));

        var result = await context.AddPlayerToRaidTeamHandler.HandleAsync(
            new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id));

        AssertFailure(result, FailureType.BusinessRule, "guild member");
    }

    [Fact]
    public async Task AddPlayerToUnknownRaidTeam_Returns_NotFound()
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

        var result = await context.AddPlayerToRaidTeamHandler.HandleAsync(
            new AddPlayerToRaidTeamCommand(guild.Id, Guid.NewGuid(), player.Id));

        AssertFailure(result, FailureType.NotFound, "RaidTeam");
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
    public async Task CreateCharacter_UpdateCharacter_And_SetMainCharacter_Work()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var character = AssertSuccess(await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage)));

        var updated = AssertSuccess(await context.UpdateCharacterHandler.HandleAsync(new UpdateCharacterCommand(
            player.Id,
            character.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFire,
            CharacterRole.Damage)));

        var mainCharacter = AssertSuccess(await context.SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, updated.Id)));

        Assert.Equal(CharacterSpecialization.MageFire, updated.Specialization);
        Assert.Equal(updated.Id, mainCharacter.MainCharacterId);
        Assert.Single(await context.ListCharactersForPlayerHandler.HandleAsync(new ListCharactersForPlayerQuery(player.Id)));
    }

    [Fact]
    public async Task CreateRaidTeam_AddPlayerToGuild_And_AddPlayerToRaidTeam_Work()
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
        var roster = AssertSuccess(await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id)));

        Assert.Single(roster.Members);
        Assert.Equal(player.Id, roster.Members.Single().PlayerId);
        Assert.Single(await context.ListRaidTeamsForGuildHandler.HandleAsync(new ListRaidTeamsForGuildQuery(guild.Id)));
    }

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

    [Fact]
    public async Task GetRaidTeamManagementOverview_When_No_Data_Exists_Returns_Empty_Collections()
    {
        var context = new TestContext();

        var overview = await context.GetRaidTeamManagementOverviewHandler.HandleAsync(new GetRaidTeamManagementOverviewQuery());

        Assert.Empty(overview.Guilds);
    }

    [Fact]
    public async Task GetRaidTeamManagementOverview_Returns_Guild_Context_When_Guild_Has_No_RaidTeams()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        var overview = await context.GetRaidTeamManagementOverviewHandler.HandleAsync(new GetRaidTeamManagementOverviewQuery());

        var guildOverview = Assert.Single(overview.Guilds);
        Assert.Equal(guild.Id, guildOverview.Id);
        Assert.Equal("Guildwise", guildOverview.Name);
        Assert.Equal("EU", guildOverview.Region);
        Assert.Equal("Draenor", guildOverview.Realm);
        Assert.Equal(0, guildOverview.RaidTeamCount);
        Assert.Empty(guildOverview.AvailablePlayers);
        Assert.Empty(guildOverview.Teams);
    }

    [Fact]
    public async Task GetRaidTeamManagementOverview_Returns_Team_Members_And_Role_Composition()
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
            CharacterSpecialization.PaladinProtection,
            CharacterRole.Tank)));

        AssertSuccess(await context.SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id)));
        AssertSuccess(await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member)));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));
        AssertSuccess(await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id)));

        var overview = await context.GetRaidTeamManagementOverviewHandler.HandleAsync(new GetRaidTeamManagementOverviewQuery());

        var guildOverview = Assert.Single(overview.Guilds);
        Assert.Equal(1, guildOverview.RaidMemberCount);
        Assert.Equal(0, guildOverview.UnassignedGuildMemberCount);
        Assert.Equal(0, guildOverview.PlayersWithoutMainCharacterCount);

        var team = Assert.Single(guildOverview.Teams);
        Assert.Equal("Team One", team.Name);
        Assert.Equal(1, team.MemberCount);
        Assert.Equal(1, team.Composition.TankCount);
        Assert.Equal(0, team.Composition.HealerCount);
        Assert.Equal(0, team.Composition.DamageCount);

        var member = Assert.Single(team.Members);
        Assert.Equal(player.Id, member.PlayerId);
        Assert.Equal("Myrmi", member.PlayerDisplayName);
        Assert.Equal(character.Id, member.MainCharacterId);
        Assert.Equal("Alysa", member.MainCharacterName);
        Assert.Equal(CharacterClass.Paladin, member.CharacterClass);
        Assert.Equal(CharacterRole.Tank, member.Role);
        Assert.True(member.HasMainCharacter);
        Assert.Equal(GuildRank.Member, member.GuildRank);

        var availablePlayer = Assert.Single(guildOverview.AvailablePlayers);
        Assert.Equal(player.Id, availablePlayer.PlayerId);
        Assert.Equal("Myrmi", availablePlayer.PlayerDisplayName);
        Assert.True(availablePlayer.HasMainCharacter);
        Assert.Equal(raidTeam.Id, Assert.Single(availablePlayer.RaidTeamIds));
        Assert.Equal("Team One", Assert.Single(availablePlayer.RaidTeamNames));
    }

    [Fact]
    public async Task GetRaidTeamManagementOverview_Counts_Guild_Members_Without_RaidTeam_As_Unassigned()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var assignedPlayer = await context.CreateReadyGuildMemberAsync(guild.Id, "Assigned", CharacterRole.Damage);
        _ = await context.CreateReadyGuildMemberAsync(guild.Id, "Bench", CharacterRole.Healer);
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));
        AssertSuccess(await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, assignedPlayer.PlayerId)));

        var overview = await context.GetRaidTeamManagementOverviewHandler.HandleAsync(new GetRaidTeamManagementOverviewQuery());

        var guildOverview = Assert.Single(overview.Guilds);
        Assert.Equal(1, guildOverview.RaidMemberCount);
        Assert.Equal(1, guildOverview.UnassignedGuildMemberCount);
        Assert.Equal(2, guildOverview.AvailablePlayers.Count);

        var assigned = guildOverview.AvailablePlayers.Single(player => player.PlayerDisplayName == "Assigned");
        Assert.Equal("Team One", Assert.Single(assigned.RaidTeamNames));

        var bench = guildOverview.AvailablePlayers.Single(player => player.PlayerDisplayName == "Bench");
        Assert.Empty(bench.RaidTeamIds);
        Assert.Empty(bench.RaidTeamNames);
    }

    [Fact]
    public async Task GetRaidTeamManagementOverview_Shows_RaidTeam_Member_Without_Main_And_Excludes_From_Composition()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var memberSetup = await context.CreateReadyGuildMemberAsync(guild.Id, "Myrmi", CharacterRole.Damage);
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));
        AssertSuccess(await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, memberSetup.PlayerId)));

        var player = await context.PlayerRepository.GetByIdAsync(memberSetup.PlayerId);
        player!.RemoveCharacter(memberSetup.CharacterId);

        var overview = await context.GetRaidTeamManagementOverviewHandler.HandleAsync(new GetRaidTeamManagementOverviewQuery());

        var guildOverview = Assert.Single(overview.Guilds);
        Assert.Equal(1, guildOverview.PlayersWithoutMainCharacterCount);

        var availablePlayer = Assert.Single(guildOverview.AvailablePlayers);
        Assert.Equal(memberSetup.PlayerId, availablePlayer.PlayerId);
        Assert.False(availablePlayer.HasMainCharacter);
        Assert.Null(availablePlayer.MainCharacterId);
        Assert.Equal("Team One", Assert.Single(availablePlayer.RaidTeamNames));

        var team = Assert.Single(guildOverview.Teams);
        Assert.Equal(0, team.Composition.TankCount);
        Assert.Equal(0, team.Composition.HealerCount);
        Assert.Equal(0, team.Composition.DamageCount);

        var member = Assert.Single(team.Members);
        Assert.Equal(memberSetup.PlayerId, member.PlayerId);
        Assert.False(member.HasMainCharacter);
        Assert.Null(member.MainCharacterId);
        Assert.Null(member.Role);
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
    public async Task DeletePlayer_Removes_Guild_Memberships_And_RaidTeam_Memberships()
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

        AssertSuccess(await context.DeletePlayerHandler.HandleAsync(new DeletePlayerCommand(player.Id)));

        Assert.Null(await context.GetPlayerHandler.HandleAsync(new GetPlayerQuery(player.Id)));
        Assert.Empty((await context.GetGuildHandler.HandleAsync(new GetGuildQuery(guild.Id)))!.Members);
        Assert.Empty((await context.GetRaidTeamHandler.HandleAsync(new GetRaidTeamQuery(guild.Id, raidTeam.Id)))!.Members);
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
    public async Task CreateCharacter_Returns_Created_Character()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var character = AssertSuccess(await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.DeathKnight,
            CharacterSpecialization.DeathKnightFrost,
            CharacterRole.Damage)));

        Assert.Equal(CharacterClass.DeathKnight, character.CharacterClass);
        Assert.Single(await context.ListCharactersHandler.HandleAsync(new ListCharactersQuery()));
    }

    [Fact]
    public async Task CreateCharacter_With_Invalid_Class_Specialization_Returns_Validation()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var result = await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage));

        AssertFailure(result, FailureType.Validation, "specialization");
    }

    [Fact]
    public async Task CreateCharacter_With_Duplicate_Identity_Returns_Conflict()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        AssertSuccess(await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage)));

        var result = await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            " alysa ",
            "eu",
            "draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinHoly,
            CharacterRole.Healer));

        AssertFailure(result, FailureType.Conflict, "Duplicate character");
    }

    [Fact]
    public async Task SetMainCharacter_For_Missing_Character_Returns_NotFound()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var result = await context.SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "Character");
    }

    [Fact]
    public async Task SetMainCharacter_For_Missing_Player_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(Guid.NewGuid(), Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "Player");
    }

    [Fact]
    public async Task AddPlayerToGuild_When_Player_Is_Already_Member_Returns_Conflict()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        AssertSuccess(await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member)));

        var result = await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Officer));

        AssertFailure(result, FailureType.Conflict, "already a guild member");
    }

    [Fact]
    public async Task AddPlayerToGuild_When_Guild_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var result = await context.AddPlayerToGuildHandler.HandleAsync(
            new AddPlayerToGuildCommand(Guid.NewGuid(), player.Id, GuildRank.Member));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Fact]
    public async Task AddPlayerToGuild_When_Player_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        var result = await context.AddPlayerToGuildHandler.HandleAsync(
            new AddPlayerToGuildCommand(guild.Id, Guid.NewGuid(), GuildRank.Member));

        AssertFailure(result, FailureType.NotFound, "Player");
    }

    [Fact]
    public async Task AddPlayerToGuild_With_Invalid_Rank_Returns_Validation()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var result = await context.AddPlayerToGuildHandler.HandleAsync(
            new AddPlayerToGuildCommand(guild.Id, player.Id, (GuildRank)999));

        AssertFailure(result, FailureType.Validation, "rank");
    }

    [Fact]
    public async Task AddPlayerToRaidTeam_When_Guild_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.AddPlayerToRaidTeamHandler.HandleAsync(
            new AddPlayerToRaidTeamCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Fact]
    public async Task AddPlayerToRaidTeam_When_Player_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));

        var result = await context.AddPlayerToRaidTeamHandler.HandleAsync(
            new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "Player");
    }

    [Fact]
    public async Task AddPlayerToRaidTeam_When_Player_Has_No_Main_Character_Returns_BusinessRule()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));
        AssertSuccess(await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member)));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));

        var result = await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id));

        AssertFailure(result, FailureType.BusinessRule, "main character");
    }

    [Fact]
    public async Task AddPlayerToRaidTeam_When_Player_Is_Already_Member_Returns_Conflict()
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

        var result = await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id));

        AssertFailure(result, FailureType.Conflict, "already a member");
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

    [Fact]
    public async Task CreatePlayer_With_Blank_DisplayName_Returns_Validation()
    {
        var context = new TestContext();

        var result = await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand(" "));

        AssertFailure(result, FailureType.Validation, "display name");
    }

    [Fact]
    public async Task UpdateCharacter_When_Character_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var result = await context.UpdateCharacterHandler.HandleAsync(new UpdateCharacterCommand(
            player.Id,
            Guid.NewGuid(),
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage));

        AssertFailure(result, FailureType.NotFound, "Character");
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
    public async Task RemovePlayerFromRaidTeam_When_Player_Is_Not_In_RaidTeam_Returns_NotFound()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));

        var result = await context.RemovePlayerFromRaidTeamHandler.HandleAsync(new RemovePlayerFromRaidTeamCommand(
            guild.Id,
            raidTeam.Id,
            player.Id));

        AssertFailure(result, FailureType.NotFound, "raid team");
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

    [Fact]
    public async Task UpdatePlayer_With_Blank_DisplayName_Returns_Validation()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var result = await context.UpdatePlayerHandler.HandleAsync(new UpdatePlayerCommand(player.Id, " "));

        AssertFailure(result, FailureType.Validation, "display name");
    }

    [Theory]
    [InlineData("", "EU", "Draenor", "name")]
    [InlineData("Alysa", "", "Draenor", "region")]
    [InlineData("Alysa", "EU", "", "realm")]
    public async Task CreateCharacter_With_Blank_Values_Returns_Validation(
        string name,
        string region,
        string realm,
        string expectedMessage)
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var result = await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            name,
            region,
            realm,
            CharacterClass.Mage,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage));

        AssertFailure(result, FailureType.Validation, expectedMessage);
    }

    [Fact]
    public async Task UpdateCharacter_When_Player_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.UpdateCharacterHandler.HandleAsync(new UpdateCharacterCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage));

        AssertFailure(result, FailureType.NotFound, "Player");
    }

    [Fact]
    public async Task UpdateCharacter_With_Invalid_Class_Specialization_Returns_Validation()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));
        var character = AssertSuccess(await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage)));

        var result = await context.UpdateCharacterHandler.HandleAsync(new UpdateCharacterCommand(
            player.Id,
            character.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage));

        AssertFailure(result, FailureType.Validation, "specialization");
    }

    [Fact]
    public async Task UpdateCharacter_With_Duplicate_Identity_Returns_Conflict()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));
        AssertSuccess(await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage)));
        var character = AssertSuccess(await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Bryn",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage)));

        var result = await context.UpdateCharacterHandler.HandleAsync(new UpdateCharacterCommand(
            player.Id,
            character.Id,
            " alysa ",
            "eu",
            "draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinHoly,
            CharacterRole.Healer));

        AssertFailure(result, FailureType.Conflict, "Duplicate character");
    }

    [Fact]
    public async Task DeleteCharacter_When_Player_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.DeleteCharacterHandler.HandleAsync(new DeleteCharacterCommand(Guid.NewGuid(), Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "Player");
    }

    [Fact]
    public async Task DeleteCharacter_When_Character_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var player = AssertSuccess(await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi")));

        var result = await context.DeleteCharacterHandler.HandleAsync(new DeleteCharacterCommand(player.Id, Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "Character");
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

    [Fact]
    public async Task RemovePlayerFromRaidTeam_When_Guild_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();

        var result = await context.RemovePlayerFromRaidTeamHandler.HandleAsync(new RemovePlayerFromRaidTeamCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "Guild");
    }

    [Fact]
    public async Task RemovePlayerFromRaidTeam_When_RaidTeam_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));

        var result = await context.RemovePlayerFromRaidTeamHandler.HandleAsync(new RemovePlayerFromRaidTeamCommand(
            guild.Id,
            Guid.NewGuid(),
            Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "RaidTeam");
    }

    [Fact]
    public async Task RemovePlayerFromRaidTeam_When_Player_Is_Missing_Returns_NotFound()
    {
        var context = new TestContext();
        var guild = AssertSuccess(await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor")));
        var raidTeam = AssertSuccess(await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One")));

        var result = await context.RemovePlayerFromRaidTeamHandler.HandleAsync(new RemovePlayerFromRaidTeamCommand(
            guild.Id,
            raidTeam.Id,
            Guid.NewGuid()));

        AssertFailure(result, FailureType.NotFound, "Player");
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
        Assert.Equal("Bring flasks.", raidEvent.Notes);

        var stored = await context.RaidEventRepository.GetByIdAsync(raidEvent.Id);
        Assert.NotNull(stored);
        Assert.Equal(raidEvent.Id, stored.Id);
        Assert.Equal(TimeSpan.Zero, stored.StartTime.Offset);
        Assert.Equal(startTime.ToUniversalTime(), stored.StartTime);
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

    [Fact]
    public async Task GetRaidEvent_Returns_Dto_For_Existing_Event()
    {
        var context = new TestContext();
        var raidEvent = await context.CreateReadyRaidEventAsync("Team One", "Raid Night");

        var loaded = await context.GetRaidEventHandler.HandleAsync(new GetRaidEventQuery(raidEvent.Id));

        Assert.NotNull(loaded);
        Assert.Equal(raidEvent.Id, loaded.Id);
        Assert.Equal("Raid Night", loaded.Title);
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

    private static T AssertSuccess<T>(Result<T> result)
    {
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Failure);
        Assert.NotNull(result.Value);
        return result.Value;
    }

    private static void AssertSuccess(Result result)
    {
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Failure);
    }

    private static void AssertFailure<T>(Result<T> result, FailureType expectedType, string expectedMessagePart)
    {
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
        Assert.NotNull(result.Failure);
        Assert.Equal(expectedType, result.Failure.Type);
        Assert.Contains(expectedMessagePart, result.Failure.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertFailure(Result result, FailureType expectedType, string expectedMessagePart)
    {
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Failure);
        Assert.Equal(expectedType, result.Failure.Type);
        Assert.Contains(expectedMessagePart, result.Failure.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TestContext
    {
        public TestContext()
        {
            CreateGuildHandler = new CreateGuildHandler(GuildRepository);
            GetGuildHandler = new GetGuildHandler(GuildRepository);
            ListGuildsHandler = new ListGuildsHandler(GuildRepository);
            UpdateGuildHandler = new UpdateGuildHandler(GuildRepository);
            DeleteGuildHandler = new DeleteGuildHandler(GuildRepository);

            CreatePlayerHandler = new CreatePlayerHandler(PlayerRepository);
            GetPlayerHandler = new GetPlayerHandler(PlayerRepository);
            ListPlayersHandler = new ListPlayersHandler(PlayerRepository);
            UpdatePlayerHandler = new UpdatePlayerHandler(PlayerRepository);
            DeletePlayerHandler = new DeletePlayerHandler(GuildRepository, PlayerRepository, TransactionRunner);

            CreateCharacterHandler = new CreateCharacterHandler(PlayerRepository);
            GetCharacterHandler = new GetCharacterHandler(PlayerRepository);
            ListCharactersHandler = new ListCharactersHandler(PlayerRepository);
            ListCharactersForPlayerHandler = new ListCharactersForPlayerHandler(PlayerRepository);
            UpdateCharacterHandler = new UpdateCharacterHandler(PlayerRepository);
            DeleteCharacterHandler = new DeleteCharacterHandler(PlayerRepository);
            SetMainCharacterHandler = new SetMainCharacterHandler(PlayerRepository);

            CreateRaidTeamHandler = new CreateRaidTeamHandler(GuildRepository);
            GetRaidTeamHandler = new GetRaidTeamHandler(GuildRepository);
            ListRaidTeamsForGuildHandler = new ListRaidTeamsForGuildHandler(GuildRepository);
            UpdateRaidTeamHandler = new UpdateRaidTeamHandler(GuildRepository);
            DeleteRaidTeamHandler = new DeleteRaidTeamHandler(GuildRepository);
            AddPlayerToRaidTeamHandler = new AddPlayerToRaidTeamHandler(GuildRepository, PlayerRepository);
            RemovePlayerFromRaidTeamHandler = new RemovePlayerFromRaidTeamHandler(GuildRepository, PlayerRepository);

            CreateRaidEventHandler = new CreateRaidEventHandler(GuildRepository, RaidEventRepository);
            GetRaidEventHandler = new GetRaidEventHandler(RaidEventRepository);
            ListRaidEventsHandler = new ListRaidEventsHandler(RaidEventRepository);

            AddPlayerToGuildHandler = new AddPlayerToGuildHandler(GuildRepository, PlayerRepository);
            AddAdditionalRoleHandler = new AddAdditionalRoleToGuildMemberHandler(GuildRepository);
            RemoveAdditionalRoleHandler = new RemoveAdditionalRoleFromGuildMemberHandler(GuildRepository);

            GetRosterOverviewHandler = new GetRosterOverviewHandler(GuildRepository, PlayerRepository);
            GetRaidTeamManagementOverviewHandler = new GetRaidTeamManagementOverviewHandler(GuildRepository, PlayerRepository);
        }

        public InMemoryGuildRepository GuildRepository { get; } = new();

        public InMemoryPlayerRepository PlayerRepository { get; } = new();

        public InMemoryRaidEventRepository RaidEventRepository { get; } = new();

        public RecordingTransactionRunner TransactionRunner { get; } = new();

        public CreateGuildHandler CreateGuildHandler { get; }
        public GetGuildHandler GetGuildHandler { get; }
        public ListGuildsHandler ListGuildsHandler { get; }
        public UpdateGuildHandler UpdateGuildHandler { get; }
        public DeleteGuildHandler DeleteGuildHandler { get; }

        public CreatePlayerHandler CreatePlayerHandler { get; }
        public GetPlayerHandler GetPlayerHandler { get; }
        public ListPlayersHandler ListPlayersHandler { get; }
        public UpdatePlayerHandler UpdatePlayerHandler { get; }
        public DeletePlayerHandler DeletePlayerHandler { get; }

        public CreateCharacterHandler CreateCharacterHandler { get; }
        public GetCharacterHandler GetCharacterHandler { get; }
        public ListCharactersHandler ListCharactersHandler { get; }
        public ListCharactersForPlayerHandler ListCharactersForPlayerHandler { get; }
        public UpdateCharacterHandler UpdateCharacterHandler { get; }
        public DeleteCharacterHandler DeleteCharacterHandler { get; }
        public SetMainCharacterHandler SetMainCharacterHandler { get; }

        public CreateRaidTeamHandler CreateRaidTeamHandler { get; }
        public GetRaidTeamHandler GetRaidTeamHandler { get; }
        public ListRaidTeamsForGuildHandler ListRaidTeamsForGuildHandler { get; }
        public UpdateRaidTeamHandler UpdateRaidTeamHandler { get; }
        public DeleteRaidTeamHandler DeleteRaidTeamHandler { get; }
        public AddPlayerToRaidTeamHandler AddPlayerToRaidTeamHandler { get; }
        public RemovePlayerFromRaidTeamHandler RemovePlayerFromRaidTeamHandler { get; }

        public CreateRaidEventHandler CreateRaidEventHandler { get; }
        public GetRaidEventHandler GetRaidEventHandler { get; }
        public ListRaidEventsHandler ListRaidEventsHandler { get; }

        public AddPlayerToGuildHandler AddPlayerToGuildHandler { get; }
        public AddAdditionalRoleToGuildMemberHandler AddAdditionalRoleHandler { get; }
        public RemoveAdditionalRoleFromGuildMemberHandler RemoveAdditionalRoleHandler { get; }
        public GetRosterOverviewHandler GetRosterOverviewHandler { get; }
        public GetRaidTeamManagementOverviewHandler GetRaidTeamManagementOverviewHandler { get; }

        public async Task<(Guid PlayerId, Guid CharacterId)> CreateReadyGuildMemberAsync(
            Guid guildId,
            string playerName,
            CharacterRole role)
        {
            var player = AssertSuccess(await CreatePlayerHandler.HandleAsync(new CreatePlayerCommand(playerName)));
            var character = AssertSuccess(await CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
                player.Id,
                $"{playerName}main",
                "EU",
                "Draenor",
                CharacterClass.Paladin,
                role == CharacterRole.Tank
                    ? CharacterSpecialization.PaladinProtection
                    : CharacterSpecialization.PaladinRetribution,
                role)));

            AssertSuccess(await SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id)));
            AssertSuccess(await AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guildId, player.Id, GuildRank.Member)));

            return (player.Id, character.Id);
        }

        public async Task<RaidEventDto> CreateReadyRaidEventAsync(string raidTeamName, string title)
        {
            var guild = AssertSuccess(await CreateGuildHandler.HandleAsync(new CreateGuildCommand(
                $"Guild{Guid.NewGuid():N}",
                "EU",
                "Draenor")));
            var raidTeam = AssertSuccess(await CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, raidTeamName)));

            return AssertSuccess(await CreateRaidEventHandler.HandleAsync(new CreateRaidEventCommand(
                guild.Id,
                raidTeam.Id,
                title,
                DateTimeOffset.UtcNow.AddDays(1),
                null,
                "Nerubar Palace",
                RaidDifficulty.Normal,
                null)));
        }
    }

    private sealed class InMemoryGuildRepository : IGuildRepository
    {
        private readonly Dictionary<Guid, Guild> _guilds = new();

        public Task<Guild?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_guilds.TryGetValue(id, out var guild) ? guild : null);

        public Task<IReadOnlyCollection<Guild>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Guild>>(_guilds.Values.ToList());

        public Task AddAsync(Guild guild, CancellationToken cancellationToken = default)
        {
            _guilds.Add(guild.Id, guild);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _guilds.Remove(id);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly Dictionary<Guid, Player> _players = new();

        public Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_players.TryGetValue(id, out var player) ? player : null);

        public Task<IReadOnlyCollection<Player>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Player>>(_players.Values.ToList());

        public Task AddAsync(Player player, CancellationToken cancellationToken = default)
        {
            _players.Add(player.Id, player);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _players.Remove(id);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryRaidEventRepository : IRaidEventRepository
    {
        private readonly Dictionary<Guid, RaidEvent> _raidEvents = new();

        public Task<RaidEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_raidEvents.TryGetValue(id, out var raidEvent) ? raidEvent : null);

        public Task<IReadOnlyCollection<RaidEvent>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<RaidEvent>>(OrderedRaidEvents().ToList());

        public Task<IReadOnlyCollection<RaidEvent>> ListForGuildAsync(
            Guid guildId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<RaidEvent>>(OrderedRaidEvents()
                .Where(raidEvent => raidEvent.GuildId == guildId)
                .ToList());

        public Task<IReadOnlyCollection<RaidEvent>> ListForRaidTeamAsync(
            Guid raidTeamId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<RaidEvent>>(OrderedRaidEvents()
                .Where(raidEvent => raidEvent.RaidTeamId == raidTeamId)
                .ToList());

        public Task AddAsync(RaidEvent raidEvent, CancellationToken cancellationToken = default)
        {
            _raidEvents.Add(raidEvent.Id, raidEvent);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        private IEnumerable<RaidEvent> OrderedRaidEvents()
            => _raidEvents.Values
                .OrderBy(raidEvent => raidEvent.StartTime)
                .ThenBy(raidEvent => raidEvent.Title)
                .ThenBy(raidEvent => raidEvent.Id);
    }

    private sealed class RecordingTransactionRunner : ITransactionRunner
    {
        public int ExecuteCalls { get; private set; }

        public async Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            ExecuteCalls++;
            await operation(cancellationToken);
        }

        public async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            ExecuteCalls++;
            return await operation(cancellationToken);
        }
    }
}



