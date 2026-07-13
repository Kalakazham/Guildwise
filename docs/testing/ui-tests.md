# UI Tests

Guildwise uses two automated UI test levels.

## Teststufen

* bUnit tests isolated Blazor component logic, conditional rendering, parameters and callbacks.
* Playwright tests real browser and E2E flows against a started Guildwise Web app.

## Voraussetzungen

* .NET 10 SDK
* built Web and E2E test projects
* installed Chromium browser for Playwright
* no PostgreSQL database for the current E2E tests

## Lokale Vorbereitung

Build the solution or at least the Web and E2E test projects:

```bash
dotnet build Guildwise.sln --no-restore
```

Install Chromium through the generated Playwright script:

```powershell
pwsh tests/Guildwise.E2ETests/bin/Debug/net10.0/playwright.ps1 install chromium
```

## Lokale Ausführung

```bash
dotnet test tests/Guildwise.E2ETests/Guildwise.E2ETests.csproj --no-build --no-restore
```

## Testhosting

The E2E fixture starts `Guildwise.Web` itself on a dynamic HTTP loopback port.

The app runs with:

* `ASPNETCORE_ENVIRONMENT=Development`
* `DOTNET_ENVIRONMENT=Development`
* `Guildwise__PersistenceProvider=InMemory`
* `Guildwise__Database__ApplyMigrationsOnStartup=false`

The current E2E tests do not use a developer PostgreSQL database.

## Artefakte

App logs are written to:

```text
artifacts/playwright/logs
```

Traces and screenshots follow in slice `0021b`.

## Debugging

* Check app stdout and stderr logs under `artifacts/playwright/logs`.
* Re-run the Chromium install command if Playwright reports a missing browser.
* Run `dotnet test tests/Guildwise.E2ETests/Guildwise.E2ETests.csproj` without `--no-build` after code changes.
