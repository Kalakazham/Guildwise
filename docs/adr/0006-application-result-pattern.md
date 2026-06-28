# ADR 0006: Application Result Pattern

## Status

Accepted

## Context

Guildwise distinguishes expected use-case outcomes from real technical failures.

Expected outcomes include:

- not found
- validation failure
- conflict
- business rule violation

These outcomes are part of normal command contracts. They are not technical errors and should not normally be represented by exceptions.

Command handlers now return `Result` or `Result<T>` for expected success and failure outcomes. Query handlers may still return `null` for missing single entities and empty collections for list results.

Domain entities may still throw exceptions to protect invariants. Technical failures may also still throw exceptions, including persistence failures, invalid runtime configuration and programming defects.

## Decision

Guildwise will use minimal Application result primitives:

- `Result`
- `Result<T>`
- `Failure`
- `FailureType`

The result model uses the term `Failure`, not `Error`, because expected use-case outcomes are not technical errors.

Result primitives live in the Application layer. Domain must not reference Application result types. Infrastructure must not define use-case result semantics.

Command handlers return `Result` or `Result<T>`. Query handlers stay nullable or collection-based unless there is a strong reason to model a query as a use-case result.

Expected command outcomes return structured failures. Technical failures still throw.

Web may consume Application results and display expected failures to users.

## Rules

- Do not use exceptions as normal control flow for expected command outcomes.
- Missing entities in command handlers should usually return `FailureType.NotFound`.
- Invalid command input should usually return `FailureType.Validation`.
- Duplicate or already-existing state should usually return `FailureType.Conflict`.
- Domain or use-case rule violations should usually return `FailureType.BusinessRule`.
- Do not catch broad exceptions and convert them into failures.
- Let persistence, runtime, configuration and programming failures throw.
- Keep `Result`, `Result<T>`, `Failure` and `FailureType` out of Domain.
- Keep Infrastructure from defining Application use-case result semantics.
- Keep query handlers nullable or collection-based unless a stronger use-case contract is needed.

## Consequences

### Positive

- Use-case contracts are clearer.
- Web UI can display expected failures without treating them as technical exceptions.
- Expected failure paths are easier to test.
- Command flows use fewer expected exceptions.

### Negative

- Command handlers have slightly more explicit branching.
- Caller code must check `Result` before using returned values.
- Some Domain exceptions may still need case-by-case handling in Application handlers.

