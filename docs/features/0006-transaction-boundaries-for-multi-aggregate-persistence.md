# Feature 0006: Transaction Boundaries for Multi-Aggregate Persistence

## Tracking

GitHub Issue: #6
Branch: `feature/0006-transaction-boundaries`
Milestone: `v0.3.0`

## Goal

Add minimal, explicit transaction boundaries for critical multi-step persistence flows.

Guildwise should improve atomicity for risky persistence operations without introducing a large UnitOfWork redesign. The Domain and Application layers must remain EF-free, and Infrastructure must continue to own EF Core and PostgreSQL details.

This feature should preserve existing command result behavior, repository contracts, domain behavior and user-facing behavior.

## Problem

Some current persistence flows can save part of a multi-step operation before a later technical failure occurs.

### DeletePlayerHandler

`DeletePlayerHandler` currently:

1. Loads the player.
2. Lists guilds.
3. Removes guild memberships and raid team memberships from guild aggregates.
4. Saves guild aggregate changes.
5. Removes the player.

If a technical failure occurs after guild changes are saved but before the player is removed, the database can be left in a partial state: guild and raid team memberships are gone, but the player still exists.

### EfPlayerRepository.AddAsync

`EfPlayerRepository.AddAsync` currently uses a two-save workaround for the `Player.MainCharacterId` / `Character.PlayerId` insert cycle:

1. Add the player and characters with `MainCharacterId` temporarily cleared.
2. Save.
3. Restore `MainCharacterId`.
4. Save again.

If the second save fails after the first save succeeds, the database can be left with a persisted player and characters but without the intended main-character assignment.

## User Value

Users should not see partial persistence after technical failures. For example, deleting a player should not remove guild and raid team memberships unless the player removal also succeeds.

There is no intended visible UI change for successful commands or expected failures. The value is safer persistence behavior.

## Technical Value

This feature improves:

* Atomicity for known multi-step persistence flows.
* Confidence in EF-backed persistence behavior.
* Separation between expected use-case outcomes and technical failures.
* Long-term reliability before additional roster features add more multi-aggregate operations.

## In Scope

This feature includes:

* Add a minimal Application transaction abstraction.
* Add an EF Core implementation in Infrastructure.
* Add an in-memory no-op implementation in Infrastructure.
* Register the transaction abstraction through Infrastructure dependency injection.
* Wrap the risky multi-aggregate delete-player flow.
* Make the repository-internal two-save player/main-character insert workaround transactional.
* Add focused rollback tests.
* Update `CHANGELOG.md`.

## Out of Scope

This feature does not include:

* Large UnitOfWork redesign.
* Repository redesign.
* Domain behavior changes.
* EF mapping changes.
* New EF migration.
* Result pattern changes.
* Query handler changes.
* Web UI changes.
* External packages unless strongly justified.
* Production deployment behavior.
* Global exception handling.

## Architecture Rules

The existing architecture rules remain in force.

* Domain must not reference transactions.
* Domain must not reference EF Core.
* Application must not reference EF Core.
* Application may define a transaction abstraction.
* Infrastructure implements transaction behavior.
* Infrastructure owns EF Core transaction APIs.
* Web does not know about transactions.
* Web must not inject repositories or `DbContext` directly.
* Expected `Result` failures are not transaction failures.
* Technical exceptions inside a transaction should cause rollback.
* Technical exceptions should not be broadly converted into `Result` failures.

## Transaction Abstraction

Add a minimal transaction runner interface to Application.

Suggested location:

```text
src/Guildwise.Application/Abstractions/Persistence/ITransactionRunner.cs
```

Preferred shape:

```csharp
public interface ITransactionRunner
{
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);

    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}
```

The abstraction should stay small. It should not expose EF Core, database transactions, isolation levels, HTTP concepts or persistence provider details.

## Infrastructure Implementations

### EF Core

Add an EF implementation in Infrastructure.

Suggested location:

```text
src/Guildwise.Infrastructure/Persistence/EfTransactionRunner.cs
```

The EF implementation should:

* Inject `GuildwiseDbContext`.
* Use `GuildwiseDbContext.Database.BeginTransactionAsync`.
* Commit when the operation completes successfully.
* Roll back and rethrow when the operation throws.
* Pass cancellation tokens through.
* Avoid nested transactions if possible by checking `Database.CurrentTransaction`.
* Keep implementation simple.

If an EF transaction is already active, the runner may execute the operation directly and let the outer transaction own commit or rollback.

Using EF Core execution strategy may be considered, but do not overbuild the first implementation.

### In-Memory

Add a no-op in-memory implementation in Infrastructure.

Suggested location:

```text
src/Guildwise.Infrastructure/Persistence/InMemoryTransactionRunner.cs
```

The in-memory implementation should:

* Execute the operation directly.
* Pass the cancellation token through.
* Not simulate rollback.
* Remain simple.

## Dependency Injection

Register the transaction abstraction through Infrastructure.

`AddInMemoryInfrastructure` should register:

```text
ITransactionRunner -> InMemoryTransactionRunner
```

