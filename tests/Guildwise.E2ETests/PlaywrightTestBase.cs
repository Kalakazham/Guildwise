using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Guildwise.E2ETests;

public abstract class PlaywrightTestBase : PageTest
{
    private const string BlazorWebSocketPath = "/_blazor";
    private const string UpdateRootComponentsFrame = "UpdateRootComponents";
    private const string EndUpdateRootComponentsFrame = "JS.EndUpdateRootComponents";
    private const string DispatchEventFrame = "DispatchEventAsync";
    private const string EndInvokeDotNetFrame = "JS.EndInvokeDotNet";

    private static readonly TimeSpan BlazorReadinessTimeout = TimeSpan.FromSeconds(15);

    protected async Task SeedSampleRosterAsync(string baseUrl)
    {
        await NavigateToRosterSetupAndWaitForBlazorCircuitAsync(baseUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Manual roster setup" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Seed sample roster" }).ClickAsync();
        await Expect(Page.GetByText("Seeded a complete roster flow.")).ToBeVisibleAsync(new() { Timeout = 15000 });
        await AssertNoGlobalErrorAsync();
    }

    protected async Task AssertNoGlobalErrorAsync()
    {
        await Expect(Page.GetByText("An unhandled error has occurred.")).ToBeHiddenAsync();
    }

    protected async Task RunWithDiagnosticsAsync(
        string testName,
        string artifactRoot,
        Func<Task> testBody)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(testName);
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactRoot);
        ArgumentNullException.ThrowIfNull(testBody);

        var testArtifactDirectory = Path.Combine(artifactRoot, GetSafeArtifactName(testName));
        if (Directory.Exists(testArtifactDirectory))
        {
            Directory.Delete(testArtifactDirectory, recursive: true);
        }

        await Context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        try
        {
            await testBody();
            await Context.Tracing.StopAsync();
        }
        catch
        {
            await CaptureFailureDiagnosticsAsync(testArtifactDirectory);
            throw;
        }
    }

    private async Task CaptureFailureDiagnosticsAsync(string testArtifactDirectory)
    {
        var diagnosticFailures = new List<string>();

        try
        {
            try
            {
                Directory.CreateDirectory(testArtifactDirectory);
            }
            catch (Exception exception)
            {
                diagnosticFailures.Add($"Artifact directory failed: {exception.Message}");
            }

            try
            {
                await Page.ScreenshotAsync(new()
                {
                    Path = Path.Combine(testArtifactDirectory, "screenshot.png"),
                    FullPage = true
                });
            }
            catch (Exception exception)
            {
                diagnosticFailures.Add($"Screenshot failed: {exception.Message}");
            }

            try
            {
                await Context.Tracing.StopAsync(new()
                {
                    Path = Path.Combine(testArtifactDirectory, "trace.zip")
                });
            }
            catch (Exception exception)
            {
                diagnosticFailures.Add($"Trace failed: {exception.Message}");
            }

            await WriteDiagnosticFailuresAsync(testArtifactDirectory, diagnosticFailures);
        }
        catch
        {
        }
    }

    private async Task NavigateToRosterSetupAndWaitForBlazorCircuitAsync(string baseUrl)
    {
        var blazorWebSocket = await NavigateAndWaitForBlazorRootComponentsAsync($"{baseUrl}/roster-setup");
        await ConfirmBlazorInteractivityWithRefreshAsync(blazorWebSocket);
    }

    private async Task<IWebSocket> NavigateAndWaitForBlazorRootComponentsAsync(string url)
    {
        // Test-side readiness is tied to the Blazor Interactive Server protocol and must be rechecked on ASP.NET Core upgrades.
        var rootComponentsReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var blazorWebSocketReady = new TaskCompletionSource<IWebSocket>(TaskCreationOptions.RunContinuationsAsynchronously);
        var rootComponentsUpdated = false;
        IWebSocket? blazorWebSocket = null;
        EventHandler<IWebSocketFrame>? sentHandler = null;
        EventHandler<IWebSocketFrame>? receivedHandler = null;

        void WebSocketHandler(object? _, IWebSocket webSocket)
        {
            if (!webSocket.Url.Contains(BlazorWebSocketPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (blazorWebSocket is not null)
            {
                return;
            }

            blazorWebSocket = webSocket;
            sentHandler = (_, frame) =>
            {
                if (frame.Text?.Contains(UpdateRootComponentsFrame, StringComparison.Ordinal) == true)
                {
                    rootComponentsUpdated = true;
                }
            };

            receivedHandler = (_, frame) =>
            {
                if (rootComponentsUpdated && frame.Text?.Contains(EndUpdateRootComponentsFrame, StringComparison.Ordinal) == true)
                {
                    rootComponentsReady.TrySetResult();
                }
            };

            webSocket.FrameSent += sentHandler;
            webSocket.FrameReceived += receivedHandler;
            blazorWebSocketReady.TrySetResult(webSocket);
        }

        Page.WebSocket += WebSocketHandler;
        try
        {
            await Page.GotoAsync(url);
            var webSocket = await blazorWebSocketReady.Task.WaitAsync(BlazorReadinessTimeout);
            await rootComponentsReady.Task.WaitAsync(BlazorReadinessTimeout);
            return webSocket;
        }
        catch (TimeoutException exception)
        {
            throw new PlaywrightException(
                "Blazor circuit did not complete the non-mutating root component readiness handshake.",
                exception);
        }
        finally
        {
            Page.WebSocket -= WebSocketHandler;
            if (blazorWebSocket is not null)
            {
                if (sentHandler is not null)
                {
                    blazorWebSocket.FrameSent -= sentHandler;
                }

                if (receivedHandler is not null)
                {
                    blazorWebSocket.FrameReceived -= receivedHandler;
                }
            }
        }
    }

    private async Task ConfirmBlazorInteractivityWithRefreshAsync(IWebSocket blazorWebSocket)
    {
        var refreshButton = Page.GetByRole(AriaRole.Button, new() { Name = "Refresh" });
        await Expect(refreshButton).ToBeVisibleAsync();
        var refreshCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var refreshDispatched = false;

        void SentHandler(object? _, IWebSocketFrame frame)
        {
            if (frame.Text?.Contains(DispatchEventFrame, StringComparison.Ordinal) == true)
            {
                refreshDispatched = true;
            }
        }

        void ReceivedHandler(object? _, IWebSocketFrame frame)
        {
            if (refreshDispatched && frame.Text?.Contains(EndInvokeDotNetFrame, StringComparison.Ordinal) == true)
            {
                refreshCompleted.TrySetResult();
            }
        }

        blazorWebSocket.FrameSent += SentHandler;
        blazorWebSocket.FrameReceived += ReceivedHandler;
        try
        {
            await refreshButton.ClickAsync();
            await refreshCompleted.Task.WaitAsync(BlazorReadinessTimeout);
        }
        catch (TimeoutException exception)
        {
            throw new PlaywrightException(
                "Blazor circuit did not complete the non-mutating refresh readiness event.",
                exception);
        }
        finally
        {
            blazorWebSocket.FrameSent -= SentHandler;
            blazorWebSocket.FrameReceived -= ReceivedHandler;
        }
    }

    private static async Task WriteDiagnosticFailuresAsync(
        string testArtifactDirectory,
        List<string> diagnosticFailures)
    {
        if (diagnosticFailures.Count == 0)
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(testArtifactDirectory);
            await File.WriteAllLinesAsync(
                Path.Combine(testArtifactDirectory, "diagnostic-errors.txt"),
                diagnosticFailures);
        }
        catch
        {
        }
    }

    private static string GetSafeArtifactName(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var safeCharacters = value
            .Select(character => invalidCharacters.Contains(character) ? '_' : character)
            .ToArray();

        return new string(safeCharacters);
    }
}
