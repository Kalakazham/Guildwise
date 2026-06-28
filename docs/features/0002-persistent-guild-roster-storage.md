# Feature 0002: Persistent Guild Roster Storage

## Tracking

GitHub Issue: #2  
Branch: `feature/0002-persistent-guild-roster-storage`  
Milestone: `v0.2.0`

## Goal

As a guild or raid organizer, I want Guildwise to persist manually created roster data, so that guilds, players, characters and raid teams survive application restarts.

Feature 0001 introduced the manual guild roster foundation with temporary in-memory storage.

Feature 0002 replaces or supplements that temporary storage with real PostgreSQL persistence using EF Core.

The goal is to establish a persistence foundation for future features.

## User Value

After this feature, a user should be able to:

- Create a guild.
- Create players.
- Create characters for players.
- Select main characters.
- Create raid teams.
- Add players to guilds.
- Add players to raid teams.
- Restart the application.
- Still see the previously created data.

This makes the manual roster setup actually usable beyond a single runtime session.

## Technical Decision

Guildwise will use:

- PostgreSQL as the database.
- EF Core as the ORM.
- Npgsql as the PostgreSQL EF Core provider.
- EF Core migrations for schema evolution.
- Fluent API mappings in Infrastructure.

The Domain layer must remain free from EF Core attributes.

## In Scope

This feature includes:

- Add EF Core packages.
- Add Npgsql PostgreSQL provider.
- Add EF Core design-time tooling if needed.
- Add `GuildwiseDbContext` in `Guildwise.Infrastructure`.
- Add EF Core entity configurations using Fluent API.
- Add initial database migration.
- Add EF-backed implementations of:
    - `IGuildRepository`
    - `IPlayerRepository`
- Register EF-backed repositories through Infrastructure dependency injection.
- Configure PostgreSQL connection string.
- Add local development database setup.
- Persist and reload guild roster foundation data.
- Add persistence tests for core save/load flows.
- Keep the existing in-memory repositories if useful for tests or local verification.
- Update `CHANGELOG.md` under `[Unreleased]`.

## Out of Scope

This feature does not include:

- Authentication.
- Authorization middleware.
- Battle.net login.
- Raider.IO integration.
- Blizzard API integration.
- Warcraft Logs integration.
- Discord bot.
- Final dashboard UI polish.
- Attendance tracking.
- Loot wishlist.
- Recruiting workflow.
- Production deployment.
- Multi-tenant hosting.
- Advanced performance optimization.
- Data import from external providers.

## Domain Concepts to Persist

The following domain concepts must be persisted.

### Guild

Persist:

- Id
- Name
- Region
- Realm
- Guild members
- Raid teams

### Player

Persist:

- Id
- DisplayName
- Characters
- MainCharacterId

### Character

Persist:

- Id
- PlayerId
- Name
- Region
- Realm
- CharacterClass
- CharacterSpecialization
- CharacterRole

### Guild Member

Persist:

- Id
- GuildId
- PlayerId
- GuildRank
- AdditionalGuildRoles

### Additional Guild Role

Persist additional roles assigned to a guild member.

Initial roles:

- RaidLead
- Recruiter

### Raid Team

Persist:

- Id
- GuildId
- Name
- Raid team members

### Raid Team Member

Persist:

- Id
- RaidTeamId
- PlayerId

## Architecture Rules

- Domain must not reference EF Core.
- Domain must not contain EF Core attributes.
- Domain must not expose public setters only for EF Core.
- Application must not reference EF Core.
- Infrastructure owns EF Core.
- Web configures persistence only through dependency injection.
- Repository interfaces remain in Application.
- Repository implementations live in Infrastructure.
- Do not create repositories for:
    - Character
    - RaidTeam
    - GuildMember
    - RaidTeamMember
- Characters remain managed through Player.
- RaidTeams and GuildMembers remain managed through Guild.
- EF mappings should respect aggregate boundaries.
- Do not weaken domain invariants for persistence convenience.

## Persistence Design Rules

### Aggregate Roots

The aggregate roots are:

