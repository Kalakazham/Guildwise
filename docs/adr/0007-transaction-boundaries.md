# ADR 0007: Transaction Boundaries

## Status

Accepted

## Context

Some Guildwise persistence flows span multiple saves or multiple aggregate roots.

Without explicit transaction boundaries, technical failures can leave partial database state.

Known cases include:

- `DeletePlayerHandler`, which removes guild and raid team memberships before removing the player.
- `EfPlayerRepository.AddAsync`, which uses a two-save workaround for the `Player.MainCharacterId` / `Character.PlayerId` insert cycle.

Expected `Result` failures are not transaction failures. For example, a missing player should return `Result.NotFound` before transactional persistence work begins.

Technical exceptions during persistence should roll back.

## Decision

Guildwise will use a minimal Application transaction abstraction:

- `ITransactionRunner`

Application may depend on this abstraction, but Application must not reference EF Core transaction APIs.

Infrastructure implements transaction behavior:

- `EfTransactionRunner`
- `InMemoryTransactionRunner`

Application handlers that coordinate multi-aggregate persistence should use `ITransactionRunner`.

Infrastructure-only multi-save workarounds may use EF transactions internally inside repository implementations.

Guildwise will not introduce a large UnitOfWork redesign at this time.

## Rules

- Application may depend on `ITransactionRunner`.
- Application must not reference EF Core transaction APIs.
- Domain must not know about transactions.
- Web must not know about transaction implementations.
- Infrastructure owns transaction implementation details.
- Use `ITransactionRunner` for Application-level multi-aggregate persistence flows.
- Use repository-internal EF transactions for Infrastructure-only multi-save operations.
- Do not start transactions for expected pre-check failures such as `Result.NotFound`.
- Technical exceptions inside a transaction should roll back and rethrow.
- Do not convert technical transaction failures into `Result` failures.
- Do not introduce a broad UnitOfWork abstraction unless a future feature explicitly requires it.

## Consequences

### Positive

- Critical persistence flows are atomic.
- Application remains EF-free.
- Infrastructure owns EF transaction behavior.
- The design improves reliability without a large UnitOfWork redesign.

### Negative

- The Application layer has another persistence abstraction.
- Handlers must carefully distinguish expected failures from technical exceptions.
- The in-memory transaction runner is a no-op and does not simulate rollback.

