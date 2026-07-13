using System.Reflection;
using Guildwise.Domain;
using static Guildwise.UnitTests.DomainModelTestSupport;
namespace Guildwise.UnitTests;

public sealed class GuildTests
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
}