- Guild
- Player

Therefore the only persistence interfaces are:

- `IGuildRepository`
- `IPlayerRepository`

### Repository Implementations

Infrastructure should provide EF-backed implementations such as:

- `EfGuildRepository`
- `EfPlayerRepository`

Temporary in-memory repositories may remain available, but production-style application wiring should use EF-backed repositories once PostgreSQL is configured.

### DbContext

Add:

```text
src/Guildwise.Infrastructure/Persistence/GuildwiseDbContext.cs
```

The DbContext should expose DbSets for aggregate roots and necessary persisted entities as required by EF Core mappings.

Possible DbSets:

```csharp
DbSet<Guild> Guilds
DbSet<Player> Players
```

Additional DbSets may be added if required by EF Core mapping, but repositories should still only target aggregate roots.

### Entity Configurations

Use Fluent API configuration classes.

Suggested structure:

```text
src/Guildwise.Infrastructure/Persistence/Configurations/
  GuildConfiguration.cs
  PlayerConfiguration.cs
  CharacterConfiguration.cs
  GuildMemberConfiguration.cs
  RaidTeamConfiguration.cs
  RaidTeamMemberConfiguration.cs
```

Do not add EF Core attributes to Domain entities.

### Domain Constructors and Backing Fields

If EF Core requires additional constructors or configuration support:

- Prefer private parameterless constructors when needed.
- Prefer backing field configuration when needed.
- Do not make collections publicly mutable.
- Do not add public setters only for EF Core.
- Do not bypass domain methods in application code.

### Enum Persistence

Enums may be stored as strings or integers.

Preferred for readability:

- Store important domain enums as strings if practical.

Relevant enums:

- GuildRank
- AdditionalGuildRole
- CharacterClass
- CharacterRole
- CharacterSpecialization

## Migration History

Guildwise should keep a clear migration history for Infrastructure persistence.

EF Core migrations must be created in the Infrastructure project.

Suggested location:

```text
src/Guildwise.Infrastructure/Persistence/Migrations/
```

Migration files must be committed to Git.

The migration history has two parts:

1. Source-controlled migration files in the repository.
2. The EF Core migration history table in the PostgreSQL database.

EF Core uses the database migration history table to track which migrations have already been applied to a specific database.

By default, EF Core uses:

```text
__EFMigrationsHistory
```

Rules:

- Do not delete or rewrite existing committed migrations unless explicitly instructed.
- Do not manually edit the database migration history table.
- Add a new migration whenever the persisted model changes.
- Migration names should be descriptive.
- Migration files belong to Infrastructure.
- Domain and Application must remain EF-free.
- Generated migration files and the model snapshot belong in Git.

Example migration names:

```text
InitialGuildRosterPersistence
AddRecruitingFields
AddAttendanceTracking
AddLootWishlist
```

For local development, migrations may be applied with:

```bash
dotnet ef database update
```

The exact command may require specifying the Infrastructure project and Web startup project once EF Core is configured.

## Migrations

Add an initial migration for the current domain model.

Suggested migration name:

```text
InitialGuildRosterPersistence
```

Expected migration output structure:

```text
src/Guildwise.Infrastructure/
  Persistence/
    Migrations/
      <timestamp>_InitialGuildRosterPersistence.cs
      <timestamp>_InitialGuildRosterPersistence.Designer.cs
      GuildwiseDbContextModelSnapshot.cs
```

These files must be committed to Git.

## Repository Behavior

### Guild Repository

The EF-backed Guild repository should support:

- Get guild by id.
- List guilds.
- Add guild.
- Remove guild.

It must load enough related data for current application use cases to work.

This includes:

- Guild members
- Additional guild roles
- Raid teams
- Raid team members

### Player Repository

The EF-backed Player repository should support:

- Get player by id.
- List players.
- Add player.
- Remove player.

It must load enough related data for current application use cases to work.

This includes:

- Characters
- Main character assignment

## Async Persistence

EF Core persistence should use async APIs where practical.

This may require changing repository interfaces and application handlers from synchronous methods to asynchronous methods.

