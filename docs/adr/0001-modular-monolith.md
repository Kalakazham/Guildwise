# ADR 0001: Use a Modular Monolith

## Status

Accepted

## Context

Guildwise is a side project with limited weekly development time. The product idea is still evolving. The application needs to support a web UI, persistence, external API integrations and background jobs in the future.

Starting with microservices would introduce significant complexity too early:

- Multiple deployments
- Network communication
- Distributed tracing
- Service-to-service authentication
- More complicated local development
- Harder testing
- More infrastructure work before product value exists

At the same time, the codebase should not become an unstructured monolith. Guildwise needs clear boundaries between domain logic, application use cases, infrastructure and web UI.

## Decision

Guildwise will start as a modular monolith.

There will be one deployable web application, but the codebase will be separated into clear projects and layers:

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

The application will be deployed as one unit at first.

External integrations such as Raider.IO, Blizzard APIs and Warcraft Logs will initially live in Infrastructure. If they become large enough, they may be extracted into separate integration projects later.

Potential future projects:

```text
src/
  Guildwise.Integrations.RaiderIo
  Guildwise.Integrations.Blizzard
  Guildwise.Integrations.WarcraftLogs
```

These would still be code modules, not separate deployed services unless there is a clear operational reason.

## Consequences

### Positive

- Faster development.
- Easier local setup.
- Simpler deployment.
- Clear project boundaries.
- Easier refactoring than a distributed system.
- Good fit for a solo or small-team side project.
- Easier for AI agents to understand and modify the codebase safely.

### Negative

- All modules are deployed together.
- Runtime isolation between modules is limited.
- Bad dependencies must be prevented through discipline and architecture tests.
- Scaling individual parts independently is not possible at first.

## Rules

- Do not introduce microservices without a concrete reason.
- Do not split external integrations into separate deployed services early.
- Keep modules separated through project structure, namespaces and architecture tests.
- Prefer vertical feature slices over large technical rewrites.
- Reconsider service extraction only when there is a real operational, scaling or team-ownership reason.

## Follow-Up Actions

- Add architecture tests to enforce project dependency rules.
- Keep `Guildwise.Domain` independent.
- Keep `Guildwise.Application` independent from Infrastructure and Web.
- Prevent Blazor components from using Infrastructure directly.
- Document future architecture changes as new ADRs.