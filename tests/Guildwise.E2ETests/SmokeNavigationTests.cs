using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Guildwise.E2ETests;

[Collection(E2ETestCollection.Name)]
public sealed class SmokeNavigationTests(GuildwiseWebAppFixture app) : PageTest
{
    [Fact]
    public async Task App_Starts_And_Primary_Navigation_Renders_Pages()
    {
        await Page.GotoAsync(app.BaseUrl);

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Manual roster setup" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Roster overview", Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Raid Teams", Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Raid Events", Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Roster setup", Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("An unhandled error has occurred.")).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Roster overview", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{app.BaseUrl}/roster");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Roster Overview" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Raid Teams", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{app.BaseUrl}/raid-teams");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Raid Teams" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Raid Events", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{app.BaseUrl}/raid-events");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Raid Events" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Roster setup", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{app.BaseUrl}/roster-setup");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Manual roster setup" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Seed sample roster" })).ToBeVisibleAsync();
    }
}
