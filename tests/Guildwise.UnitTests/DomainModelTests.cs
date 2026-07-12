using System.Reflection;
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

    [Fact]
    public void Guild_CreateRaidTeam_Creates_RaidTeam_Belonging_To_Guild()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");

        var raidTeam = guild.CreateRaidTeam("Team One");

        Assert.Equal("Team One", raidTeam.Name);
        Assert.Equal(guild.Id, raidTeam.GuildId);
        Assert.Single(guild.RaidTeams);
        Assert.Contains(raidTeam, guild.RaidTeams);
    }

    [Fact]
    public void RaidTeam_Has_No_Public_Creation_Api()
    {
        var publicConstructors = typeof(RaidTeam).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        var publicCreateMethods = typeof(RaidTeam)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name == "Create");

        Assert.Empty(publicConstructors);
        Assert.Empty(publicCreateMethods);
    }

    [Fact]
    public void RaidTeam_Has_No_Public_Rename_Or_RemovePlayer_Api()
    {
        var publicBypassMethods = typeof(RaidTeam)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.Name is "Rename" or "RemovePlayer");

        Assert.Empty(publicBypassMethods);
    }

    [Fact]
    public void RaidTeamMember_Has_No_Public_Creation_Api()
    {
        var publicConstructors = typeof(RaidTeamMember).GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        Assert.Empty(publicConstructors);
    }

    [Fact]
    public void Guild_CreateRaidTeam_Rejects_Duplicate_Names_Within_Guild()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");

        guild.CreateRaidTeam("Team One");

        Assert.Throws<InvalidOperationException>(() => guild.CreateRaidTeam("team one"));
    }

    [Fact]
    public void Guild_RenameRaidTeam_Renames_Team_Through_Guild()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var raidTeam = guild.CreateRaidTeam("Team One");

        guild.RenameRaidTeam(raidTeam, "Mythic Team");

        Assert.Equal("Mythic Team", raidTeam.Name);
    }

    [Fact]
    public void Guild_RenameRaidTeam_Rejects_Duplicate_Names_Within_Guild()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var raidTeam = guild.CreateRaidTeam("Team One");
        guild.CreateRaidTeam("Team Two");

        Assert.Throws<InvalidOperationException>(() => guild.RenameRaidTeam(raidTeam, "team two"));
    }

    [Fact]
    public void Guild_RenameRaidTeam_Rejects_RaidTeam_From_Another_Guild()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var otherGuild = Guild.Create("Other", "EU", "Draenor");
        var otherRaidTeam = otherGuild.CreateRaidTeam("Team One");

        Assert.Throws<InvalidOperationException>(() => guild.RenameRaidTeam(otherRaidTeam, "Team Two"));
    }

    [Fact]
    public void Guild_AddPlayerToRaidTeam_Rejects_Player_Without_Main_Character()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var raidTeam = guild.CreateRaidTeam("Team One");
        var player = Player.Create("Myrmi");
        guild.AddMember(player, GuildRank.Member);

        Assert.Throws<InvalidOperationException>(() => guild.AddPlayerToRaidTeam(raidTeam, player));
    }

    [Fact]
    public void Guild_AddPlayerToRaidTeam_Rejects_Player_Who_Is_Not_GuildMember()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var raidTeam = guild.CreateRaidTeam("Team One");
        var player = Player.Create("Myrmi");
        var character = CreateMainReadyCharacter(player);
        player.SetMainCharacter(character);

        Assert.Throws<InvalidOperationException>(() => guild.AddPlayerToRaidTeam(raidTeam, player));
    }

    [Fact]
    public void Guild_AddPlayerToRaidTeam_Rejects_RaidTeam_From_Another_Guild()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var otherGuild = Guild.Create("Other", "EU", "Draenor");
        var otherRaidTeam = otherGuild.CreateRaidTeam("Team One");
        var player = Player.Create("Myrmi");
        var character = CreateMainReadyCharacter(player);
        player.SetMainCharacter(character);
        guild.AddMember(player, GuildRank.Member);

        Assert.Throws<InvalidOperationException>(() => guild.AddPlayerToRaidTeam(otherRaidTeam, player));
    }

    [Fact]
    public void Guild_AddPlayerToRaidTeam_Adds_GuildMember_With_Main_Character()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var raidTeam = guild.CreateRaidTeam("Team One");
        var player = Player.Create("Myrmi");
        var character = CreateMainReadyCharacter(player);
        player.SetMainCharacter(character);
        guild.AddMember(player, GuildRank.Member);

        guild.AddPlayerToRaidTeam(raidTeam, player);

        Assert.Single(raidTeam.Members);
        Assert.Equal(player.Id, raidTeam.Members.Single().PlayerId);
    }

    [Fact]
    public void Guild_AddPlayerToRaidTeam_Rejects_Duplicate_Players()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var raidTeam = guild.CreateRaidTeam("Team One");
        var player = Player.Create("Myrmi");
        var character = CreateMainReadyCharacter(player);
        player.SetMainCharacter(character);
        guild.AddMember(player, GuildRank.Member);

        guild.AddPlayerToRaidTeam(raidTeam, player);

        Assert.Throws<InvalidOperationException>(() => guild.AddPlayerToRaidTeam(raidTeam, player));
    }

    [Fact]
    public void Guild_RemovePlayerFromRaidTeam_Removes_Player_Through_Guild()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var raidTeam = guild.CreateRaidTeam("Team One");
        var player = CreateRosterReadyMember(guild);
        guild.AddPlayerToRaidTeam(raidTeam, player);

        guild.RemovePlayerFromRaidTeam(raidTeam, player.Id);

        Assert.Empty(raidTeam.Members);
    }

    [Fact]
    public void Guild_RemovePlayerFromRaidTeam_Rejects_RaidTeam_From_Another_Guild()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var otherGuild = Guild.Create("Other", "EU", "Draenor");
        var otherRaidTeam = otherGuild.CreateRaidTeam("Team One");
        var player = CreateRosterReadyMember(guild);

        Assert.Throws<InvalidOperationException>(() => guild.RemovePlayerFromRaidTeam(otherRaidTeam, player.Id));
    }

    [Fact]
    public void Guild_RemoveMember_Removes_Player_From_All_Guild_RaidTeams()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var firstRaidTeam = guild.CreateRaidTeam("Team One");
        var secondRaidTeam = guild.CreateRaidTeam("Team Two");
        var player = CreateRosterReadyMember(guild);
        guild.AddPlayerToRaidTeam(firstRaidTeam, player);
        guild.AddPlayerToRaidTeam(secondRaidTeam, player);

        guild.RemoveMember(player.Id);

        Assert.Empty(guild.Members);
        Assert.Empty(firstRaidTeam.Members);
        Assert.Empty(secondRaidTeam.Members);
    }

    [Fact]
    public void GuildMember_Has_No_Public_Creation_Api()
    {
        var publicConstructors = typeof(GuildMember).GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        Assert.Empty(publicConstructors);
    }

    [Fact]
    public void GuildMember_AddAdditionalRole_Adds_And_Removes_Roles()
    {
        var member = CreateGuildMember();

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
        var member = CreateGuildMember();

        member.AddAdditionalRole(AdditionalGuildRole.RaidLead);

        Assert.Throws<InvalidOperationException>(() => member.AddAdditionalRole(AdditionalGuildRole.RaidLead));
    }

    [Fact]
    public void GuildMember_AddAdditionalRole_Rejects_Undefined_Roles()
    {
        var member = CreateGuildMember();

        Assert.Throws<ArgumentOutOfRangeException>(() => member.AddAdditionalRole((AdditionalGuildRole)999));
    }

    [Fact]
    public void RaidEvent_Create_Creates_Valid_Event()
    {
        var guildId = Guid.NewGuid();
        var raidTeamId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(3);

        var raidEvent = RaidEvent.Create(
            guildId,
            raidTeamId,
            " Liberation of Undermine ",
            startTime,
            endTime,
            " Liberation of Undermine ",
            RaidDifficulty.Heroic,
            "  Bring flasks. ");

        Assert.Equal(guildId, raidEvent.GuildId);
        Assert.Equal(raidTeamId, raidEvent.RaidTeamId);
        Assert.Equal("Liberation of Undermine", raidEvent.Title);
        Assert.Equal(startTime, raidEvent.StartTime);
        Assert.Equal(endTime, raidEvent.EndTime);
        Assert.Equal("Liberation of Undermine", raidEvent.InstanceName);
        Assert.Equal(RaidDifficulty.Heroic, raidEvent.Difficulty);
        Assert.Equal(RaidEventStatus.Scheduled, raidEvent.Status);
        Assert.Equal("Bring flasks.", raidEvent.Notes);
    }

    [Fact]
    public void RaidEvent_Create_Normalizes_TimeValues_To_Utc()
    {
        var startTime = new DateTimeOffset(2026, 7, 13, 20, 30, 0, TimeSpan.FromHours(2));
        var endTime = startTime.AddHours(3);

        var raidEvent = RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            startTime,
            endTime,
            "Nerubar Palace",
            RaidDifficulty.Heroic,
            null);

        Assert.Equal(TimeSpan.Zero, raidEvent.StartTime.Offset);
        Assert.Equal(startTime.ToUniversalTime(), raidEvent.StartTime);
        Assert.NotNull(raidEvent.EndTime);
        Assert.Equal(TimeSpan.Zero, raidEvent.EndTime.Value.Offset);
        Assert.Equal(endTime.ToUniversalTime(), raidEvent.EndTime.Value);
    }

    [Fact]
    public void RaidEvent_UpdateDetails_Updates_Event_Details()
    {
        var raidEvent = RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null);
        var newGuildId = Guid.NewGuid();
        var newRaidTeamId = Guid.NewGuid();
        var newStartTime = new DateTimeOffset(2026, 7, 13, 20, 30, 0, TimeSpan.FromHours(2));
        var newEndTime = newStartTime.AddHours(3);

        raidEvent.UpdateDetails(
            newGuildId,
            newRaidTeamId,
            " Manaforge Omega ",
            newStartTime,
            newEndTime,
            " Manaforge Omega ",
            RaidDifficulty.Mythic,
            "  Bring cauldrons. ");

        Assert.Equal(newGuildId, raidEvent.GuildId);
        Assert.Equal(newRaidTeamId, raidEvent.RaidTeamId);
        Assert.Equal("Manaforge Omega", raidEvent.Title);
        Assert.Equal(TimeSpan.Zero, raidEvent.StartTime.Offset);
        Assert.Equal(newStartTime.ToUniversalTime(), raidEvent.StartTime);
        Assert.NotNull(raidEvent.EndTime);
        Assert.Equal(TimeSpan.Zero, raidEvent.EndTime.Value.Offset);
        Assert.Equal(newEndTime.ToUniversalTime(), raidEvent.EndTime.Value);
        Assert.Equal("Manaforge Omega", raidEvent.InstanceName);
        Assert.Equal(RaidDifficulty.Mythic, raidEvent.Difficulty);
        Assert.Equal(RaidEventStatus.Scheduled, raidEvent.Status);
        Assert.Equal("Bring cauldrons.", raidEvent.Notes);
    }

    [Fact]
    public void RaidEvent_UpdateDetails_Rejects_Cancelled_Event()
    {
        var raidEvent = RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null);
        raidEvent.Cancel();

        Assert.Throws<InvalidOperationException>(() => raidEvent.UpdateDetails(
            raidEvent.GuildId,
            raidEvent.RaidTeamId,
            "Updated",
            DateTimeOffset.UtcNow.AddDays(2),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Cancel_Sets_Status_To_Cancelled()
    {
        var raidEvent = RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null);

        raidEvent.Cancel();

        Assert.Equal(RaidEventStatus.Cancelled, raidEvent.Status);
    }

    [Fact]
    public void RaidEvent_Cancel_When_Already_Cancelled_Is_Idempotent()
    {
        var raidEvent = RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null);

        raidEvent.Cancel();
        raidEvent.Cancel();

        Assert.Equal(RaidEventStatus.Cancelled, raidEvent.Status);
    }

    [Fact]
    public void RaidEvent_SetSignup_Adds_New_Signup()
    {
        var raidEvent = CreateRaidEvent();
        var playerId = Guid.NewGuid();

        var signup = raidEvent.SetSignup(playerId, RaidEventSignupStatus.Signed);

        Assert.Equal(raidEvent.Id, signup.RaidEventId);
        Assert.Equal(playerId, signup.PlayerId);
        Assert.Equal(RaidEventSignupStatus.Signed, signup.Status);
        Assert.Same(signup, Assert.Single(raidEvent.Signups));
    }

    [Fact]
    public void RaidEvent_SetSignup_Updates_Existing_Signup_For_Player()
    {
        var raidEvent = CreateRaidEvent();
        var playerId = Guid.NewGuid();
        var initial = raidEvent.SetSignup(playerId, RaidEventSignupStatus.Signed);

        var updated = raidEvent.SetSignup(playerId, RaidEventSignupStatus.Tentative);

        Assert.Same(initial, updated);
        Assert.Single(raidEvent.Signups);
        Assert.Equal(RaidEventSignupStatus.Tentative, updated.Status);
    }

    [Fact]
    public void RaidEvent_SetSignup_Rejects_Empty_PlayerId()
    {
        var raidEvent = CreateRaidEvent();

        Assert.Throws<ArgumentException>(() => raidEvent.SetSignup(Guid.Empty, RaidEventSignupStatus.Signed));
    }

    [Fact]
    public void RaidEvent_SetSignup_Rejects_Unknown_And_Undefined_Status()
    {
        var raidEvent = CreateRaidEvent();
        var playerId = Guid.NewGuid();

        Assert.Throws<ArgumentOutOfRangeException>(() => raidEvent.SetSignup(playerId, RaidEventSignupStatus.Unknown));
        Assert.Throws<ArgumentOutOfRangeException>(() => raidEvent.SetSignup(playerId, (RaidEventSignupStatus)999));
    }

    [Fact]
    public void RaidEvent_SetSignup_Rejects_Cancelled_Event()
    {
        var raidEvent = CreateRaidEvent();
        raidEvent.Cancel();

        Assert.Throws<InvalidOperationException>(() => raidEvent.SetSignup(Guid.NewGuid(), RaidEventSignupStatus.Signed));
    }

    [Fact]
    public void RaidEvent_Signups_Collection_Is_Not_Publicly_Mutable()
    {
        var raidEvent = CreateRaidEvent();
        raidEvent.SetSignup(Guid.NewGuid(), RaidEventSignupStatus.Signed);

        Assert.IsAssignableFrom<IReadOnlyCollection<RaidEventSignup>>(raidEvent.Signups);
        Assert.False(raidEvent.Signups is ICollection<RaidEventSignup> { IsReadOnly: false });
    }

    [Fact]
    public void RaidEvent_Missing_Response_Is_Not_Stored_As_Signup()
    {
        var raidEvent = CreateRaidEvent();

        Assert.Empty(raidEvent.Signups);
        Assert.DoesNotContain(RaidEventSignupStatus.Unknown, raidEvent.Signups.Select(signup => signup.Status));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_Empty_Guild_Or_RaidTeam()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(1);

        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.Empty,
            Guid.NewGuid(),
            "Raid Night",
            startTime,
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));

        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "Raid Night",
            startTime,
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_Blank_Title()
    {
        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            " ",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_Blank_InstanceName()
    {
        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            " ",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_Default_StartTime()
    {
        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            default,
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_EndTime_Not_After_StartTime()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(1);

        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            startTime,
            startTime,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_Unknown_And_Undefined_Difficulty()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            startTime,
            null,
            "Nerubar Palace",
            RaidDifficulty.Unknown,
            null));

        Assert.Throws<ArgumentOutOfRangeException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            startTime,
            null,
            "Nerubar Palace",
            (RaidDifficulty)999,
            null));
    }

    private static Character CreateMainReadyCharacter(Player player)
        => player.AddCharacter(
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);

    private static Player CreateRosterReadyMember(Guild guild)
    {
        var player = Player.Create("Myrmi");
        var character = CreateMainReadyCharacter(player);
        player.SetMainCharacter(character);
        guild.AddMember(player, GuildRank.Member);
        return player;
    }

    private static GuildMember CreateGuildMember()
    {
        var guild = Guild.Create("Guildwise", "EU", "Draenor");
        var player = Player.Create("Myrmi");
        return guild.AddMember(player, GuildRank.Officer);
    }

    private static RaidEvent CreateRaidEvent()
        => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null);
}
