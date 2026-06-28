# ADR 0005: Persistence Strategy

## Status

Accepted

## Context

Guildwise needs persistent storage for manually created guild roster data.

Feature 0001 introduced the manual guild roster foundation and temporary in-memory storage. This allowed the application to prove the first vertical slice, but data is lost when the application restarts.

Feature 0002 introduces real persistence.

The persistence solution must support the current modular monolith architecture:

* Domain contains business rules and must stay independent.
* Application contains use cases and repository interfaces.
* Infrastructure contains technical persistence implementation.
* Web configures persistence through dependency injection.

Guildwise currently has two aggregate roots:

* `Guild`
* `Player`

Characters are managed through `Player`.

Raid teams and guild members are managed through `Guild`.

The persistence model must respect these aggregate boundaries.

## Decision

Guildwise will use:

* PostgreSQL as the primary relational database.
* EF Core as the ORM.
* Npgsql as the PostgreSQL EF Core provider.
* EF Core Migrations for schema evolution.
* Fluent API mappings in Infrastructure.

EF Core code belongs only in `Guildwise.Infrastructure`.

The Domain and Application projects must not reference EF Core.

## Persistence Ownership

### Domain

The Domain layer must remain persistence-ignorant.

Domain entities must not contain:

* EF Core attributes
* ASP.NET Core attributes
* database-specific logic
* public setters only for persistence
* infrastructure DTOs
* external API DTOs

Domain collections should remain protected from arbitrary external mutation.

### Application

The Application layer defines persistence ports.

Repository interfaces remain in Application:

```text
IGuildRepository
IPlayerRepository
```

Application handlers use these interfaces to load and save aggregate roots.

Application must not reference:

* EF Core
* Npgsql
* DbContext
* database migrations
* Infrastructure implementations

### Infrastructure

The Infrastructure layer owns persistence implementation.

Infrastructure contains:

* `GuildwiseDbContext`
* EF Core entity configurations
* EF-backed repository implementations
* EF Core migrations
* database registration through dependency injection

Infrastructure implements Application repository interfaces.

### Web

The Web layer is the composition root.

Web may call Infrastructure dependency injection extension methods.

Web must not directly use:

* `GuildwiseDbContext`
* EF repository implementations
* Npgsql-specific types

Blazor components should use Application handlers, not repositories or DbContexts.

## Repository Strategy

Guildwise uses aggregate-root repositories only.

Allowed repository interfaces:

```text
IGuildRepository
IPlayerRepository
```

Do not create repositories for:

* `Character`
* `RaidTeam`
* `GuildMember`
* `RaidTeamMember`

Reason:

* Characters are owned and managed through `Player`.
* Raid teams and guild members are owned and managed through `Guild`.
* Creating repositories for child entities would weaken aggregate boundaries and make it easier to bypass domain rules.

## EF Core Mapping Strategy

EF Core mappings must use Fluent API configuration classes in Infrastructure.

Suggested structure:

```text
src/Guildwise.Infrastructure/
  Persistence/
    GuildwiseDbContext.cs
    Configurations/
      GuildConfiguration.cs
      PlayerConfiguration.cs
      CharacterConfiguration.cs
      GuildMemberConfiguration.cs
      RaidTeamConfiguration.cs
      RaidTeamMemberConfiguration.cs
```

Domain entities must not use EF Core attributes such as:

```text
[Key]
[Required]
[ForeignKey]
[Owned]
```

If EF Core requires additional support for mapping rich domain objects, prefer:

* private parameterless constructors
* backing field mappings
* Fluent API relationship configuration
* explicit value conversions for enums

Do not make domain collections publicly mutable just to satisfy EF Core.

## Enum Persistence

Domain enums may be persisted as strings or integers.

The preferred default is string persistence for readability where practical.

Relevant enums:

* `GuildRank`
* `AdditionalGuildRole`
* `CharacterClass`
* `CharacterRole`
* `CharacterSpecialization`

If string enum persistence becomes inconvenient or inefficient later, this decision may be revisited.

## Migration Strategy

Guildwise uses EF Core Migrations to manage database schema evolution.

Migrations belong to Infrastructure.

Suggested location:

```text
src/Guildwise.Infrastructure/Persistence/Migrations/
```

Migration files must be committed to Git.

The migration history has two parts:

1. Source-controlled migration files in the repository.
2. The EF Core migration history table in the PostgreSQL database.

By default, EF Core uses this database table:

```text
__EFMigrationsHistory
```

Rules:

* Add a new migration whenever the persisted model changes.
* Use descriptive migration names.
* Commit generated migration files.
* Commit the model snapshot file.
* Do not manually edit the database migration history table.
* Do not delete or rewrite committed migrations unless explicitly instructed.
* Do not generate migrations into Domain, Application or Web.

Example migration names:

```text
InitialGuildRosterPersistence
AddRecruitingFields
AddAttendanceTracking
AddLootWishlist
```

## EF Core Tooling Strategy

Guildwise uses an Infrastructure design-time DbContext factory for EF Core tooling.

The factory is located in Infrastructure:

```text
src/Guildwise.Infrastructure/Persistence/GuildwiseDbContextFactory.cs
```

This allows EF Core migration commands to run without using `Guildwise.Web` as the startup project.

This keeps EF Core design-time tooling out of the Web project.

## Design-Time Factory

The design-time factory creates `GuildwiseDbContext` for EF Core tooling.

It should first check this environment variable:

```text
GUILDWISE_CONNECTION_STRING
```

If the environment variable is not set, the factory may use a local development fallback connection string.

Current local development fallback connection string:

```text
Host=localhost;Port=55432;Database=guildwise;Username=guildwise;Password=guildwise
```

