# Guildwise Agent Instructions

Guildwise is a modular monolith built with ASP.NET Core Blazor and a pragmatic Clean Architecture style.

The current goal is to build a useful guild and raid roster management tool before adding external integrations such as Raider.IO, Blizzard APIs or Warcraft Logs.

## Project Structure

- `src/Guildwise.Domain`
  - Contains domain entities, value objects, enums, domain services and domain rules.
  - Must not reference any other Guildwise project.
  - Must not contain EF Core, ASP.NET Core, JSON serialization attributes, HTTP clients or external API DTOs.

- `src/Guildwise.Application`
  - Contains use cases, application services, DTOs, commands, queries and interfaces/ports.
  - References `Guildwise.Domain`.
  - Defines interfaces for persistence and external data providers.
  - Must not reference `Guildwise.Infrastructure` or `Guildwise.Web`.

- `src/Guildwise.Infrastructure`
  - Contains EF Core, persistence, repositories, caching, background jobs and external API clients.
  - References `Guildwise.Application` and `Guildwise.Domain`.
  - Implements interfaces defined by the Application layer.

- `src/Guildwise.Web`
  - Contains Blazor UI, routing, composition root and dependency injection.
  - References `Guildwise.Application` and `Guildwise.Infrastructure`.
  - May reference Infrastructure only for dependency injection and composition, preferably in `Program.cs` or extension methods.
  - Blazor pages and components must not directly use Infrastructure classes, repositories or DbContexts.

- `tests/Guildwise.UnitTests`
  - Contains unit tests for Domain and Application.
  - Must not use real databases, network calls or web hosts.

- `tests/Guildwise.Web.Tests`
  - Contains fast bUnit component tests for Blazor components.
  - Must not use a real browser, real database or Testcontainers.

- `tests/Guildwise.E2ETests`
  - Contains Playwright browser smoke tests for the started Blazor Web app.
  - Uses real navigation, Blazor interactivity and cross-component UI flows.
  - Must not change an existing developer database.
  - The current E2E host forces InMemory persistence.
  - Must not call external APIs.

- `tests/Guildwise.IntegrationTests`
  - Contains integration tests for endpoints, persistence and application wiring.
  - May reference Web, Application and Infrastructure.

- `tests/Guildwise.ArchitectureTests`
  - Contains tests that enforce architectural boundaries.
  - Should fail if forbidden project or namespace dependencies are introduced.

## Current MVP

Build manual guild, raid team and roster management first.

The first vertical slice is:

- Create a guild.
- Create a raid team for a guild.
- Add characters manually.
- Assign role, class and specialization.
- Show the roster in the web UI.
- Add unit tests for domain behavior.
- Add integration tests once endpoints or persistence exist.

## Explicitly Out of Scope For Now

Do not implement these unless a feature file explicitly says so:

- Raider.IO integration
- Blizzard API integration
- Warcraft Logs integration
- Authentication
- Billing
- Discord bot
- AI recommendations
- Advanced analytics
- Microservices
- Distributed architecture

## Architecture Rules

- Domain must not depend on Application, Infrastructure or Web.
- Application must not depend on Infrastructure or Web.
- Infrastructure may depend on Application and Domain.
- Web may depend on Application and Infrastructure.
- Web must not contain business logic.
- Blazor components should call Application use cases or services, not repositories directly.
- External API DTOs must not leak into Domain.
- EF Core attributes must not be placed on Domain entities.

## Frontend UI Rules

- Do not introduce a new UI framework, component library or admin template without an ADR.
- Keep UI dependencies exclusively in `Guildwise.Web`.
- Do not put CSS classes, color values, badge logic, icon names, Blazor types or component concepts in Domain or Application.
- Keep Infrastructure free of UI concepts.
- Use AdminLTE only as structural or visual inspiration, not as a direct dependency.
- Prefer evolving existing Guildwise Web components before adding external UI libraries.

## Persistence Rules

Guildwise uses PostgreSQL with EF Core for persistent storage.

- EF Core belongs only in `Guildwise.Infrastructure`.
- Do not add EF Core references to Domain or Application.
- Do not add EF Core attributes to Domain entities.
- Use Fluent API mappings in Infrastructure.
- Repository interfaces stay in Application.
- Repository implementations live in Infrastructure.
- Migrations belong to Infrastructure and must be committed to Git.
- Do not rewrite committed migrations unless explicitly instructed.

## EF Core Tooling Rules

Guildwise uses an Infrastructure design-time DbContext factory for EF Core tooling.

The design-time factory is located at:

```text
src/Guildwise.Infrastructure/Persistence/GuildwiseDbContextFactory.cs
```

EF Core migration commands should use the Infrastructure project as both the migration project and the startup project.

This keeps EF Core design-time tooling out of the Web project.

## Migration Commands

