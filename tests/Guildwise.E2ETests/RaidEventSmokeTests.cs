using System.Globalization;
using Microsoft.Playwright;

namespace Guildwise.E2ETests;

[Collection(E2ETestCollection.Name)]
public sealed class RaidEventSmokeTests(GuildwiseWebAppFixture app) :
    PlaywrightTestBase,
    IClassFixture<GuildwiseWebAppFixture>
{
    [Fact]
    public async Task RaidEvent_Can_Be_Created_And_Selected()
    {
        await RunWithDiagnosticsAsync(
            nameof(RaidEvent_Can_Be_Created_And_Selected),
            app.ArtifactRootPath,
            async () =>
            {
                await SeedSampleRosterAsync(app.BaseUrl);

                await Page.GetByRole(AriaRole.Link, new() { Name = "Raid Events", Exact = true }).ClickAsync();
                await Expect(Page).ToHaveURLAsync($"{app.BaseUrl}/raid-events");
                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Raid Events" })).ToBeVisibleAsync();
                await Expect(Page.GetByLabel("Raid team")).ToContainTextAsync("Team One");

                var title = $"E2E Raid {Guid.NewGuid():N}"[..17];
                var start = DateTime.Now.AddDays(7).Date.AddHours(20);
                var end = start.AddHours(3);

                await Page.GetByLabel("Title").FillAsync(title);
                await Page.GetByLabel("Start date/time").FillAsync(FormatDateTimeLocal(start));
                await Page.GetByLabel("End date/time").FillAsync(FormatDateTimeLocal(end));
                await Page.GetByLabel("Instance").FillAsync("Manaforge Omega");
                await Page.GetByLabel("Difficulty").SelectOptionAsync("Heroic");
                await Page.GetByLabel("Notes").FillAsync("E2E smoke event notes");
                await Page.GetByRole(AriaRole.Button, new() { Name = "Create Event" }).ClickAsync();

                await Expect(Page.GetByText($"Created {title}.")).ToBeVisibleAsync();
                await Page.GetByRole(AriaRole.Button, new() { Name = "Refresh" }).ClickAsync();
                await Expect(Page.GetByText(title)).ToBeVisibleAsync();

                await Page.GetByText(title).Nth(0).ClickAsync();

                await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = title })).ToBeVisibleAsync();
                await Expect(Page.GetByText("Team One").Nth(1)).ToBeVisibleAsync();
                await Expect(Page.GetByText("Manaforge Omega")).ToBeVisibleAsync();
                await Expect(Page.GetByText("Heroic").Nth(1)).ToBeVisibleAsync();
                await Expect(Page.GetByText("E2E smoke event notes")).ToBeVisibleAsync();
                await AssertNoGlobalErrorAsync();
            });
    }

    private static string FormatDateTimeLocal(DateTime value)
        => value.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
}
