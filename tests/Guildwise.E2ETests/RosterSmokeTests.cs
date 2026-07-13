using Microsoft.Playwright;

namespace Guildwise.E2ETests;

[Collection(E2ETestCollection.Name)]
public sealed class RosterSmokeTests(GuildwiseWebAppFixture app) :
    PlaywrightTestBase,
    IClassFixture<GuildwiseWebAppFixture>
{
    [Fact]
    public async Task SampleRoster_Can_Be_Seeded_And_Shown_In_Roster()
    {
        await RunWithDiagnosticsAsync(
            nameof(SampleRoster_Can_Be_Seeded_And_Shown_In_Roster),
            app.ArtifactRootPath,
            async () =>
            {
                await SeedSampleRosterAsync(app.BaseUrl);

                await Page.GetByRole(AriaRole.Link, new() { Name = "Roster overview", Exact = true }).ClickAsync();

                await Expect(Page).ToHaveURLAsync($"{app.BaseUrl}/roster");
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Roster Overview" })).ToBeVisibleAsync();
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Guildwise" })).ToBeVisibleAsync();
                await Expect(Page.GetByText("Myrmi")).ToBeVisibleAsync();
                await Expect(Page.GetByText("Alysa")).ToBeVisibleAsync();
                await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Team One" })).ToBeVisibleAsync();
                await AssertNoGlobalErrorAsync();
            });
    }
}