If repository interfaces are changed to async:

- Use `Task<T>` or `Task`.
- Add `CancellationToken` parameters where useful.
- Update Application handlers.
- Update UnitTests and IntegrationTests.
- Keep the refactor focused on persistence requirements.

Example repository shape:

```csharp
Task<Guild?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
Task<IReadOnlyCollection<Guild>> ListAsync(CancellationToken cancellationToken = default);
Task AddAsync(Guild guild, CancellationToken cancellationToken = default);
Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
Task SaveChangesAsync(CancellationToken cancellationToken = default);
```

The exact shape should be decided during implementation, but it must remain simple and consistent.

## Local Development Database

This feature should provide a local PostgreSQL setup.

Preferred approach:

- Docker Compose for PostgreSQL.

Suggested service:

```yaml
postgres:
  image: postgres:latest
  environment:
    POSTGRES_DB: guildwise
    POSTGRES_USER: guildwise
    POSTGRES_PASSWORD: guildwise
  ports:
    - "5432:5432"
```

The actual password is acceptable for local development only.

Do not commit production secrets.

## Configuration

Add a connection string in development configuration.

Example:

```json
{
  "ConnectionStrings": {
    "GuildwiseDatabase": "Host=localhost;Port=5432;Database=guildwise;Username=guildwise;Password=guildwise"
  }
}
```

Rules:

- Development connection strings may be committed if they contain only local development credentials.
- Production secrets must not be committed.
- Do not add real credentials to source control.

## Dependency Injection

Infrastructure should expose clear DI methods for persistence registration.

Because Feature 0001 introduced temporary in-memory storage, Feature 0002 should avoid ambiguous registration behavior.

Recommended structure:

```csharp
services.AddInMemoryInfrastructure();
services.AddPostgresInfrastructure(configuration);
```

or:

```csharp
services.AddInfrastructure(configuration);
```

where PostgreSQL becomes the default runtime registration.

Rules:

- If both in-memory and PostgreSQL registrations remain available, method names must make the selected storage explicit.
- The Web project should not know concrete repository classes.
- The Web project should only call Infrastructure DI extension methods.
- Application handler registration remains in Application.
- Infrastructure repository registration remains in Infrastructure.

## Suggested Implementation Slices

This feature should be implemented in small slices.

### 0002a: Persistence Packages and DbContext

Implement:

- EF Core package references.
- Npgsql provider package.
- EF Core design-time package if needed.
- `GuildwiseDbContext`.
- Basic Infrastructure DI registration.
- Connection string configuration.

Do not implement all mappings yet if a smaller step is needed.

Suggested commit:

```text
feat: add EF Core PostgreSQL persistence foundation
```

### 0002b: Entity Mappings and Initial Migration

Implement:

- Fluent API mappings for the current domain model.
- Initial EF Core migration.
- Migration snapshot.
- Schema creation for guild roster foundation data.

Migration files and the model snapshot must be committed.

Suggested commit:

```text
feat: add initial guild roster persistence migration
```

### 0002c: EF Repository Implementations

Implement:

- `EfGuildRepository`
- `EfPlayerRepository`
- repository DI registration
- repository tests

Suggested commit:

```text
feat: add EF-backed guild roster repositories
```

### 0002d: Persistence Integration Tests

Implement tests that prove persisted data can be saved and loaded.

Tests should cover:

- Save and load guild.
- Save and load player.
- Save and load player with characters.
- Save and load main character assignment.
- Save and load guild members.
- Save and load additional guild roles.
- Save and load raid teams.
- Save and load raid team members.

Suggested commit:

```text
test: add persistent guild roster storage tests
```

### 0002e: Web Wiring Verification

Update the existing minimal verification UI to use PostgreSQL-backed persistence.

Verify manually:

- Create data in the UI.
- Restart the app.
- Data is still available.

Manual local verification steps:

1. Start Docker Compose PostgreSQL.
2. Apply migrations:

   ```bash
   dotnet tool run dotnet-ef database update \
     --project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
     --startup-project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
     --context GuildwiseDbContext
   ```

