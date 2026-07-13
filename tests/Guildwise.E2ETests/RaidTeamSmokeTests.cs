using Microsoft.Playwright;

namespace Guildwise.E2ETests;

[Collection(E2ETestCollection.Name)]
public sealed class RaidTeamSmokeTests(GuildwiseWebAppFixture app) :
    PlaywrightTestBase,
    IClassFixture<GuildwiseWebAppFixture>
{
    [Fact]
    public async Task Seeded_RaidTeam_Can_Be_Opened()
    {
        await RunWithDiagnosticsAsync(
            nameof(Seeded_RaidTeam_Can_Be_Opened),
            app.ArtifactRootPath,
            async () =>
            {
                await SeedSampleRosterAsync(app.BaseUrl);

                await Page.GetByRole(AriaRole.Link, new() { Name = "Raid Teams", Exact = true }).ClickAsync();
                await Expect(Page).ToHaveURLAsync($"{app.BaseUrl}/raid-teams");
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Raid Teams" })).ToBeVisibleAsync();

                await Page.GetByText("Team One", new() { Exact = true }).Nth(0).ClickAsync();

                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Team Detail" })).ToBeVisibleAsync();
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Team One" })).ToBeVisibleAsync();
                await Expect(Page.GetByText("Myrmi")).ToBeVisibleAsync();
                await Expect(Page.GetByText("Alysa")).ToBeVisibleAsync();
                await AssertNoGlobalErrorAsync();
            });
    }
}
