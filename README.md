# Guildwise

Guildwise is a modular monolith built with ASP.NET Core Blazor and a pragmatic Clean Architecture style. The current product focus is manual guild, raid team and roster management.

## Required Tools

- .NET SDK: the projects currently target `net10.0`, so install the .NET 10 SDK. This repository does not currently contain a `global.json`; if one is added later, its `sdk.version` is the expected SDK version.
- Docker or Docker Desktop.
- Local .NET tools restored from `dotnet-tools.json`.
- PostgreSQL for local development is provided through Docker Compose.
- Integration tests use Testcontainers PostgreSQL and require Docker.

## Restore

Run from the repository root:

```bash
dotnet restore Guildwise.sln
dotnet tool restore
```

## Build And Test

```bash
dotnet build Guildwise.sln
dotnet test Guildwise.sln
```

`dotnet test` runs unit, architecture and integration tests. The integration tests start their own PostgreSQL container through Testcontainers, apply EF Core migrations to that test database automatically, and do not use the local Docker Compose database or host port `55432`.

## Local PostgreSQL

Start the local development database:

```bash
docker compose up -d
docker compose ps
```

Stop it without deleting data:

```bash
docker compose down
```

Local development connection details:

```text
Host:      localhost
Port:      55432
Database:  guildwise
Username:  guildwise
Password:  guildwise
```

These credentials are for local development only. Do not use them for production.

The Docker container listens on PostgreSQL port `5432` internally, but the Windows host port is intentionally `55432`:

```text
Host/Rider/.NET app: localhost:55432
Docker container:    postgres:5432
```

## Apply EF Core Migrations

Migrations are owned by `Guildwise.Infrastructure`. Use the Infrastructure project as both the migration project and the startup project:

```bash
dotnet tool run dotnet-ef database update \
  --project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --startup-project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --context GuildwiseDbContext
```

PowerShell:

```powershell
dotnet tool run dotnet-ef database update `
  --project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj `
  --startup-project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj `
  --context GuildwiseDbContext
```

The Web startup does not automatically run migrations. Do not use `Guildwise.Web` as the EF tooling startup project unless explicitly instructed. Do not add `Microsoft.EntityFrameworkCore.Design` to `Guildwise.Web` just for EF tooling.

## Run The Web App

Development configuration uses:

```text
Guildwise:PersistenceProvider = Postgres
```

Start PostgreSQL, apply migrations, then run:

```bash
dotnet run --project ./src/Guildwise.Web/Guildwise.Web.csproj
```

PostgreSQL must be running and migrations must be applied before using the persistent Web workflow.

## Runtime Persistence Configuration

Development may use local PostgreSQL or explicitly configured `InMemory` persistence. If `Guildwise:PersistenceProvider` is missing in Development, Guildwise falls back to `InMemory` for quick local experiments.

Non-Development environments must explicitly configure safe persistence:

- `Guildwise:PersistenceProvider` is required.
- `InMemory` is not allowed.
- `Postgres` requires a non-empty `ConnectionStrings:GuildwiseDatabase` value.
- Production secrets must come from environment or deployment configuration, not committed files.

The Web app does not run EF Core migrations automatically in any environment.

## Manual Persistence Verification

1. Start Docker Compose PostgreSQL with `docker compose up -d`.
2. Apply EF Core migrations.
3. Start the Web app.
4. Create a guild, player, character and raid team in the verification UI.
5. Stop the Web app.
6. Start the Web app again.
7. Confirm the data still exists.

## Reset The Local Database

This deletes local development data:

```bash
docker compose down -v
docker compose up -d
```

Re-apply migrations after the database starts.

## GitVersion

Check the calculated version:

```bash
dotnet gitversion /showvariable FullSemVer
```

Expected branch behavior:

- `main` shows stable release versions such as `0.2.0`.
- `dev` shows development versions such as `0.3.0-dev.*`.
- Do not manually edit project version numbers for releases.

## Troubleshooting

Docker is not running:
Start Docker or Docker Desktop, then rerun the failing `docker compose` or `dotnet test` command.

PostgreSQL port conflict:
Guildwise uses host port `55432`. If Docker Compose cannot bind the port, check for another process or container already using `55432`.

Wrong PostgreSQL port:
Use `localhost:55432` from the host. Do not change local development commands back to `5432`; `5432` is only the container-internal port.

Integration tests fail because Docker is unavailable:
Start Docker. Testcontainers needs Docker and does not use the local Compose database.

EF migration command cannot create `GuildwiseDbContext`:
Run `dotnet tool restore`, confirm PostgreSQL is running on `localhost:55432`, and use the Infrastructure project for both `--project` and `--startup-project`.

Local database has stale dummy data:
Reset the local database with `docker compose down -v`, start it again with `docker compose up -d`, then re-apply migrations.