3. Run `Guildwise.Web`.
4. Create a guild, player, character and raid team through the verification UI.
5. Stop and restart `Guildwise.Web`.
6. Confirm the created roster data still exists.

Suggested commit:

```text
feat: wire roster setup UI to persistent storage
```

## Acceptance Criteria

### Database Setup

- PostgreSQL can be started locally.
- Guildwise can connect to the local PostgreSQL database.
- The initial database schema can be created through EF Core migrations.
- The EF Core migration history table exists in the database after migrations are applied.

### Migration History

- Initial migration exists in Infrastructure.
- Migration files are committed to Git.
- `GuildwiseDbContextModelSnapshot.cs` is committed to Git.
- Existing committed migrations are not rewritten or deleted.
- New persisted model changes are represented by new migrations.

### Persistence

- Guilds can be saved and loaded from PostgreSQL.
- Players can be saved and loaded from PostgreSQL.
- Characters can be saved and loaded with their owning player.
- Main character assignments are persisted.
- Guild members are persisted.
- Additional guild roles are persisted.
- Raid teams are persisted.
- Raid team members are persisted.

### Application Behavior

- Existing Application use cases continue to work.
- Existing Domain rules continue to be enforced.
- Existing verification UI continues to work.
- Data survives application restarts.

### Architecture

- Domain remains EF-free.
- Application remains EF-free.
- EF Core code is limited to Infrastructure.
- Repository interfaces remain in Application.
- Repository implementations live in Infrastructure.
- Architecture tests pass.

### Tests

- Existing Domain tests pass.
- Existing Application tests pass.
- Existing Architecture tests pass.
- Persistence tests are added.
- `dotnet build Guildwise.sln` succeeds.
- `dotnet test Guildwise.sln` succeeds.

### Changelog

- `CHANGELOG.md` is updated under `[Unreleased]`.

## Technical Notes

- Use EF Core Fluent API instead of attributes.
- Do not add EF attributes to Domain entities.
- Keep repository count limited to aggregate roots.
- Prefer async persistence APIs if repository interfaces are changed.
- Do not add external API integrations.
- Do not add authentication.
- Do not add production deployment automation.
- Keep the persistence implementation simple and explicit.
- Do not optimize prematurely.
- Do not rewrite existing migrations unless explicitly instructed.
- Do not manually edit the EF migration history table.

## Risks and Open Questions

### EF Core Mapping of Rich Domain Model

The current Domain model uses private collections and controlled methods.

EF Core mapping may require:

- private constructors
- backing field mappings
- careful relationship configuration

Do not weaken domain encapsulation without a clear reason.

### Repository Async Refactor

Introducing EF Core may require converting repository interfaces and handlers to async.

This is acceptable if done consistently and covered by tests.

### In-Memory vs PostgreSQL Wiring

In-memory repositories may remain useful for tests or demos.

However, the default application runtime should use PostgreSQL-backed persistence after this feature.

### Test Database Strategy

Persistence tests should ideally use a real PostgreSQL database.

Possible approaches:

- Testcontainers PostgreSQL
- Docker Compose test database
- Local development database

The selected approach should be simple and reliable.

### Migration Management

Migrations are part of the Infrastructure history.

They should be treated as source-controlled artifacts.

Once committed, migrations should normally not be edited retroactively. Instead, future schema changes should be represented through new migrations.

## Done Definition

This feature is done when:

- PostgreSQL persistence is implemented.
- EF Core mappings exist for the roster foundation model.
- Initial migration exists.
- Migration files are committed.
- EF migration model snapshot is committed.
- EF-backed `IGuildRepository` and `IPlayerRepository` implementations exist.
- Application use cases work with persistent repositories.
- The minimal verification UI can persist and reload data.
- Data survives application restart.
- Domain remains EF-free.
- Application remains EF-free.
- Architecture tests pass.
- Persistence tests pass.
- `dotnet build Guildwise.sln` succeeds.
- `dotnet test Guildwise.sln` succeeds.
- `CHANGELOG.md` has an entry under `[Unreleased]`.
