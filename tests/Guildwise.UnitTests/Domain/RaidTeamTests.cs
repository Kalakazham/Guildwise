using System.Reflection;
using Guildwise.Domain;
using static Guildwise.UnitTests.DomainModelTestSupport;
namespace Guildwise.UnitTests;

public sealed class RaidTeamTests
{
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
}
