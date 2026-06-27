using Guildwise.Domain;

namespace Guildwise.UnitTests;

public sealed class DomainModelTests
{
    [Fact]
    public void Guild_Create_Valid_Guild()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");

        Assert.Equal("Guildwise", guild.Name);
        Assert.Equal("EU", guild.Region);
        Assert.Equal("Draenor", guild.Realm);
        Assert.Empty(guild.RaidTeams);
        Assert.Empty(guild.Members);
    }

    [Theory]
    [InlineData("", "EU", "Draenor")]
    [InlineData("Guildwise", "", "Draenor")]
    [InlineData("Guildwise", "EU", "")]
    public void Guild_Create_Rejects_Empty_Fields(string name, string region, string realm)
    {
        Assert.Throws<ArgumentException>(() => Guild.Create(name, region, realm));
    }

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
    public void Character_Create_Valid_Character()
    {
        var character = Character.Create(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.Retribution,
            CharacterRole.Damage);

        Assert.Equal("Alysa", character.Name);
        Assert.Equal("EU", character.Region);
        Assert.Equal("Draenor", character.Realm);
        Assert.Equal(CharacterClass.Paladin, character.CharacterClass);
        Assert.Equal(CharacterSpecialization.Retribution, character.Specialization);
        Assert.Equal(CharacterRole.Damage, character.Role);
        Assert.Null(character.PlayerId);
    }

    [Fact]
    public void Player_AddCharacter_Assigns_Character_To_Player()
    {
        var player = Player.Create("Myrmi");
        var character = Character.Create(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.Retribution,
            CharacterRole.Damage);

        player.AddCharacter(character);

        Assert.Single(player.Characters);
        Assert.Equal(player.Id, character.PlayerId);
        Assert.Contains(character, player.Characters);
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
            CharacterSpecialization.Retribution,
            CharacterRole.Damage);

        var duplicate = Character.Create(
            "Alysa",
            "eu",
            "draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.Protection,
            CharacterRole.Tank);

        Assert.Throws<InvalidOperationException>(() => player.AddCharacter(duplicate));
    }

    [Fact]
    public void Player_SetMainCharacter_Sets_Main_Character()
    {
        var player = Player.Create("Myrmi");
        var character = player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.Retribution,
            CharacterRole.Damage);

        player.SetMainCharacter(character);

        Assert.Equal(character.Id, player.MainCharacterId);
    }

    [Fact]
    public void Player_SetMainCharacter_Rejects_Foreign_Character()
    {
        var player = Player.Create("Myrmi");
        var foreignPlayer = Player.Create("Other");
        var foreignCharacter = foreignPlayer.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.Retribution,
            CharacterRole.Damage);

        Assert.Throws<InvalidOperationException>(() => player.SetMainCharacter(foreignCharacter));
    }

    [Fact]
    public void Player_RemoveCharacter_Clears_Main_Character()
    {
        var player = Player.Create("Myrmi");
        var character = player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.Retribution,
            CharacterRole.Damage);
        player.SetMainCharacter(character);

        player.RemoveCharacter(character.Id);

        Assert.Empty(player.Characters);
        Assert.Null(player.MainCharacterId);
        Assert.Null(character.PlayerId);
    }

    [Fact]
    public void RaidTeam_Create_Valid_RaidTeam()
    {
        var raidTeam = RaidTeam.Create("Team One");

        Assert.Equal("Team One", raidTeam.Name);
        Assert.Equal(Guid.Empty, raidTeam.GuildId);
        Assert.Empty(raidTeam.Members);
    }

    [Fact]
    public void Guild_AddRaidTeam_Adds_Team_To_Guild()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var raidTeam = RaidTeam.Create("Team One");

        guild.AddRaidTeam(raidTeam);

        Assert.Single(guild.RaidTeams);
        Assert.Equal(guild.Id, raidTeam.GuildId);
    }

    [Fact]
    public void Guild_AddRaidTeam_Rejects_Duplicate_Names_Within_Guild()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");

        guild.AddRaidTeam(RaidTeam.Create("Team One"));

        Assert.Throws<InvalidOperationException>(() => guild.AddRaidTeam(RaidTeam.Create("team one")));
    }

    [Fact]
    public void RaidTeam_AddPlayer_Rejects_Player_Without_Main_Character()
    {
        var raidTeam = RaidTeam.Create("Team One");
        var player = Player.Create("Myrmi");

        Assert.Throws<InvalidOperationException>(() => raidTeam.AddPlayer(player));
    }

    [Fact]
    public void RaidTeam_AddPlayer_Adds_Player_With_Main_Character()
    {
        var raidTeam = RaidTeam.Create("Team One");
        var player = Player.Create("Myrmi");
        var character = player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.Retribution,
            CharacterRole.Damage);
        player.SetMainCharacter(character);

        raidTeam.AddPlayer(player);

        Assert.Single(raidTeam.Members);
        Assert.Equal(player.Id, raidTeam.Members.Single().PlayerId);
    }

    [Fact]
    public void RaidTeam_AddPlayer_Rejects_Duplicate_Players()
    {
        var raidTeam = RaidTeam.Create("Team One");
        var player = Player.Create("Myrmi");
        var character = player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.Retribution,
            CharacterRole.Damage);
        player.SetMainCharacter(character);

        raidTeam.AddPlayer(player);

        Assert.Throws<InvalidOperationException>(() => raidTeam.AddPlayer(player));
    }

    [Fact]
    public void GuildMember_AddAdditionalRole_Adds_And_Removes_Roles()
    {
        var member = new GuildMember(Guid.NewGuid(), Guid.NewGuid(), GuildRank.Officer);

        member.AddAdditionalRole(AdditionalGuildRole.RaidLead);
        member.AddAdditionalRole(AdditionalGuildRole.Recruiter);
        member.RemoveAdditionalRole(AdditionalGuildRole.RaidLead);

        Assert.Single(member.AdditionalRoles);
        Assert.Contains(AdditionalGuildRole.Recruiter, member.AdditionalRoles);
        Assert.DoesNotContain(AdditionalGuildRole.RaidLead, member.AdditionalRoles);
    }

    [Fact]
    public void GuildMember_AddAdditionalRole_Rejects_Duplicate_Roles()
    {
        var member = new GuildMember(Guid.NewGuid(), Guid.NewGuid(), GuildRank.Officer);

        member.AddAdditionalRole(AdditionalGuildRole.RaidLead);

        Assert.Throws<InvalidOperationException>(() => member.AddAdditionalRole(AdditionalGuildRole.RaidLead));
    }
}
