using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Guildwise.E2ETests;

public sealed class GuildwiseWebAppFixture : IAsyncLifetime, IDisposable
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(250);

    private readonly HttpClient _httpClient = new();
    private Process? _process;
    private Task? _stdoutTask;
    private Task? _stderrTask;
    private StreamWriter? _stdoutWriter;
    private StreamWriter? _stderrWriter;

    public string BaseUrl { get; private set; } = string.Empty;

    public string ArtifactRootPath { get; private set; } = string.Empty;

    public string StdoutLogPath { get; private set; } = string.Empty;

    public string StderrLogPath { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var webProjectDirectory = Path.Combine(repositoryRoot, "src", "Guildwise.Web");
        var webAssemblyPath = Path.Combine(webProjectDirectory, "bin", "Debug", "net10.0", "Guildwise.Web.dll");
        if (!File.Exists(webAssemblyPath))
        {
            throw new InvalidOperationException(
                $"Guildwise.Web was not built. Expected '{webAssemblyPath}'. Run 'dotnet build Guildwise.sln --no-restore' before executing E2E tests.");
        }

        ArtifactRootPath = Path.Combine(repositoryRoot, "artifacts", "playwright");
        var logDirectory = Path.Combine(ArtifactRootPath, "logs");
        Directory.CreateDirectory(logDirectory);
        StdoutLogPath = Path.Combine(logDirectory, "app-stdout.log");
        StderrLogPath = Path.Combine(logDirectory, "app-stderr.log");

        _stdoutWriter = new StreamWriter(File.Open(StdoutLogPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
        {
            AutoFlush = true
        };
        _stderrWriter = new StreamWriter(File.Open(StderrLogPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
        {
            AutoFlush = true
        };

        BaseUrl = $"http://127.0.0.1:{GetFreeLoopbackPort()}";
        _process = StartWebProcess(webAssemblyPath, webProjectDirectory);
        _stdoutTask = CopyOutputAsync(_process.StandardOutput, _stdoutWriter);
        _stderrTask = CopyOutputAsync(_process.StandardError, _stderrWriter);

        await WaitUntilReadyAsync();
    }

    public async Task DisposeAsync()
    {
        try
        {
            await StopProcessAsync();
        }
        finally
        {
            if (_stdoutTask is not null)
            {
                await _stdoutTask;
            }

            if (_stderrTask is not null)
            {
                await _stderrTask;
            }

            _stdoutWriter?.Dispose();
            _stderrWriter?.Dispose();
            _process?.Dispose();
            _httpClient.Dispose();
        }
    }

    public void Dispose()
    {
        _stdoutWriter?.Dispose();
        _stderrWriter?.Dispose();
        _process?.Dispose();
        _httpClient.Dispose();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Guildwise.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root by searching for Guildwise.sln.");
    }

    private Process StartWebProcess(string webAssemblyPath, string webProjectDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = webProjectDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(webAssemblyPath);
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";
        startInfo.Environment["ASPNETCORE_URLS"] = BaseUrl;
        startInfo.Environment["Guildwise__PersistenceProvider"] = "InMemory";
        startInfo.Environment["Guildwise__Database__ApplyMigrationsOnStartup"] = "false";
        startInfo.Environment["Logging__EventLog__LogLevel__Default"] = "None";
        RemovePostgresEnvironmentOverrides(startInfo);

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start Guildwise.Web process.");

        return process;
    }

    private static void RemovePostgresEnvironmentOverrides(ProcessStartInfo startInfo)
    {
        startInfo.Environment.Remove("ConnectionStrings__GuildwiseDatabase");
        startInfo.Environment.Remove("Guildwise__PersistenceProvider__Postgres");
        startInfo.Environment.Remove("PGHOST");
        startInfo.Environment.Remove("PGPORT");
        startInfo.Environment.Remove("PGDATABASE");
        startInfo.Environment.Remove("PGUSER");
        startInfo.Environment.Remove("PGPASSWORD");
    }

    private async Task WaitUntilReadyAsync()
    {
        var deadline = DateTimeOffset.UtcNow.Add(StartupTimeout);
        Exception? lastException = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (_process?.HasExited == true)
            {
                throw await CreateStartupExceptionAsync($"Guildwise.Web exited during startup with code {_process.ExitCode}.");
            }

            try
            {
                using var response = await _httpClient.GetAsync(BaseUrl);
                if ((int)response.StatusCode < 500)
                {
                    return;
                }
            }
            catch (HttpRequestException exception)
            {
                lastException = exception;
            }
            catch (TaskCanceledException exception)
            {
                lastException = exception;
            }

            await Task.Delay(PollInterval);
        }

        throw await CreateStartupExceptionAsync($"Guildwise.Web did not become ready within {StartupTimeout.TotalSeconds:N0} seconds. Last error: {lastException?.Message ?? "none"}");
    }

    private async Task<InvalidOperationException> CreateStartupExceptionAsync(string message)
    {
        await FlushLogsAsync();
        var stdoutTail = ReadLogTail(StdoutLogPath);
        var stderrTail = ReadLogTail(StderrLogPath);

        return new InvalidOperationException(
            $"""
            {message}
            Base URL: {BaseUrl}
            Stdout log: {StdoutLogPath}
            Stderr log: {StderrLogPath}
            Stdout tail:
            {stdoutTail}
            Stderr tail:
            {stderrTail}
            """);
    }

    private async Task StopProcessAsync()
    {
        if (_process is null || _process.HasExited)
        {
            return;
        }

        try
        {
            _process.CloseMainWindow();
            using var shutdownCancellation = new CancellationTokenSource(ShutdownTimeout);
            await _process.WaitForExitAsync(shutdownCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
            }
        }
    }

    private async Task FlushLogsAsync()
    {
        if (_stdoutWriter is not null)
        {
            await _stdoutWriter.FlushAsync();
        }

        if (_stderrWriter is not null)
        {
            await _stderrWriter.FlushAsync();
        }
    }

    private static async Task CopyOutputAsync(TextReader reader, TextWriter writer)
    {
        var buffer = new char[4096];
        while (true)
        {
            var count = await reader.ReadAsync(buffer);
            if (count == 0)
            {
                break;
            }

            await writer.WriteAsync(buffer.AsMemory(0, count));
        }
    }

    private static string ReadLogTail(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return "<log unavailable>";
        }

        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var lines = reader
            .ReadToEnd()
            .Split(Environment.NewLine)
            .TakeLast(40);
        return string.Join(Environment.NewLine, lines);
    }

    private static int GetFreeLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }
}
