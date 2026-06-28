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
using Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;
using Guildwise.Application.RaidTeams.CreateRaidTeam;
using Guildwise.Application.RaidTeams.DeleteRaidTeam;
using Guildwise.Application.RaidTeams.GetRaidTeam;
using Guildwise.Application.RaidTeams.ListRaidTeamsForGuild;
using Guildwise.Application.RaidTeams.RemovePlayerFromRaidTeam;
using Guildwise.Application.RaidTeams.UpdateRaidTeam;
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
            DeletePlayerHandler = new DeletePlayerHandler(GuildRepository, PlayerRepository);

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

            AddPlayerToGuildHandler = new AddPlayerToGuildHandler(GuildRepository, PlayerRepository);
            AddAdditionalRoleHandler = new AddAdditionalRoleToGuildMemberHandler(GuildRepository);
            RemoveAdditionalRoleHandler = new RemoveAdditionalRoleFromGuildMemberHandler(GuildRepository);
        }

        public InMemoryGuildRepository GuildRepository { get; } = new();

        public InMemoryPlayerRepository PlayerRepository { get; } = new();

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

        public AddPlayerToGuildHandler AddPlayerToGuildHandler { get; }
        public AddAdditionalRoleToGuildMemberHandler AddAdditionalRoleHandler { get; }
        public RemoveAdditionalRoleFromGuildMemberHandler RemoveAdditionalRoleHandler { get; }
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
}



