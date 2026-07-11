# Guildwise Architecture

Guildwise is a modular monolith for guild and raid organization.

The application starts as a single deployable web application, but the codebase is separated into clear modules and layers. The goal is to keep development fast while preserving architectural boundaries.

## High-Level Goal

Guildwise should help raid leads and guild organizers answer practical questions:

* Who is in the roster?
* Which roles, classes and specializations are available?
* Who belongs to which raid team?
* Who attends raids regularly?
* Which characters need preparation?
* What should the raid lead know before the next raid?

The first version focuses on manual roster management.

External data sources such as Raider.IO, Blizzard APIs and Warcraft Logs will be added later.

## Solution Structure

```text
src/
  Guildwise.Domain
  Guildwise.Application
  Guildwise.Infrastructure
  Guildwise.Web

tests/
  Guildwise.UnitTests
  Guildwise.IntegrationTests
  Guildwise.ArchitectureTests
```

## Layer Responsibilities

### Domain

The Domain layer contains the business model and core rules.

Examples:

* Guild
* Player
* Character
* GuildMember
* RaidTeam
* RaidTeamMember
* RaidEvent
* AttendanceRecord
* LootWishlistItem

The Domain layer must be independent.

It must not know about:

* databases
* EF Core
* web frameworks
* external APIs
* UI concerns
* infrastructure concerns

Domain entities should protect business invariants through explicit methods and should not expose publicly mutable collections.

### Application

The Application layer contains use cases and application-level orchestration.

Examples:

* CreateGuild
* CreatePlayer
* CreateCharacter
* SetMainCharacter
* CreateRaidTeam
* AddPlayerToRaidTeam
* GetRosterDashboard
* CalculateAttendance
* BuildWeeklySummary

The Application layer defines interfaces for things it needs from the outside world, such as repositories or external profile providers.

Application code orchestrates domain behavior. It should not bypass domain rules.

Application must not reference Infrastructure, Web, EF Core or database-specific types.

### Infrastructure

The Infrastructure layer contains technical implementations.

Examples:

* EF Core DbContext
* PostgreSQL persistence
* Repository implementations
* Database migrations
* Migration history
* Caching
* Background jobs
* Raider.IO client
* Blizzard API client
* Warcraft Logs client

Infrastructure implements interfaces defined in the Application layer.

Infrastructure owns all persistence-specific details.

This includes:

* `GuildwiseDbContext`
* EF Core entity configurations
* EF-backed repository implementations
* EF Core migrations
* PostgreSQL registration through dependency injection
* temporary in-memory repository implementations

Domain and Application must remain persistence-ignorant.

### Web

The Web layer contains the Blazor application, routing, pages, components and dependency injection setup.

The Web layer is the composition root. It wires Application and Infrastructure together.

Blazor components should not contain business logic.

Blazor components should call Application use cases or handlers, not repositories or DbContexts directly.

## Dependency Rules

Allowed dependencies:

```text
Guildwise.Domain
  -> no Guildwise project references

Guildwise.Application
  -> Guildwise.Domain

Guildwise.Infrastructure
  -> Guildwise.Application
  -> Guildwise.Domain

Guildwise.Web
  -> Guildwise.Application
  -> Guildwise.Infrastructure
```

Test dependencies:

```text
Guildwise.UnitTests
  -> Guildwise.Domain
  -> Guildwise.Application

Guildwise.IntegrationTests
  -> Guildwise.Web
  -> Guildwise.Application
  -> Guildwise.Infrastructure

Guildwise.ArchitectureTests
  -> all production projects
```

## Important Boundary Rules

* Domain must not reference Application, Infrastructure or Web.
* Application must not reference Infrastructure or Web.
* Infrastructure must not be used directly from Blazor components.
* Web may reference Infrastructure only for dependency injection and composition.
* External DTOs must be mapped before entering the Domain.
* Domain entities must not be shaped around external API responses.
* Business rules belong in Domain or Application, not in Web.
* Repository interfaces belong in Application.
* Repository implementations belong in Infrastructure.

## Frontend UI Boundaries

Guildwise Web currently uses custom Blazor components and custom CSS for the roster-focused UI.

Admin dashboard patterns such as sidebar navigation, panels, cards, tables, forms and widgets may guide the Web layout, but AdminLTE is only a structural and visual reference, not a direct dependency.

UI dependencies, display labels, badge styling and WoW class colors belong in `Guildwise.Web`. Domain, Application and Infrastructure must stay free of CSS, color, Blazor, icon and component concepts.

The frontend UI stack decision is documented in:

```text
docs/adr/0008-frontend-ui-stack.md
```

## Application Result Handling

Command handlers return `Result` or `Result<T>` for expected use-case outcomes. Expected failures use `Failure` and `FailureType` values such as `NotFound`, `Validation`, `Conflict` and `BusinessRule`.

