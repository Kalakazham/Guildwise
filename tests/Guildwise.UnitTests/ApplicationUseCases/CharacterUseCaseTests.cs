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
public sealed class CharacterUseCaseTests
{
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
}
