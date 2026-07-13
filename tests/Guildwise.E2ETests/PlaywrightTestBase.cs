using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Guildwise.E2ETests;

public abstract class PlaywrightTestBase : PageTest
{
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
