# Feature 0003: Developer Workflow and Runtime Hardening

## Tracking

GitHub Issue: #3
Branch: `feature/0003-developer-workflow-runtime-hardening`
Milestone: `v0.3.0`

## Goal

Improve Guildwise's developer setup, runtime configuration safety and automated validation after introducing PostgreSQL persistence.

After this feature, a developer should be able to clone the repository, start the local database, apply migrations, run the Web app and execute the test suite from documented steps.

The project should also fail fast for unsafe production persistence configuration and run build/test validation automatically in CI.

Feature 0002 introduced persistent PostgreSQL storage. Feature 0003 makes that foundation easier to use, harder to misconfigure and safer to maintain.

## User Value

This feature is primarily developer-facing.

After this feature, a developer should be able to:

* Understand the project setup from documentation.
* Start the local PostgreSQL database.
* Apply EF Core migrations.
* Run the Web app locally.
* Run the full test suite.
* Understand that integration tests use Testcontainers and require Docker.
* Trust that CI validates build and tests automatically.
* Avoid accidentally running production-like environments with unsafe in-memory persistence.

## Technical Value

This feature improves:

* Developer onboarding.
* Local development reproducibility.
* Runtime configuration safety.
* Continuous validation.
* Repository hygiene.
* Long-term maintainability.

## In Scope

This feature includes:

* Add or improve developer setup documentation.
* Document required tools.
* Document local PostgreSQL setup.
* Document EF Core migration commands.
* Document Web startup commands.
* Document test execution.
* Document Testcontainers usage.
* Document GitVersion basics for local version checks.
* Add production-safe persistence configuration behavior.
* Prevent unsafe Production startup with missing or in-memory persistence configuration.
* Add GitHub Actions CI for restore, build and test.
* Add `.gitattributes` if appropriate to normalize line endings.
* Update `CHANGELOG.md` under `[Unreleased]`.

## Out of Scope

This feature does not include:

* Authentication.
* Authorization.
* Battle.net login.
* Raider.IO integration.
* Blizzard API integration.
* Warcraft Logs integration.
* Discord bot.
* Final dashboard UI polish.
* Async repository refactor.
* Application error/result pattern refactor.
* UI error handling redesign.
* Production deployment.
* Dockerizing the Web app.
* Database backup/restore strategy.
* Kubernetes or cloud deployment.
* Secrets management for production infrastructure.
* Observability, logging dashboards or metrics.
* End-user documentation.

## Current State

Feature 0002 delivered:

* PostgreSQL persistence through EF Core.
* Npgsql provider.
* `GuildwiseDbContext`.
* Fluent API mappings.
* Initial EF Core migration.
* EF-backed aggregate-root repositories.
* Configurable Web persistence provider.
* Local Docker Compose PostgreSQL setup.
* Testcontainers-based PostgreSQL integration tests.
* Manual UI restart verification for persisted roster data.

Current version state after Feature 0002:

* `main`: `v0.2.0`
* `dev`: `0.3.0-dev.*`

## Architecture Rules

The existing architecture rules remain in force.

* Domain must not reference EF Core.
* Domain must not contain EF Core attributes.
* Application must not reference EF Core.
* Infrastructure owns EF Core and PostgreSQL.
* Web configures persistence only through dependency injection.
* Web must not inject `DbContext` into components.
* Web must not inject repositories directly into components.
* Blazor components should use Application handlers.
* Repository interfaces remain in Application.
* Repository implementations live in Infrastructure.
* Do not create repositories for:

    * `Character`
    * `RaidTeam`
    * `GuildMember`
    * `RaidTeamMember`

## Runtime Configuration Rules

Guildwise currently supports configurable persistence through:

```json
{
  "Guildwise": {
    "PersistenceProvider": "Postgres"
  }
}
```

Supported values:

* `InMemory`
* `Postgres`

### Development

Development may use:

* `Postgres` for realistic local testing.
* `InMemory` for quick manual experiments if explicitly configured.

The committed Development configuration may contain local-only Docker credentials.

Example local-only connection string:

```text
Host=localhost;Port=55432;Database=guildwise;Username=guildwise;Password=guildwise
```

This is acceptable only for local development.

### Production

Production must not silently fall back to `InMemory`.

Production-like environments should fail fast if:

* `Guildwise:PersistenceProvider` is missing.
* `Guildwise:PersistenceProvider` is invalid.
* `Guildwise:PersistenceProvider` is `InMemory`.
* `Guildwise:PersistenceProvider` is `Postgres` but the required connection string is missing.