The local Docker setup intentionally maps PostgreSQL like this:

```text
55432:5432
```

Meaning:

```text
Host/Rider/.NET app: localhost:55432
Docker container:    postgres:5432
```

The host port `55432` is used to avoid conflicts with other local PostgreSQL installations or stale port mappings on the default PostgreSQL port `5432`.

## Migration Commands

Migration commands should use the Infrastructure project as both the migration project and the startup project.

Example migration command:

```bash
dotnet tool run dotnet-ef migrations add InitialGuildRosterPersistence \
  --project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --startup-project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --context GuildwiseDbContext \
  --output-dir Persistence/Migrations
```

Example database update command:

```bash
dotnet tool run dotnet-ef database update \
  --project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --startup-project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
  --context GuildwiseDbContext
```

## Rules

* Do not add `Microsoft.EntityFrameworkCore.Design` to `Guildwise.Web` just to satisfy EF Core tooling.
* Keep EF Core design-time tooling in `Guildwise.Infrastructure`.
* Do not use `Guildwise.Web` as the EF tooling startup project unless explicitly instructed.
* Keep migrations in Infrastructure.
* Migration files must be committed to Git.
* The EF Core model snapshot must be committed to Git.
* Do not manually edit the EF migration history table.
* Do not delete or rewrite committed migrations unless explicitly instructed.
* Do not generate migrations into Domain, Application or Web.
* Do not change the local PostgreSQL host port back to `5432` unless explicitly instructed.

## Rider Database Connection

For Rider database tooling, use:

```text
Host:      127.0.0.1
Port:      55432
Database:  guildwise
User:      guildwise
Password:  guildwise
```

Or use this JDBC URL:

```text
jdbc:postgresql://127.0.0.1:55432/guildwise?user=guildwise&password=guildwise&sslmode=disable
```

## Consequences

### Positive

* EF Core tooling does not require EF design-time packages in the Web project.
* Persistence tooling remains owned by Infrastructure.
* Migration commands are explicit and repeatable.
* The local PostgreSQL port avoids conflicts with other local PostgreSQL instances.
* The design-time factory can be configured through an environment variable for CI or non-local environments.

### Negative

* EF Core commands are longer because both `--project` and `--startup-project` point to Infrastructure.
* Developers must remember that the host port is `55432`, not `5432`.
* Documentation and prompts must consistently use the Infrastructure design-time factory approach.

## Local Development Database

Guildwise will use PostgreSQL locally.

The preferred local development setup is Docker Compose.

Development credentials may be simple and committed only if they are clearly local-only.

Example local development connection string:

```json
{
  "ConnectionStrings": {
    "GuildwiseDatabase": "Host=localhost;Port=5432;Database=guildwise;Username=guildwise;Password=guildwise"
  }
}
```

Production secrets must not be committed.

## Dependency Injection Strategy

Infrastructure should expose explicit dependency injection methods.

Possible structure:

```csharp
services.AddInMemoryInfrastructure();
services.AddPostgresInfrastructure(configuration);
```

or:

```csharp
services.AddInfrastructure(configuration);
```

where PostgreSQL becomes the default runtime registration.

If both in-memory and PostgreSQL implementations remain available, the method names must make the selected persistence mode clear.

Application handler registration remains in Application.

Infrastructure persistence registration remains in Infrastructure.

## Testing Strategy

Persistence behavior should be tested separately from pure Domain and Application behavior.

Relevant tests:

* Save and load guild.
* Save and load player.
* Save and load player with characters.
* Save and load main character assignment.
* Save and load guild members.
* Save and load additional guild roles.
* Save and load raid teams.
* Save and load raid team members.

Persistence tests should ideally run against real PostgreSQL.

Acceptable approaches:

* Testcontainers PostgreSQL
* Docker Compose test database
* Local development database for early manual testing

In-memory repositories may remain useful for fast tests, but they do not replace persistence integration tests.

## Consequences

### Positive

* PostgreSQL provides a strong relational foundation for roster data.
* EF Core reduces manual persistence boilerplate.
* EF Core Migrations provide a source-controlled schema history.
* Infrastructure owns persistence details.
* Domain and Application remain clean.
* Aggregate boundaries remain explicit.

### Negative

* EF Core mapping of rich domain models requires careful configuration.
* Private collections and controlled constructors may require Fluent API backing-field mappings.
* Repository interfaces may need to become asynchronous.
* Persistence tests require database setup.
* Migrations must be managed carefully.

## Alternatives Considered

### Dapper

Dapper is lightweight and gives direct SQL control.

It was not selected as the primary persistence approach because Guildwise benefits from:

* change tracking
* relationship mapping
* migrations
* reduced persistence boilerplate
* easier early development

Dapper may still be considered later for specialized read models or reporting queries.

### NHibernate

NHibernate is a mature full ORM.

It was not selected because EF Core is simpler to integrate into a modern ASP.NET Core project and is sufficient for Guildwise's current needs.

### In-Memory Storage

In-memory storage was useful for Feature 0001.

It is not sufficient for real usage because data is lost when the application restarts.

It may remain available for tests, demos or local verification.

### SQLite

SQLite would be simpler for local development.

It was not selected as the primary database because Guildwise is intended to use PostgreSQL-style relational persistence from the start.

## Follow-Up Actions

* Add EF Core and Npgsql packages.
* Add `GuildwiseDbContext`.
* Add Fluent API entity configurations.
* Add initial migration.
* Add EF-backed `IGuildRepository` and `IPlayerRepository`.
* Add PostgreSQL local development setup.
* Add persistence integration tests.
* Update `CHANGELOG.md`.
* Keep architecture tests passing.