Use this command pattern to add migrations:

```bash
dotnet tool run dotnet-ef migrations add <MigrationName> \
  --project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --startup-project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --context GuildwiseDbContext \
  --output-dir Persistence/Migrations
```

Use this command pattern to update the local database:

```bash
dotnet tool run dotnet-ef database update \
  --project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --startup-project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --context GuildwiseDbContext
```

## Development Startup Migrations

The Web app may apply pending EF Core migrations automatically during local Development startup when Postgres persistence is configured.

This must stay Development-only. Do not enable automatic startup migrations for Production or other non-development environments.

Use EF Core `MigrateAsync()` for this flow. Do not use `EnsureCreated()`.

## Tooling Rules

* Do not add `Microsoft.EntityFrameworkCore.Design` to `Guildwise.Web` just to make EF tooling work with the Web project as startup project.
* Do not use `Guildwise.Web` as the EF tooling startup project unless explicitly instructed.
* Keep EF Core design-time tooling in `Guildwise.Infrastructure`.
* Keep migrations in `Guildwise.Infrastructure`.
* Do not generate migrations into Domain, Application or Web.
* Do not manually edit the EF migration history table.
* Do not delete or rewrite committed migrations unless explicitly instructed.
* Migration files and the EF Core model snapshot must be committed to Git.

## Design-Time Connection String

The design-time factory should first check this environment variable:

```text
GUILDWISE_CONNECTION_STRING
```

If the environment variable is not set, the factory may use a local development fallback connection string.

Current local Docker PostgreSQL setup:

```text
Host=localhost;Port=55432;Database=guildwise;Username=guildwise;Password=guildwise
```

The Docker container still uses PostgreSQL port `5432` internally.

The Windows host port is intentionally `55432`.

Do not change the local PostgreSQL host port back to `5432` unless explicitly instructed.

## Local PostgreSQL Ports

Local development uses this Docker port mapping:

```text
55432:5432
```

Meaning:

```text
Host/Rider/.NET app: localhost:55432
Docker container:    postgres:5432
```

Use this connection string for local development:

```text
Host=localhost;Port=55432;Database=guildwise;Username=guildwise;Password=guildwise
```

Use this JDBC URL for Rider database tooling:

```text
jdbc:postgresql://127.0.0.1:55432/guildwise?user=guildwise&password=guildwise&sslmode=disable
```

## AI Agent Rules

AI agents must not change the PostgreSQL host port back to `5432`.

AI agents must not add EF Core packages to Domain or Application.

AI agents must not add EF Core attributes to Domain entities.

AI agents must not add `Microsoft.EntityFrameworkCore.Design` to `Guildwise.Web` unless explicitly instructed.

AI agents must use the Infrastructure design-time factory for EF Core migration commands.

AI agents must not add external API clients, credentials, background jobs or sync engines without an explicit feature and ADR or feature documentation.

AI agents must not put external API DTOs in Domain.

AI agents must not make Application depend on concrete API clients.

AI agents must not make Web call external APIs directly.

AI agents must keep future roster, raid event, signup, attendance and performance features external-source-ready when those areas can later be fed by WoWAudit, Blizzard WoW API, Raider.IO or Warcraft Logs.

WoWAudit, Blizzard WoW API, Raider.IO and Warcraft Logs are planned future sources, but must not be implemented without explicit implementation scope.

## Result Handling Rules

* Command handlers should return `Result` or `Result<T>` for expected outcomes.
* Query handlers may return `null` for missing single entities or empty collections for lists.
* Do not use exceptions as normal control flow for expected command outcomes.
* Do not catch broad exceptions and convert them into failures.
* Keep `Result`, `Result<T>`, `Failure` and `FailureType` in Application.
* Domain must not reference Application result types.
* Technical failures may still throw.

## Transaction Boundary Rules

* Use `ITransactionRunner` for Application-level multi-aggregate persistence operations.
* Keep EF transaction APIs in Infrastructure.
* Do not introduce EF references into Application.
* Do not start transactions for expected pre-check failures such as `NotFound`.
* Technical exceptions inside transactions should roll back and rethrow.
* Do not redesign into a large UnitOfWork abstraction unless explicitly requested.

## Testing Rules

