# Guildwise Architecture

Guildwise is a modular monolith for guild and raid organization.

The application starts as a single deployable web application, but the codebase is separated into clear modules and layers. The goal is to keep development fast while preserving architectural boundaries.

## High-Level Goal

Guildwise should help raid leads and guild organizers answer practical questions:

- Who is in the roster?
- Which roles, classes and specializations are available?
- Who belongs to which raid team?
- Who attends raids regularly?
- Which characters need preparation?
- What should the raid lead know before the next raid?

The first version focuses on manual roster management. External data sources such as Raider.IO, Blizzard APIs and Warcraft Logs will be added later.

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

Guild
RaidTeam
Character
RosterMember
RaidEvent
AttendanceRecord
LootWishlistItem

The Domain layer must be independent. It must not know about databases, web frameworks, external APIs or UI concerns.

### Application

The Application layer contains use cases and application-level orchestration.

Examples:

CreateGuild
CreateRaidTeam
AddCharacterToRoster
GetRosterDashboard
CalculateAttendance
BuildWeeklySummary

The Application layer defines interfaces for things it needs from the outside world, such as repositories or external profile providers.

### Infrastructure

The Infrastructure layer contains technical implementations.

Examples:

EF Core DbContext
Repository implementations
Database migrations
Caching
Background jobs
Raider.IO client
Blizzard API client
Warcraft Logs client

Infrastructure implements interfaces defined in the Application layer.

### Web

The Web layer contains the Blazor application, routing, pages, components and dependency injection setup.

The Web layer is the composition root. It wires Application and Infrastructure together.

Blazor components should not contain business logic. They should call Application use cases or services.

## Dependency Rules

Allowed dependencies:

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

Test dependencies:

Guildwise.UnitTests
  -> Guildwise.Domain
  -> Guildwise.Application

Guildwise.IntegrationTests
  -> Guildwise.Web
  -> Guildwise.Application
  -> Guildwise.Infrastructure

Guildwise.ArchitectureTests
  -> all production projects

## Important Boundary Rules

- Domain must not reference Application, Infrastructure or Web.
- Application must not reference Infrastructure or Web.
- Infrastructure must not be used directly from Blazor components.
- Web may reference Infrastructure only for dependency injection and composition.
- External DTOs must be mapped before entering the Domain.
- Domain entities must not be shaped around external API responses.
- Business rules belong in Domain or Application, not in Web.

## First Vertical Slice

The first feature is manual roster management.

Flow:

User creates a guild.
User creates a raid team.
User adds a character manually.
User assigns role, class and specialization.
Guildwise displays a roster dashboard.

No external APIs are involved in this first slice.

## Future Integrations

External integrations should be added behind Application interfaces.

Potential providers:

Raider.IO
Blizzard WoW API
Warcraft Logs
Discord

External integrations should live in Infrastructure initially. If an integration becomes large enough, it can be extracted into its own project later.

Potential future projects:

Guildwise.Integrations.RaiderIo
Guildwise.Integrations.Blizzard
Guildwise.Integrations.WarcraftLogs

## Testing Strategy

Guildwise uses three main test types:

Unit tests for Domain and Application behavior.
Integration tests for endpoints, persistence and application wiring.
Architecture tests for enforcing layer boundaries.

The goal is not 100% coverage. The goal is confidence in important behavior and protection against architectural drift.

## Architecture Philosophy

Guildwise follows pragmatic Clean Architecture.

This means:

The inner layers stay independent.
The web app remains simple to deploy.
Infrastructure is kept outside business logic.
The architecture should help development, not slow it down.
