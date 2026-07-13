using System.Reflection;
using Guildwise.Domain;
using static Guildwise.UnitTests.DomainModelTestSupport;
namespace Guildwise.UnitTests;

public sealed class GuildMemberTests
{
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
}
