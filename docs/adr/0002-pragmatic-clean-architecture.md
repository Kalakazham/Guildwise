# ADR 0002: Use Pragmatic Clean Architecture

## Status

Accepted

## Context

Guildwise needs clear boundaries between business logic, application use cases, infrastructure and web UI.

A strict Clean Architecture implementation would usually avoid a direct project reference from the Web project to the Infrastructure project. This often requires an additional composition or bootstrap project.

For Guildwise, that would add ceremony early in the project without much practical benefit.

At the same time, the project must prevent business logic from leaking into the web UI or infrastructure concerns from leaking into the domain model.

## Decision

Guildwise will use a pragmatic Clean Architecture style.

The allowed production project dependencies are:

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

The Web project may reference Infrastructure because it acts as the composition root.

This means `Guildwise.Web` may call infrastructure registration methods such as:

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

However, Blazor pages and components must not directly depend on Infrastructure classes, repositories, DbContexts, external API clients or other technical implementation details.

## Layer Responsibilities

### Domain

The Domain layer contains the core business model and business rules.

It must not depend on:

- EF Core
- ASP.NET Core
- Blazor
- HTTP clients
- JSON serialization attributes
- External API DTOs
- Infrastructure services

### Application

The Application layer contains use cases, application services, DTOs, commands, queries and interfaces.

It defines what the application needs from the outside world, but it does not implement technical details.

Examples:

- Repository interfaces
- External profile provider interfaces
- Use case handlers
- Application DTOs
- Validation logic that belongs to a use case

### Infrastructure

The Infrastructure layer implements technical details.

Examples:

- EF Core DbContext
- Repository implementations
- Database migrations
- Caching
- Background jobs
- External API clients
- File storage
- Email or Discord integrations

Infrastructure implements interfaces defined by the Application layer.

### Web

The Web layer contains the Blazor UI, routing, web-specific models and dependency injection setup.

The Web layer should call Application use cases or services.

The Web layer should not contain business rules.

## Rules

- Domain must not reference Application, Infrastructure or Web.
- Application must not reference Infrastructure or Web.
- Infrastructure may reference Application and Domain.
- Web may reference Application and Infrastructure.
- Web may use Infrastructure only for dependency injection and composition.
- Blazor components must not inject DbContexts, repositories or external API clients directly.
- External API DTOs must be mapped before entering the Domain.
- EF Core attributes must not be placed on Domain entities.
- Business rules belong in Domain or Application, not in Web.

## Consequences

### Positive

- Simple ASP.NET Core setup.
- Clear enough layering for a side project.
- No unnecessary bootstrap project.
- Easy dependency injection in `Program.cs`.
- Infrastructure remains outside business logic.
- Domain remains independent.
- Architecture can still be enforced through tests.

### Negative

- Web has a project reference to Infrastructure.
- Developers and AI agents must respect that Infrastructure is only used for composition.
- Architecture tests are needed to prevent misuse.
- The boundary between Web and Infrastructure depends partly on discipline.

## Architecture Test Expectations

Architecture tests should enforce at least these rules:

- Domain does not depend on Application, Infrastructure or Web.
- Application does not depend on Infrastructure or Web.
- Domain does not use EF Core or ASP.NET Core types.
- Application does not use Infrastructure types.
- Blazor components do not directly use `DbContext`.
- Blazor components do not directly use repository implementations.

## Follow-Up Actions

- Add `Guildwise.ArchitectureTests`.
- Add architecture tests for forbidden dependencies.
- Keep Infrastructure usage in Web limited to `Program.cs` and dependency injection extension methods.
- Consider introducing a Bootstrapper project only if Web starts depending on Infrastructure outside composition.