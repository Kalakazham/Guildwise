using AngleSharp.Dom;
using Bunit;
using Guildwise.Application.Contracts.RaidTeamManagement;
using Guildwise.Domain;
using Guildwise.Web.Components.Pages.RaidTeamManagementComponents;
using Microsoft.AspNetCore.Components;

namespace Guildwise.Web.Tests;

public sealed class RaidTeamAvailablePlayersTests : BunitContext
{
    private static readonly Guid GuildAId = new("00000000-0000-0000-0000-000000000100");
    private static readonly Guid GuildBId = new("00000000-0000-0000-0000-000000000200");
    private static readonly Guid TeamAId = new("00000000-0000-0000-0000-000000000101");
    private static readonly Guid TeamBId = new("00000000-0000-0000-0000-000000000102");

    [Fact]
    public void Filters_AvailablePlayers_By_Search_Text()
    {
        var guild = CreateGuild(
            GuildAId,
            TeamAId,
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Ayla Searchmatch", "Ayla"),
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Borin Hidden", "Borin"));
        var team = guild.Teams[0];

        var cut = RenderAvailablePlayers(guild, team);

        SearchInput(cut).Input("Searchmatch");

        Assert.Contains("Ayla Searchmatch", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Borin Hidden", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Resets_Filters_When_Guild_Changes()
    {
        var guildA = CreateGuild(
            GuildAId,
            TeamAId,
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Only Visible", "Only"),
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Filtered Out", "Filtered"));
        var guildB = CreateGuild(
            GuildBId,
            TeamAId,
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Guild B First", "First"),
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Guild B Second", "Second"));
        var cut = RenderAvailablePlayers(guildA, guildA.Teams[0]);
        SearchInput(cut).Input("Only");

        cut.Render(parameters => parameters
            .Add(component => component.Guild, guildB)
            .Add(component => component.SelectedTeam, guildB.Teams[0]));

        Assert.Equal(string.Empty, SearchInput(cut).GetAttribute("value"));
        Assert.Contains("Guild B First", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Guild B Second", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Resets_Filters_When_Selected_Team_Changes()
    {
        var guild = CreateGuild(
            GuildAId,
            TeamAId,
            TeamBId,
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Team A Match", "Match"),
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Team B Visible", "Visible"));
        var cut = RenderAvailablePlayers(guild, guild.Teams[0]);
        SearchInput(cut).Input("Match");

        cut.Render(parameters => parameters
            .Add(component => component.Guild, guild)
            .Add(component => component.SelectedTeam, guild.Teams[1]));

        Assert.Equal(string.Empty, SearchInput(cut).GetAttribute("value"));
        Assert.Contains("Team A Match", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Team B Visible", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Does_Not_Reset_Filters_When_Same_Team_ReRenders()
    {
        var guild = CreateGuild(
            GuildAId,
            TeamAId,
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Needle Player", "Needle"),
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Other Player", "Other"));
        var rerenderedGuild = CreateGuild(
            GuildAId,
            TeamAId,
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Needle Player", "Needle"),
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Other Player", "Other"));
        var cut = RenderAvailablePlayers(guild, guild.Teams[0]);
        SearchInput(cut).Input("Needle");

        cut.Render(parameters => parameters
            .Add(component => component.Guild, rerenderedGuild)
            .Add(component => component.SelectedTeam, rerenderedGuild.Teams[0]));

        Assert.Equal("Needle", SearchInput(cut).GetAttribute("value"));
        Assert.Contains("Needle Player", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Other Player", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Sorts_AvailablePlayers_By_DisplayName_Then_PlayerId()
    {
        var lowerPlayerId = new Guid("00000000-0000-0000-0000-000000000001");
        var higherPlayerId = new Guid("00000000-0000-0000-0000-000000000002");
        var guild = CreateGuild(
            GuildAId,
            TeamAId,
            RaidTeamTestData.AvailablePlayer(Guid.NewGuid(), "Beta Player", "Beta"),
            RaidTeamTestData.AvailablePlayer(higherPlayerId, "alpha Player", "Alpha Main"),
            RaidTeamTestData.AvailablePlayer(lowerPlayerId, "alpha Player", "Zeta Main"));

        var cut = RenderAvailablePlayers(guild, guild.Teams[0]);

        AssertVisibleOrder(
            cut.Markup,
            "Zeta Main - EU - Zeta Main Realm",
            "Alpha Main - EU - Alpha Main Realm",
            "Beta Player");
    }

    [Fact]
    public void Invokes_Add_Callback_With_Selected_Player()
    {
        var expectedPlayerId = Guid.NewGuid();
        var guild = CreateGuild(
            GuildAId,
            TeamAId,
            RaidTeamTestData.AvailablePlayer(expectedPlayerId, "Callback Player", "Callback"));
        AvailableRaidTeamPlayerDto? addedPlayer = null;
        var callbackCount = 0;

        var cut = RenderAvailablePlayers(
            guild,
            guild.Teams[0],
            player =>
            {
                callbackCount++;
                addedPlayer = player;
            });

        AddButton(cut).Click();

        Assert.Equal(1, callbackCount);
        Assert.NotNull(addedPlayer);
        Assert.Equal(expectedPlayerId, addedPlayer.PlayerId);
    }

    private static RaidTeamManagementGuildDto CreateGuild(
        Guid guildId,
        Guid teamId,
        params AvailableRaidTeamPlayerDto[] players)
        => CreateGuild(guildId, teamId, null, players);

    private static RaidTeamManagementGuildDto CreateGuild(
        Guid guildId,
        Guid teamAId,
        Guid? teamBId,
        params AvailableRaidTeamPlayerDto[] players)
    {
        var teams = teamBId.HasValue
            ? [RaidTeamTestData.Team(teamAId, guildId, "Team A"), RaidTeamTestData.Team(teamBId.Value, guildId, "Team B")]
            : new[] { RaidTeamTestData.Team(teamAId, guildId, "Team A") };

        return RaidTeamTestData.Guild(guildId, players, teams);
    }

    private IRenderedComponent<RaidTeamAvailablePlayers> RenderAvailablePlayers(
        RaidTeamManagementGuildDto guild,
        RaidTeamManagementTeamDto team,
        Action<AvailableRaidTeamPlayerDto>? onAddPlayer = null)
        => Render<RaidTeamAvailablePlayers>(parameters => parameters
            .Add(component => component.Guild, guild)
            .Add(component => component.SelectedTeam, team)
            .Add(
                component => component.OnAddPlayer,
                EventCallback.Factory.Create<AvailableRaidTeamPlayerDto>(
                    this,
                    onAddPlayer ?? (_ => { }))));

    private static IElement SearchInput(IRenderedComponent<RaidTeamAvailablePlayers> cut)
        => cut.Find("input[placeholder='Player, main character, realm, raid team']");

    private static IElement AddButton(IRenderedComponent<RaidTeamAvailablePlayers> cut)
        => cut.FindAll("button").Single(button => button.TextContent.Trim() == "Add");

    private static void AssertVisibleOrder(string markup, params string[] expectedTexts)
    {
        var previousIndex = -1;

        foreach (var expectedText in expectedTexts)
        {
            var currentIndex = markup.IndexOf(expectedText, StringComparison.Ordinal);

            Assert.True(
                currentIndex > previousIndex,
                $"Expected '{expectedText}' after previous text in rendered markup.");

            previousIndex = currentIndex;
        }
    }
}
