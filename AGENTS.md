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

## Testing Rules

- Add or update tests for meaningful domain or application behavior.
- Unit tests should be fast and isolated.
- Integration tests should verify that important flows work through real application wiring.
- Architecture tests should enforce layer boundaries.
- After changes, run:

```bash
dotnet build
dotnet test
```
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