The error message should clearly explain the invalid configuration.

Production secrets must not be committed to Git.

## Developer Documentation Requirements

Add or update developer documentation.

Preferred location:

```text
README.md
```

Alternative or additional location:

```text
docs/development.md
```

Documentation should include:

* Required .NET SDK.
* Required Docker / Docker Desktop.
* Required local .NET tools.
* How to restore tools.
* How to start local PostgreSQL.
* How to apply EF Core migrations.
* How to run the Web app.
* How to run build.
* How to run tests.
* How to check GitVersion.
* How to reset the local development database.
* How to troubleshoot common local PostgreSQL issues.

## Required Commands To Document

### Restore

```bash
dotnet restore Guildwise.sln
```

### Build

```bash
dotnet build Guildwise.sln
```

### Test

```bash
dotnet test Guildwise.sln
```

### Start Local PostgreSQL

```bash
docker compose up -d
```

### Stop Local PostgreSQL

```bash
docker compose down
```

### Reset Local PostgreSQL Data

```bash
docker compose down -v
docker compose up -d
```

### Apply EF Core Migrations

```bash
dotnet tool run dotnet-ef database update \
  --project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --startup-project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --context GuildwiseDbContext
```

### Run Web App

```bash
dotnet run --project ./src/Guildwise.Web/Guildwise.Web.csproj
```

### Check GitVersion

```bash
dotnet gitversion /showvariable FullSemVer
```

## Testcontainers Documentation

Integration tests use Testcontainers PostgreSQL.

Documentation must mention:

* Integration tests require Docker.
* Integration tests do not use the local Docker Compose database.
* Integration tests do not use local port `55432`.
* Testcontainers starts its own PostgreSQL container.
* EF Core migrations are applied to the test database automatically.
* If Docker is not running, integration tests may fail.

## CI Requirements

Add GitHub Actions CI.

Suggested workflow file:

```text
.github/workflows/ci.yml
```

The CI should run on:

* Pull requests into `dev`
* Pull requests into `main`
* Pushes to `dev`
* Pushes to `main`

The CI should:

* Check out the repository.
* Set up the required .NET SDK.
* Restore dependencies.
* Build the solution.
* Run tests.

Required commands:

```bash
dotnet restore Guildwise.sln
dotnet build Guildwise.sln --no-restore
dotnet test Guildwise.sln --no-build
```

Because integration tests use Testcontainers, CI must run in an environment with Docker available.

GitHub-hosted Linux runners are preferred initially.

## Line Ending Normalization

If line-ending warnings continue, add `.gitattributes`.

Goal:

* Avoid noisy LF/CRLF warnings.
* Keep text files normalized.
* Avoid accidental large diffs caused only by line endings.

Suggested `.gitattributes` policy:

```gitattributes
* text=auto

*.cs text eol=crlf
*.csproj text eol=crlf
*.sln text eol=crlf
*.props text eol=crlf
*.targets text eol=crlf

*.md text eol=crlf
*.json text eol=crlf
*.editorconfig text eol=crlf

*.yml text eol=lf
*.yaml text eol=lf
*.sh text eol=lf

*.png binary
*.jpg binary
*.jpeg binary
*.gif binary
*.ico binary
*.pdf binary
```

Only normalize line endings deliberately.

Do not mix line-ending normalization with unrelated code changes if it creates a very large diff.

## Suggested Implementation Slices

### 0003a: Developer Setup Documentation

Implement:

* Add or update developer setup documentation.
* Document required tools.
* Document local database startup.
* Document migration command.
* Document Web startup.
* Document tests.
* Document Testcontainers and Docker requirement.
* Document local DB reset.
* Update `CHANGELOG.md`.

Suggested commit:

```text
docs: add developer setup guide
```

### 0003b: Production Persistence Fail-Fast

Implement:

* Update Web persistence provider selection.
* Keep Development behavior convenient.
* Prevent Production from silently using InMemory.
* Throw clear startup errors for invalid Production persistence configuration.
* Add tests if a suitable test location exists.
* Update `CHANGELOG.md`.

Suggested commit:

```text
fix: fail fast for unsafe production persistence configuration
```

### 0003c: GitHub Actions Build/Test CI

Implement:

* Add GitHub Actions workflow.
* Run restore, build and test.
* Ensure Testcontainers tests can run in CI.
* Update documentation if CI behavior needs explanation.
* Update `CHANGELOG.md`.