Query handlers may return nullable DTOs for missing single entities and empty collections for lists.

Technical failures may still throw exceptions. Domain does not reference Application result types. Web consumes Application results and displays expected failures to users.

## Persistence

Guildwise uses PostgreSQL with EF Core for persistent storage.

Persistence belongs to `Guildwise.Infrastructure`.

The persistence strategy is documented in:

```text
docs/adr/0005-persistence-strategy.md
```

### Persistence Rules

* Domain must not reference EF Core.
* Domain must not contain EF Core attributes.
* Domain must not expose public setters only for persistence.
* Application must not reference EF Core.
* Infrastructure owns EF Core.
* Repository interfaces remain in Application.
* Repository implementations live in Infrastructure.
* Web configures persistence through dependency injection only.
* Blazor components must not use `DbContext` directly.
* Blazor components must not use repository implementations directly.
* Domain entities must be mapped through EF Core Fluent API in Infrastructure.

### Aggregate Root Repositories

Guildwise uses aggregate-root repositories only.

Allowed repository interfaces:

```text
IGuildRepository
IPlayerRepository
```

Do not create repositories for:

```text
Character
RaidTeam
GuildMember
RaidTeamMember
```

Reason:

* Characters are managed through `Player`.
* RaidTeams and GuildMembers are managed through `Guild`.
* Creating repositories for child entities would weaken aggregate boundaries and make it easier to bypass domain rules.

### Migrations

EF Core migrations belong to Infrastructure.

Suggested location:

```text
src/Guildwise.Infrastructure/Persistence/Migrations/
```

Migration files and the EF Core model snapshot must be committed to Git.

EF Core also maintains a database-side migration history table.

Default table:

```text
__EFMigrationsHistory
```

Rules:

* Add a new migration whenever the persisted model changes.
* Do not manually edit the EF migration history table.
* Do not delete or rewrite committed migrations unless explicitly instructed.
* Do not generate migrations into Domain, Application or Web.

### Transaction Boundaries

`ITransactionRunner` is an Application abstraction for Application-level multi-aggregate persistence flows.

`EfTransactionRunner` and `InMemoryTransactionRunner` live in Infrastructure. EF transaction APIs must remain in Infrastructure.

Repository-internal two-save EF workarounds may use EF transactions internally. Expected `Result` failures should generally be returned before transactional persistence work begins.

## First Vertical Slice

The first feature is manual guild roster foundation.

It includes:

* Domain model for guilds, players, characters, guild members and raid teams.
* Application use case handlers.
* Temporary in-memory infrastructure.
* A minimal Blazor verification UI.

This is not the final production roster dashboard.

Flow:

1. User creates a guild.
2. User creates a player.
3. User creates a character for the player.
4. User selects a main character.
5. User creates a raid team.
6. User adds the player to the guild.
7. User adds the player to the raid team.
8. Guildwise displays the resulting roster setup.

No external APIs are involved in this first slice.

## Future Integrations

External integrations should be added behind Application interfaces.

Potential providers:

* Raider.IO
* Blizzard WoW API
* Warcraft Logs
* Discord

External integrations should live in Infrastructure initially. If an integration becomes large enough, it can be extracted into its own project later.

Potential future projects:

```text
Guildwise.Integrations.RaiderIo
Guildwise.Integrations.Blizzard
Guildwise.Integrations.WarcraftLogs
```

## Testing Strategy

Guildwise uses these main test types:

* Unit tests for Domain and Application behavior.
* Integration tests for endpoints, persistence and application wiring.
* Architecture tests for enforcing layer boundaries.

### Unit Tests

Unit tests cover isolated Domain and Application behavior.

They should not require:

* real databases
* web hosts
* external APIs
* file system state
* network access

### Integration Tests

Integration tests cover multiple layers working together.

They may cover:

* Web endpoint or page wiring
* Application handler wiring
* Infrastructure dependency injection
* repository behavior
* persistence behavior

Persistence integration tests should eventually run against PostgreSQL.

Acceptable approaches:

* Testcontainers PostgreSQL
* Docker Compose test database
* local development database for early manual verification

### Architecture Tests

Architecture tests protect the intended boundaries.

They should enforce that:

* Domain does not depend on Application, Infrastructure or Web.
* Application does not depend on Infrastructure or Web.
* Domain does not use EF Core or ASP.NET Core types.
* Application does not use EF Core.
* Blazor components do not directly use repositories or DbContexts.

The goal is not 100% coverage.

The goal is confidence in important behavior and protection against architectural drift, especially during AI-assisted development.

## Architecture Philosophy

Guildwise follows pragmatic Clean Architecture.

This means:

* The inner layers stay independent.
* The web app remains simple to deploy.
* Infrastructure is kept outside business logic.
* The architecture should help development, not slow it down.
* Persistence is implemented in Infrastructure without leaking into Domain or Application.
* External integrations are introduced behind Application interfaces.
