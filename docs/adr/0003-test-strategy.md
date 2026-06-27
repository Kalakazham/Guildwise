# ADR 0003: Test Strategy

## Status

Accepted

## Context

Guildwise will be developed with AI assistance. AI agents can generate useful code quickly, but they can also introduce regressions, architectural violations or overly broad changes.

The project needs a test strategy that provides confidence without slowing down development too much.

The application will contain multiple kinds of logic:

* Domain rules
* Application use cases
* Web endpoints and UI flows
* Persistence
* External integrations
* Architecture boundaries

These concerns should not all be tested in the same way.

## Decision

Guildwise will use three main test projects:

```text
tests/
  Guildwise.UnitTests
  Guildwise.IntegrationTests
  Guildwise.ArchitectureTests
```

Each test project has a distinct purpose.

## Unit Tests

Unit tests verify isolated Domain and Application behavior.

They should be fast, deterministic and independent from infrastructure.

Unit tests must not depend on:

* Real databases
* Web servers
* Network calls
* External APIs
* File system state
* Background jobs

Examples:

* A guild can create a raid team.
* A guild rejects duplicate raid team names.
* A character can be created with a valid name, region and realm.
* A raid team can add a roster member.
* A raid team rejects duplicate roster members.
* Attendance rate calculation works correctly once attendance exists.

Unit tests should primarily cover:

* Domain entities
* Value objects
* Domain services
* Application use cases
* Validation rules
* Important calculations

## Integration Tests

Integration tests verify that multiple parts of the application work together.

They may use:

* ASP.NET Core test host
* WebApplicationFactory
* Real dependency injection
* Test database
* Persistence layer
* HTTP endpoints

Examples:

* `POST /guilds` creates a guild.
* `GET /guilds/{id}/roster` returns a roster.
* Invalid input returns `400 Bad Request`.
* Missing guild returns `404 Not Found`.
* Persistence saves and reloads guild data correctly.

Integration tests should be used for important application flows, not for every small domain rule.

When PostgreSQL persistence is introduced, integration tests should eventually use Testcontainers instead of relying only on in-memory substitutes.

## Architecture Tests

Architecture tests enforce layer boundaries.

They are especially important because AI agents may accidentally create forbidden dependencies while implementing features.

Examples:

* Domain must not depend on Application, Infrastructure or Web.
* Application must not depend on Infrastructure or Web.
* Domain must not use EF Core or ASP.NET Core types.
* Application must not use Infrastructure implementation types.
* Blazor components must not directly use DbContext.
* Blazor components must not directly use repository implementations.

Architecture tests should fail when the intended modular monolith structure is violated.

## Test Project Responsibilities

### Guildwise.UnitTests

References:

```text
Guildwise.Domain
Guildwise.Application
```

Purpose:

* Domain behavior
* Application behavior
* Validation
* Calculations
* Use cases without real infrastructure

### Guildwise.IntegrationTests

References:

```text
Guildwise.Web
Guildwise.Application
Guildwise.Infrastructure
```

Purpose:

* Endpoint tests
* Application wiring
* Persistence tests
* Dependency injection tests
* Important user flows

### Guildwise.ArchitectureTests

References:

```text
Guildwise.Domain
Guildwise.Application
Guildwise.Infrastructure
Guildwise.Web
```

Purpose:

* Enforce project dependency rules
* Enforce namespace rules
* Prevent architecture drift
* Protect the codebase from accidental AI-generated boundary violations

## Code Coverage

Code coverage is useful as a signal, not as a goal by itself.

Initial target ranges:

| Area           |                                                 Target |
| -------------- | -----------------------------------------------------: |
| Domain         |                                                 80-90% |
| Application    |                                                 70-85% |
| Infrastructure |                                     no hard percentage |
| Web            | covered mainly through integration and later E2E tests |
| Overall        |                                   60-75% is acceptable |

100% coverage is not a goal.

Good tests for important behavior are more valuable than high coverage from shallow tests.

## Testing Rules

* Add or update tests for meaningful domain or application behavior.
* Do not test trivial properties without behavior.
* Prefer testing observable behavior over implementation details.
* Avoid excessive mocking.
* Unit tests should be fast.
* Integration tests may be slower but should cover important flows.
* Tests should use clear domain language.
* AI-generated code should not be accepted before `dotnet build` and `dotnet test` pass.

## AI Development Rules

When an AI agent changes code, it should:

1. Read `AGENTS.md`.
2. Read the relevant feature file in `docs/features`.
3. Check existing ADRs in `docs/adr`.
4. Add or update tests for the changed behavior.
5. Run `dotnet build`.
6. Run `dotnet test`.
7. Summarize changed files and important decisions.

If tests cannot be run, the agent must explicitly state why.

## Consequences

### Positive

* Fast feedback for domain logic.
* Confidence in important application flows.
* Protection against architecture drift.
* Safer AI-assisted development.
* Clear separation between unit, integration and architecture tests.

### Negative

* More initial setup.
* Tests need maintenance.
* Some architecture rules may need refinement as the project evolves.
* Integration tests may require additional tooling once persistence is added.

## Follow-Up Actions

* Add `Guildwise.ArchitectureTests`.
* Add an architecture testing library such as NetArchTest.
* Add architecture tests for forbidden dependencies.
* Add integration test setup once endpoints or persistence exist.
* Add CI once the first useful feature exists.
* Run `dotnet build` and `dotnet test` after every AI-generated change.