- Add or update tests for meaningful domain or application behavior.
- Unit tests should be fast and isolated.
- Web component tests should use bUnit for local component logic, conditional rendering, parameters and callbacks.
- bUnit tests do not replace browser or end-to-end tests; Playwright tests are tracked separately.
- Run Web component tests locally with `dotnet test tests/Guildwise.Web.Tests/Guildwise.Web.Tests.csproj`.
- Playwright E2E tests live in `tests/Guildwise.E2ETests`.
- The current E2E host uses InMemory persistence and must not touch a developer PostgreSQL database.
- Run local E2E tests with `dotnet test tests/Guildwise.E2ETests/Guildwise.E2ETests.csproj`.
- See `docs/testing/ui-tests.md` for local Playwright setup and execution.
- Playwright smoke tests run in a separate merge-blocking CI job and remain outside coverage.
- The Playwright CI job installs Chromium and uploads app logs, trace and screenshot as `playwright-artifacts` on failure.
- New full browser flows should use the existing Playwright diagnostic support.
- Playwright tests must not use developer databases or external APIs.
- Integration tests should verify that important flows work through real application wiring.
- Architecture tests should enforce layer boundaries.
- CI runs architecture tests without coverage, and runs unit, Web component and integration tests with coverage collection.
- CI merges unit, Web component and integration coverage into HTML, text and Cobertura reports and uploads them as the `coverage-report` artifact for 14 days.
- The same project-specific test commands and local `reportgenerator` tool can be used to reproduce coverage locally.
- Coverage currently has no hard minimum threshold; low coverage must not be hidden with unjustified product-code exclusions.
- Coverage is a visibility signal and does not replace meaningful behavior tests.
- Add bUnit coverage for new relevant Blazor component logic.
- After changes, run:

```bash
dotnet build
dotnet test
```

## Code Quality Guardrails

Guildwise uses `tools/check-code-quality.ps1` to enforce lightweight file-size guardrails and known-debt baselines.

Solution-wide build and analyzer policy lives in `Directory.Build.props`.

- Nullable reference types, implicit usings, .NET analyzers and `AnalysisLevel` are configured centrally.
- Build, compiler, Roslyn analyzer and code-analysis warnings are treated as errors.
- NuGet Audit checks direct and transitive dependencies and blocks known vulnerabilities from `moderate` severity upward.
- CI runs `dotnet format Guildwise.sln --verify-no-changes --severity error --no-restore` to block only error-level format and charset issues.
- `EnforceCodeStyleInBuild` and broader `dotnet format` style gates are not enabled yet.
- Pull requests are checked for whitespace errors in the actual PR diff.
- Dependabot monitors NuGet and GitHub Actions dependencies weekly against `dev`.

- Run `pwsh -NoProfile -File tools/check-code-quality.ps1` before completing implementation work.
- Do not create new large files that violate the current gates.
- Do not let known-debt files grow beyond `tools/code-quality-baseline.json`.
- Do not raise baseline limits without explicit user approval.
- Do not bypass quality gates to land a feature.
- If a feature would grow an already large file, refactor first or provide a focused refactor plan.

## Changelog Rules

Guildwise uses `CHANGELOG.md` to track notable changes.

For every feature, fix, architectural change, tooling change or meaningful refactor, update the `[Unreleased]` section of `CHANGELOG.md`.

Use these sections:

* `Added` for new features, new capabilities, new tests or new infrastructure.
* `Changed` for refactors or behavior changes.
* `Fixed` for bug fixes.
* `Removed` for removed functionality.

Do not add trivial implementation details to the changelog.

Good changelog entries:

* Added manual guild roster domain model.
* Added application use case handlers for manual guild roster setup.
* Added temporary in-memory storage for guild and player aggregate roots.
* Added minimal Blazor verification UI for manual guild roster setup.
* Refactored the Application layer into explicit use case handlers.

Bad changelog entries:

* Renamed a local variable.
* Reformatted a file.
* Added an empty class.
* Changed whitespace.

AI agents must update `CHANGELOG.md` as part of the same change when implementing a feature slice.

## Coding Style

- Prefer clear domain language over generic technical names.
- Keep features small and vertical.
- Prefer simple code over premature abstraction.
- Avoid large speculative frameworks.
- Do not add dependencies unless they are needed for the current feature.
- Use file-scoped namespaces.
- Use nullable reference types properly.
- Prefer explicit domain methods over public mutable collections.

## Agent Workflow

Before making changes:

1. Read this file.
2. Read the relevant feature file in `docs/features`.
3. Check existing architecture decisions in `docs/adr`.
4. Keep the change focused.
5. Add or update tests.
6. Run build and tests if possible.
7. Summarize changed files and important decisions.

## Versioning Rules

Guildwise uses GitVersion for version calculation.

- Do not manually hardcode or increment version numbers in project files unless explicitly requested.
- Version numbers are calculated from Git history, branches and tags.
- `main` contains stable release history.
- `dev` contains integration builds and produces pre-release versions.
- `feature/*` branches are used for individual feature work.
- Real releases are created by tagging `main` with tags like `v0.1.0`, `v0.2.0` or `v1.0.0`.
- Keep `CHANGELOG.md` updated for notable user-facing, architectural or tooling changes.
- Do not create Git tags unless explicitly instructed by the user.

Useful commands:

```bash
dotnet gitversion /showvariable FullSemVer
dotnet build
dotnet test