`AddPostgresInfrastructure` should register:

```text
ITransactionRunner -> EfTransactionRunner
```

Suggested lifetimes:

* In-memory transaction runner: singleton, matching current in-memory repositories.
* EF transaction runner: scoped, matching `GuildwiseDbContext` and EF repositories.

Application handler registration should remain in `Guildwise.Application`.

## Risk-Sensitive Flows

### 1. DeletePlayerHandler

Expected behavior:

* Missing player still returns `Result.NotFound`.
* No transaction is needed for expected failure before persistence.
* The mutation/persistence section runs transactionally.
* Technical exception during persistence rolls back guild changes and player removal.
* Command result semantics remain unchanged.

Suggested flow:

```text
load player
if missing:
  return Result.NotFound

transaction:
  list guilds
  remove player from guild memberships and raid teams
  save guild aggregate changes
  remove player

return Result.Success
```

The handler should inject `ITransactionRunner` in addition to the existing repositories.

Expected `Result.NotFound` should remain outside the transaction. Technical exceptions inside the transaction should propagate.

### 2. EfPlayerRepository.AddAsync

Expected behavior:

* Existing two-save main-character workaround remains.
* Both save steps happen in one transaction.
* If the second save fails, the first save is rolled back.
* No caller-facing behavior changes.
* No repository interface changes are required.

For players without `MainCharacterId`, the existing single-save path can remain simple.

For players with `MainCharacterId`, wrap the two-save path in an EF transaction. If a transaction already exists, avoid starting a nested transaction and let the outer transaction own commit or rollback.

## Expected Results and Exceptions

Expected use-case outcomes should continue to return `Result` or `Result<T>` failures.

Examples:

* Missing player in `DeletePlayerHandler` returns `Result.NotFound`.
* Validation failures continue to return `Result.Validation`.
* Conflict and business-rule outcomes continue to return structured failures.

Technical failures should continue to throw.

Examples:

* EF persistence failure.
* PostgreSQL unavailable.
* Invalid runtime configuration.
* Programming defects.

Transaction rollback is for technical exceptions during persistence, not for normal expected result failures.

## Testing Requirements

Prefer unit tests for Application behavior and integration tests for EF rollback behavior.

### Unit Tests

Update unit tests as needed for constructor changes.

`DeletePlayerHandler` unit tests should use a no-op or fake `ITransactionRunner`.

Existing unit tests should continue to verify:

* `DeletePlayerHandler` removes player, guild membership and raid team membership.
* `DeletePlayerHandler` returns `Result.NotFound` for a missing player.
* In-memory infrastructure remains usable for handlers that depend on the transaction abstraction.

### Integration Tests

Add focused Testcontainers PostgreSQL tests for EF transaction behavior.

Required integration tests:

* `EfTransactionRunner` rolls back changes if an operation saves data and then throws.
* `DeletePlayerHandler` rolls back guild membership and raid team membership changes if player removal throws.
* `EfPlayerRepository.AddAsync` does not leave partial player/character rows if the second save fails.
* `EfPlayerRepository.AddAsync` still persists a player with a main character successfully.

Existing success-path tests must remain green.

### Dependency Injection Tests

Update DI tests to verify:

* In-memory infrastructure registers `ITransactionRunner` to `InMemoryTransactionRunner`.
* PostgreSQL infrastructure registers `ITransactionRunner` to `EfTransactionRunner`.
* Web persistence configuration still selects the expected infrastructure mode without Web knowing about transaction details directly.

## Rollback Test Strategy

### EfTransactionRunner rollback

Use the existing PostgreSQL test fixture.

1. Create a DbContext.
2. Run an operation through `EfTransactionRunner`.
3. Add a guild or player.
4. Save changes.
5. Throw a test exception.
6. Assert the exception is propagated.
7. Open a new DbContext.
8. Assert the saved data is not present.

### DeletePlayerHandler rollback

Use real EF-backed guild persistence and a failing player repository wrapper.

1. Persist a player with a main character.
2. Persist a guild with guild membership, raid team and raid team membership.
3. Construct `DeletePlayerHandler` with:
   * `EfGuildRepository`
   * a wrapper `IPlayerRepository` that delegates reads and throws from `RemoveAsync`
   * `EfTransactionRunner`
4. Execute the handler.
5. Assert the technical exception is propagated.
6. Open a new DbContext.
7. Assert the player still exists.
8. Assert the guild member still exists.
9. Assert the raid team member still exists.

### EfPlayerRepository.AddAsync rollback

Use a SaveChanges failure that happens only on the second save in the two-save path.

Possible approach:

1. Configure a DbContext with an EF Core `SaveChangesInterceptor` in the test.
2. Let the first save succeed.
3. Throw from the second save.
4. Call `EfPlayerRepository.AddAsync` with a player that has a main character.
5. Assert the exception is propagated.
6. Open a new DbContext.
7. Assert the player and character rows were not partially persisted.

Keep test-only failure injection in the test project. Do not add production test hooks.

## Documentation Requirements

