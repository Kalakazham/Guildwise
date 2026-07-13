using AngleSharp.Dom;
using Bunit;
using Guildwise.Application.Contracts.RaidTeamManagement;
using Guildwise.Domain;
using Guildwise.Web.Components.Pages.RaidTeamManagementComponents;
using Microsoft.AspNetCore.Components;

namespace Guildwise.Web.Tests;

public sealed class RaidTeamDetailPanelTests : BunitContext
{
    private static readonly Guid GuildId = new("00000000-0000-0000-0000-000000000300");
    private static readonly Guid TeamId = new("00000000-0000-0000-0000-000000000301");

    [Fact]
    public void Shows_Composition_Hints_With_Missing_Roles_And_Main_Count()
    {
        var team = RaidTeamTestData.Team(
            TeamId,
            GuildId,
            "Progression",
            [
                RaidTeamTestData.Member(Guid.NewGuid(), "First Missing", hasMainCharacter: false, role: null),
                RaidTeamTestData.Member(Guid.NewGuid(), "Second Missing", hasMainCharacter: false, role: null)
            ],
            new RaidTeamCompositionDto(0, 0, 0));
        var guild = RaidTeamTestData.Guild(GuildId, [], [team]);

        var cut = RenderDetailPanel(guild, team);

        Assert.Contains("No tank assigned", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("No healer assigned", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("No DPS assigned", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("2 members without main character", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Invokes_Remove_Callback_With_Selected_Member()
    {
        var expectedPlayerId = Guid.NewGuid();
        var otherPlayerId = Guid.NewGuid();
        var expectedMember = RaidTeamTestData.Member(expectedPlayerId, "Target Member");
        var otherMember = RaidTeamTestData.Member(otherPlayerId, "Other Member");
        var team = RaidTeamTestData.Team(TeamId, GuildId, "Progression", [expectedMember, otherMember]);
        var guild = RaidTeamTestData.Guild(GuildId, [], [team]);
        RaidTeamManagementMemberDto? removedMember = null;
        var callbackCount = 0;

        var cut = RenderDetailPanel(
            guild,
            team,
            member =>
            {
                callbackCount++;
                removedMember = member;
            });

        RemoveButtonFor(cut, "Target Member").Click();

        Assert.Equal(1, callbackCount);
        Assert.NotNull(removedMember);
        Assert.Equal(expectedPlayerId, removedMember.PlayerId);
        Assert.NotEqual(otherPlayerId, removedMember.PlayerId);
    }

    private IRenderedComponent<RaidTeamDetailPanel> RenderDetailPanel(
        RaidTeamManagementGuildDto guild,
        RaidTeamManagementTeamDto team,
        Action<RaidTeamManagementMemberDto>? onRemovePlayer = null)
        => Render<RaidTeamDetailPanel>(parameters => parameters
            .Add(component => component.Guild, guild)
            .Add(component => component.SelectedTeam, team)
            .Add(
                component => component.OnRemovePlayer,
                EventCallback.Factory.Create<RaidTeamManagementMemberDto>(
                    this,
                    onRemovePlayer ?? (_ => { }))));

    private static IElement RemoveButtonFor(IRenderedComponent<RaidTeamDetailPanel> cut, string playerName)
    {
        var row = cut.FindAll("tbody tr")
            .Single(candidate => candidate.TextContent.Contains(playerName, StringComparison.Ordinal));

        return row.QuerySelector("button")
            ?? throw new InvalidOperationException($"No remove button found for '{playerName}'.");
    }
}
