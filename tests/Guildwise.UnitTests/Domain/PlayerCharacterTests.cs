using System.Reflection;
using Guildwise.Domain;
using static Guildwise.UnitTests.DomainModelTestSupport;
namespace Guildwise.UnitTests;
public sealed class PlayerCharacterTests
{
    [Fact]
    public void Player_Create_Valid_Player()
    {
        var player = Player.Create("Myrmi");

        Assert.Equal("Myrmi", player.DisplayName);
        Assert.Empty(player.Characters);
        Assert.Null(player.MainCharacterId);
    }

    [Fact]
    public void Player_Create_Rejects_Empty_DisplayName()
    {
        Assert.Throws<ArgumentException>(() => Player.Create(""));
    }

    [Fact]
    public void Player_AddCharacter_Creates_Character_Belonging_To_Player()
    {
        var player = Player.Create("Myrmi");

        var character = player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);

        Assert.Equal("Alysa", character.Name);
        Assert.Equal("EU", character.Region);
        Assert.Equal("Draenor", character.Realm);
        Assert.Equal(CharacterClass.Paladin, character.CharacterClass);
        Assert.Equal(CharacterSpecialization.PaladinRetribution, character.Specialization);
        Assert.Equal(CharacterRole.Damage, character.Role);
        Assert.Equal(player.Id, character.PlayerId);
        Assert.Contains(character, player.Characters);
    }

    [Theory]
    [InlineData(CharacterClass.Mage, CharacterSpecialization.MageFrost)]
    [InlineData(CharacterClass.DeathKnight, CharacterSpecialization.DeathKnightFrost)]
    [InlineData(CharacterClass.Paladin, CharacterSpecialization.PaladinRetribution)]
    public void Player_AddCharacter_Accepts_Matching_Class_And_Specialization(
        CharacterClass characterClass,
        CharacterSpecialization specialization)
    {
        var player = Player.Create("Myrmi");

        var character = player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            characterClass,
            specialization,
            CharacterRole.Damage);

        Assert.Equal(characterClass, character.CharacterClass);
        Assert.Equal(specialization, character.Specialization);
    }

    [Fact]
    public void Player_AddCharacter_Rejects_Mismatched_Class_And_Specialization()
    {
        var player = Player.Create("Myrmi");

        Assert.Throws<InvalidOperationException>(() => player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage));
    }

    [Fact]
    public void Character_Has_No_Public_Creation_Api()
    {
        var publicConstructors = typeof(Character).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        var publicCreateMethods = typeof(Character)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name == "Create");

        Assert.Empty(publicConstructors);
        Assert.Empty(publicCreateMethods);
    }

    [Fact]
    public void Character_Has_No_Public_Update_Api()
    {
        var publicUpdateMethods = typeof(Character)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.Name == "Update");

        Assert.Empty(publicUpdateMethods);
    }

    [Fact]
    public void Player_AddCharacter_Rejects_Duplicate_Characters()
    {
        var player = Player.Create("Myrmi");

        player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);

        Assert.Throws<InvalidOperationException>(() => player.AddCharacter(
            "Alysa",
            "eu",
            "draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinProtection,
            CharacterRole.Tank));
    }

    [Fact]
    public void Player_AddCharacter_Rejects_Unknown_And_Undefined_Enum_Values()
    {
        var player = Player.Create("Myrmi");

        Assert.Throws<ArgumentOutOfRangeException>(() => player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Unknown,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage));

        Assert.Throws<ArgumentOutOfRangeException>(() => player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            (CharacterSpecialization)999,
            CharacterRole.Damage));
    }

    [Fact]
    public void Player_UpdateCharacter_Updates_Character_Through_Player()
    {
        var player = Player.Create("Myrmi");
        var character = CreateMainReadyCharacter(player);

        player.UpdateCharacter(
            character.Id,
            "Belysa",
            "EU",
            "Silvermoon",
            CharacterClass.Priest,
            CharacterSpecialization.PriestHoly,
            CharacterRole.Healer);

        Assert.Equal("Belysa", character.Name);
        Assert.Equal("Silvermoon", character.Realm);
        Assert.Equal(CharacterClass.Priest, character.CharacterClass);
        Assert.Equal(CharacterSpecialization.PriestHoly, character.Specialization);
        Assert.Equal(CharacterRole.Healer, character.Role);
    }

    [Fact]
    public void Player_UpdateCharacter_Rejects_Mismatched_Class_And_Specialization()
    {
        var player = Player.Create("Myrmi");
        var character = CreateMainReadyCharacter(player);

        Assert.Throws<InvalidOperationException>(() => player.UpdateCharacter(
            character.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage));
    }

    [Fact]
    public void Player_UpdateCharacter_Rejects_Duplicate_Character_Identity()
    {
        var player = Player.Create("Myrmi");
        var character = CreateMainReadyCharacter(player);

        player.AddCharacter(
            "Belysa",
            "EU",
            "Silvermoon",
            CharacterClass.Priest,
            CharacterSpecialization.PriestHoly,
            CharacterRole.Healer);

        Assert.Throws<InvalidOperationException>(() => player.UpdateCharacter(
            character.Id,
            "Belysa",
            "eu",
            "silvermoon",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinProtection,
            CharacterRole.Tank));
    }

    [Fact]
    public void Player_UpdateCharacter_Rejects_Foreign_Character()
    {
        var player = Player.Create("Myrmi");
        var foreignPlayer = Player.Create("Other");
        var foreignCharacter = CreateMainReadyCharacter(foreignPlayer);

        Assert.Throws<InvalidOperationException>(() => player.UpdateCharacter(
            foreignCharacter.Id,
            "Belysa",
            "EU",
            "Silvermoon",
            CharacterClass.Priest,
            CharacterSpecialization.PriestHoly,
            CharacterRole.Healer));
    }

    [Fact]
    public void Player_SetMainCharacter_Sets_Main_Character()
    {
        var player = Player.Create("Myrmi");
        var character = CreateMainReadyCharacter(player);

        player.SetMainCharacter(character);

        Assert.Equal(character.Id, player.MainCharacterId);
    }

    [Fact]
    public void Player_SetMainCharacter_Rejects_Foreign_Character()
    {
        var player = Player.Create("Myrmi");
        var foreignPlayer = Player.Create("Other");
        var foreignCharacter = CreateMainReadyCharacter(foreignPlayer);

        Assert.Throws<InvalidOperationException>(() => player.SetMainCharacter(foreignCharacter));
    }

    [Fact]
    public void Player_RemoveCharacter_Clears_Main_Character_Without_Orphaning_Character()
    {
        var player = Player.Create("Myrmi");
        var character = CreateMainReadyCharacter(player);
        player.SetMainCharacter(character);

        player.RemoveCharacter(character.Id);

        Assert.Empty(player.Characters);
        Assert.Null(player.MainCharacterId);
        Assert.Equal(player.Id, character.PlayerId);
    }
}