Update `CHANGELOG.md` under `[Unreleased]` when implementing the feature.

Suggested entry:

```text
Added transaction boundaries for critical multi-step persistence flows.
```

No README update is required unless implementation introduces a meaningful developer-facing behavior change.

## Suggested Implementation Slices

### 0006a: Transaction Abstraction and DI

Implement:

* Add `ITransactionRunner`.
* Add `EfTransactionRunner`.
* Add `InMemoryTransactionRunner`.
* Register implementations in Infrastructure DI.
* Add DI registration tests.
* Add a simple EF rollback integration test for the runner.

Suggested commit:

```text
feat: add persistence transaction runner
```

### 0006b: DeletePlayer Transaction Boundary

Implement:

* Inject `ITransactionRunner` into `DeletePlayerHandler`.
* Keep missing-player `Result.NotFound` outside the transaction.
* Wrap mutation/persistence section in a transaction.
* Update unit test construction.
* Add rollback integration test for player deletion.

Suggested commit:

```text
fix: make player deletion persistence atomic
```

### 0006c: Transactional Main Character Insert Workaround

Implement:

* Wrap `EfPlayerRepository.AddAsync` two-save main-character path in an EF transaction.
* Preserve existing single-save behavior for players without a main character.
* Add rollback integration test for second-save failure.
* Keep existing success-path persistence tests green.

Suggested commit:

```text
fix: make player main-character insert transactional
```

### 0006d: Transaction Coverage Cleanup

Implement:

* Review transaction coverage.
* Add missing focused tests if needed.
* Update `CHANGELOG.md`.
* Run `dotnet build Guildwise.sln`.
* Run `dotnet test Guildwise.sln`.

Suggested commit:

```text
test: cover transaction rollback behavior
```

## Acceptance Criteria

### Abstraction

* `ITransactionRunner` exists in Application.
* `ITransactionRunner` does not reference EF Core or Infrastructure types.
* Domain does not reference transactions.

### Infrastructure

* EF transaction runner exists in Infrastructure.
* In-memory transaction runner exists in Infrastructure.
* EF transaction runner commits on success.
* EF transaction runner rolls back and rethrows on exception.
* EF transaction runner passes cancellation tokens.
* EF transaction runner avoids nested transactions where practical.
* In-memory transaction runner executes directly.

### Dependency Injection

* In-memory infrastructure registers the in-memory transaction runner.
* PostgreSQL infrastructure registers the EF transaction runner.
* Web configuration continues to select infrastructure mode through existing infrastructure registration methods.

### DeletePlayerHandler

* `DeletePlayerHandler` uses the transaction abstraction.
* Missing player still returns `Result.NotFound`.
* Technical exception during delete-player persistence rolls back guild membership changes and player removal.
* Existing successful delete-player behavior remains unchanged.

### EfPlayerRepository.AddAsync

* Two-save main-character insert path is transactional.
* If the second save fails, the first save is rolled back.
* Existing success behavior remains unchanged.
* Repository interface remains unchanged.

### Architecture

* Domain remains EF-free.
* Application remains EF-free.
* Infrastructure remains persistence-focused.
* No EF migration is created.
* Query handlers are unchanged.
* Result primitives are unchanged.
* Web UI is unchanged.

### Tests

* Unit tests pass.
* Integration tests pass.
* Architecture tests pass.
* Testcontainers PostgreSQL continues to be used for EF rollback tests.
* `dotnet build Guildwise.sln` succeeds.
* `dotnet test Guildwise.sln` succeeds.

### Changelog

* `CHANGELOG.md` is updated under `[Unreleased]` when implementation is completed.

## Risks and Open Questions

### EF Execution Strategy

EF Core execution strategies can require the transaction to be created inside the strategy delegate. The first implementation should keep this simple and only add execution-strategy handling if it is necessary for correctness with the configured PostgreSQL provider.

### Existing Save-In-Repository Pattern

Repositories currently save changes internally for `AddAsync` and `RemoveAsync`, while handlers call `SaveChangesAsync` after mutating loaded aggregates. This feature should work with the current pattern and should not redesign repository responsibilities.

### Transaction Scope

Avoid broad transaction use everywhere. The first feature scope is intentionally limited to known risky flows.

### Test Failure Injection

Rollback tests may need small test-only wrappers or EF interceptors to force failures at exact points. Keep these in test projects and avoid production test hooks.

## Done Definition

This feature is done when:

* The transaction abstraction exists in Application.
* Infrastructure provides EF and in-memory implementations.
* Infrastructure DI registers the correct implementation for each persistence mode.
* `DeletePlayerHandler` uses the abstraction for its multi-aggregate mutation/persistence section.
* `EfPlayerRepository.AddAsync` two-save main-character path is transactional.
* Expected `Result` failures remain expected results.
* Technical exceptions inside transactions roll back.
* Domain remains independent.
* Application remains EF-free.
* Infrastructure owns EF transaction behavior.
* No EF migration is created.
* Unit, integration and architecture tests pass.
* `CHANGELOG.md` is updated.