Suggested commit:

```text
ci: add build and test workflow
```

### 0003d: Line Ending Normalization

Implement:

* Add `.gitattributes` if appropriate.
* Normalize line endings carefully.
* Avoid unrelated changes.
* Update `CHANGELOG.md`.

Suggested commit:

```text
chore: normalize repository line endings
```

## Acceptance Criteria

### Developer Documentation

* Developer setup documentation exists.
* Documentation explains required .NET SDK.
* Documentation explains Docker requirement.
* Documentation explains local PostgreSQL startup.
* Documentation includes EF Core migration command.
* Documentation includes Web startup command.
* Documentation includes build command.
* Documentation includes test command.
* Documentation explains that integration tests use Testcontainers.
* Documentation explains local database reset.

### Runtime Configuration

* Development can still use PostgreSQL through configuration.
* Development can still use InMemory if explicitly configured.
* Production does not silently fall back to InMemory.
* Invalid persistence provider values fail with a clear startup error.
* Missing Production persistence configuration fails with a clear startup error.
* Missing Production PostgreSQL connection string fails with a clear startup error.

### CI

* GitHub Actions workflow exists.
* CI runs restore.
* CI runs build.
* CI runs tests.
* CI works with Testcontainers-based integration tests.
* CI is triggered for relevant branches or pull requests.

### Line Endings

* `.gitattributes` exists if needed.
* LF/CRLF warnings are reduced or eliminated.
* Line-ending normalization does not hide unrelated code changes.

### Architecture

* Domain remains EF-free.
* Application remains EF-free.
* EF Core code remains in Infrastructure.
* Web remains composition root for persistence selection.
* Blazor components remain behind Application handlers.
* Architecture tests pass.

### Validation

* `dotnet build Guildwise.sln` succeeds.
* `dotnet test Guildwise.sln` succeeds.
* `CHANGELOG.md` is updated under `[Unreleased]`.

## Manual Verification

After implementation, verify locally:

1. Restore packages:

   ```bash
   dotnet restore Guildwise.sln
   ```

2. Start local PostgreSQL:

   ```bash
   docker compose up -d
   ```

3. Apply migrations:

   ```bash
   dotnet tool run dotnet-ef database update \
     --project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
     --startup-project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
     --context GuildwiseDbContext
   ```

4. Run the Web app:

   ```bash
   dotnet run --project ./src/Guildwise.Web/Guildwise.Web.csproj
   ```

5. Run build:

   ```bash
   dotnet build Guildwise.sln
   ```

6. Run tests:

   ```bash
   dotnet test Guildwise.sln
   ```

7. Check version:

   ```bash
   dotnet gitversion /showvariable FullSemVer
   ```

## Risks and Open Questions

### CI and Testcontainers

GitHub Actions should support Docker on hosted Linux runners, but Testcontainers startup can fail if Docker is unavailable or restricted.

If CI fails because Docker is unavailable, options include:

* Configure the runner correctly.
* Split integration tests from unit tests.
* Add a separate CI job for integration tests.
* Use a PostgreSQL service container instead of Testcontainers for CI only.

The preferred initial approach is to keep tests unchanged and let Testcontainers manage PostgreSQL.

### Production Configuration

Production deployment is out of scope, but Production runtime behavior should still be safe.

Fail-fast configuration should not require real production secrets in Git.

### Documentation Drift

Developer documentation can become outdated.

AGENTS.md should remind AI agents to update developer documentation when setup commands or local infrastructure change.

### Line Ending Normalization

Adding `.gitattributes` can cause a large one-time diff if many files need renormalization.

If this happens, keep the normalization commit separate and avoid mixing it with code changes.

## Done Definition

This feature is done when:

* Developer setup documentation exists and is accurate.
* Local PostgreSQL setup is documented.
* EF migration commands are documented.
* Web startup is documented.
* Test execution is documented.
* Testcontainers and Docker requirements are documented.
* Production persistence configuration fails fast when unsafe.
* GitHub Actions CI runs restore, build and test.
* Line-ending behavior is normalized or intentionally documented.
* Domain remains EF-free.
* Application remains EF-free.
* Web remains the composition root for persistence selection.
* `dotnet build Guildwise.sln` succeeds.
* `dotnet test Guildwise.sln` succeeds.
* `CHANGELOG.md` has an entry under `[Unreleased]`.
