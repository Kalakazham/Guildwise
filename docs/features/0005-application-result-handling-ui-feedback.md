# Feature 0005: Application Result Handling and UI Feedback

## Tracking

GitHub Issue: #5
Branch: `feature/0005-application-result-handling-ui-feedback`
Milestone: `v0.3.0`

## Goal

Introduce explicit Application result handling for expected use-case outcomes and improve UI feedback for those outcomes.

Guildwise should distinguish between expected business/application outcomes and real technical failures.

Expected outcomes such as "not found", "invalid input", "already exists" or "not allowed by a domain rule" should be represented as structured results instead of being treated as unexpected exceptions.

Technical failures such as broken configuration, unavailable database connections, corrupted persistence state or programming defects should still be allowed to fail through exceptions.

## Philosophy

Guildwise should not use exceptions as normal control flow.

Expected outcomes are part of the use case contract.

Examples of expected outcomes:

* A player was not found.
* A guild was not found.
* A character was not found.
* A required value is missing.
* A player is already part of a guild.
* A player is already part of a raid team.
* A player cannot be added to a raid team because they do not have a main character.
* A character class and specialization combination is invalid.
* A command cannot be completed because the requested state already exists.

Examples of real failures:

* PostgreSQL is unavailable.
* EF Core cannot save changes because of a database failure.
* Runtime configuration is invalid.
* A required dependency is missing.
* The application reaches an impossible internal state.
* A programming defect causes an unexpected exception.

## User Value

After this feature, users should see understandable feedback in the Web UI when an expected action cannot be completed.

Instead of a raw exception or generic failure, the UI should show messages such as:

* `Player was not found.`
* `Character name is required.`
* `Player is already a member of this raid team.`
* `The selected character specialization is not valid for this class.`

This makes the current verification UI more stable and prepares the project for a proper roster management interface.

## Technical Value

This feature improves:

* Use-case contracts.
* Application-layer clarity.
* UI feedback.
* Testability of expected failure paths.
* Separation between expected outcomes and real technical failures.
* Long-term maintainability before more product UI is built.

## In Scope

This feature includes:

* Add minimal result primitives in the Application layer.
* Use `Result` for command handlers that do not need to return a value.
* Use `Result<T>` for command handlers that return a DTO.
* Keep query handlers nullable or collection-based where appropriate.
* Convert expected "not found" command outcomes from exceptions to result failures.
* Convert expected validation/business-rule command outcomes to result failures where practical.
* Keep real technical failures as exceptions.
* Update the Web verification UI to display result failures clearly.
* Update unit tests for result success and failure outcomes.
* Update integration tests where affected.
* Update `CHANGELOG.md` under `[Unreleased]`.

## Out of Scope

This feature does not include:

* Full UI redesign.
* Authentication.
* Authorization.
* External WoW API integrations.
* Battle.net login.
* Raider.IO integration.
* Blizzard API integration.
* Warcraft Logs integration.
* Discord bot.
* MediatR.
* FluentValidation.
* A global exception middleware strategy.
* A full API layer.
* A new UnitOfWork abstraction.
* Transaction-boundary redesign.
* EF mapping changes.
* New EF migrations.
* Async persistence changes.
* Repository behavior changes.
* Domain model redesign.

## Architecture Rules

The existing architecture rules remain in force.

* Domain must not reference Application result types.
* Domain must not reference EF Core.
* Application must remain EF-free.
* Infrastructure must not define Application result semantics.
* Web may consume Application results.
* Web must not inject repositories directly.
* Web must not inject `DbContext` directly.
* Blazor components should use Application handlers.
* Repository interfaces remain in Application.
* Repository implementations remain in Infrastructure.
* Do not create repositories for:

    * `Character`
    * `RaidTeam`
    * `GuildMember`
    * `RaidTeamMember`

## Result Design

Add minimal result primitives to `Guildwise.Application`.

Suggested location:

```text
src/Guildwise.Application/Common/Results/
  Result.cs
  ResultOfT.cs
  Failure.cs
  FailureType.cs
```

Use the term `Failure`, not `Error`, to avoid treating expected use-case outcomes as technical errors.

### FailureType

Suggested initial failure types:

```csharp
public enum FailureType
{
    NotFound,
    Validation,
    Conflict,
    BusinessRule
}
```

Meaning:

* `NotFound`: requested entity does not exist.
* `Validation`: input is missing or structurally invalid.
* `Conflict`: requested action conflicts with existing state.
* `BusinessRule`: requested action violates a domain/use-case rule.

### Failure

Suggested shape:

```csharp
public sealed record Failure(FailureType Type, string Message);
```

Keep the initial model simple.

Do not add localization, field-level validation dictionaries or HTTP status codes in this feature.

### Result

Suggested shape:

```csharp
public sealed record Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Failure? Failure { get; }

    private Result(bool isSuccess, Failure? failure)
    {
        IsSuccess = isSuccess;
        Failure = failure;
    }

    public static Result Success() => new(true, null);

    public static Result NotFound(string message) =>
        new(false, new Failure(FailureType.NotFound, message));

    public static Result Validation(string message) =>
        new(false, new Failure(FailureType.Validation, message));

    public static Result Conflict(string message) =>
        new(false, new Failure(FailureType.Conflict, message));

    public static Result BusinessRule(string message) =>
        new(false, new Failure(FailureType.BusinessRule, message));
}
```

### Result<T>

Suggested shape:

```csharp
public sealed record Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public Failure? Failure { get; }

    private Result(bool isSuccess, T? value, Failure? failure)
    {
        IsSuccess = isSuccess;
        Value = value;
        Failure = failure;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> NotFound(string message) =>
        new(false, default, new Failure(FailureType.NotFound, message));

    public static Result<T> Validation(string message) =>
        new(false, default, new Failure(FailureType.Validation, message));

    public static Result<T> Conflict(string message) =>
        new(false, default, new Failure(FailureType.Conflict, message));

    public static Result<T> BusinessRule(string message) =>
        new(false, default, new Failure(FailureType.BusinessRule, message));
}
```

Exact implementation may differ if it remains simple and readable.

## Query Handling Rules

Query handlers should not return `Result<T>` by default.

Preferred query behavior:

### Get by id

If the entity does not exist, return `null`.

Example:

```csharp
Task<GuildDto?> HandleAsync(GetGuildQuery query, CancellationToken cancellationToken = default);
```

### List

If no entities exist, return an empty collection.

Example:

```csharp
Task<IReadOnlyCollection<GuildDto>> HandleAsync(ListGuildsQuery query, CancellationToken cancellationToken = default);
```

Rationale:

* "Not found" for a query is a valid data outcome.
* Empty lists are valid data outcomes.
* These cases do not need `Result`.

## Command Handling Rules

Command handlers should return `Result` or `Result<T>`.

### Commands returning data

Example:

```csharp
Task<Result<CharacterDto>> HandleAsync(CreateCharacterCommand command, CancellationToken cancellationToken = default);
```

### Commands not returning data

Example:

```csharp
Task<Result> HandleAsync(DeletePlayerCommand command, CancellationToken cancellationToken = default);
```

### Expected failures

Expected failures should return a failed result.

Examples:

```csharp
return Result<PlayerDto>.NotFound($"Player '{command.PlayerId}' was not found.");
```

```csharp
return Result.Validation("Character name is required.");
```

```csharp
return Result.Conflict("Player is already a member of this raid team.");
```

```csharp
return Result.BusinessRule("Player must have a main character before joining a raid team.");
```

### Real failures

Do not catch and convert real technical failures unless there is a clear use-case reason.

Examples that should still throw:

* Database connection failure.
* EF Core persistence failure.
* Invalid runtime configuration.
* Unexpected null state.
* Programming defects.

## Domain Exception Strategy

Domain entities may still throw exceptions to protect invariants.

This feature does not require converting Domain methods to `Result`.

Application handlers should avoid expected Domain exceptions where practical by checking expected conditions before calling Domain methods.

Example:

```csharp
var player = await playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);

if (player is null)
{
    return Result<CharacterDto>.NotFound($"Player '{command.PlayerId}' was not found.");
}
```

If a Domain method still throws because an invariant is violated, decide case by case:

* If the violation is expected from user input, convert it in the Application handler to a `Result` failure.
* If the violation indicates a programming defect or impossible state, allow the exception to remain.

Do not weaken Domain invariants.

## Handler Conversion Strategy

Convert command handlers in a controlled way.

### Commands likely returning Result<T>

* `CreateGuild`
* `CreatePlayer`
* `CreateCharacter`
* `CreateRaidTeam`
* `AddPlayerToGuild`
* `AddPlayerToRaidTeam`

### Commands likely returning Result

* `UpdateGuild`
* `DeleteGuild`
* `UpdatePlayer`
* `DeletePlayer`
* `UpdateCharacter`
* `DeleteCharacter`
* `SetMainCharacter`
* `UpdateRaidTeam`
* `DeleteRaidTeam`
* `RemovePlayerFromRaidTeam`
* `AddAdditionalRoleToGuildMember`
* `RemoveAdditionalRoleFromGuildMember`

Exact handler return shapes should be based on the current handler contracts.

Query handlers should generally remain as nullable DTOs or collections.

## Expected Failure Cases To Cover

At minimum, cover the most important expected failure cases.

### Player

* Updating a missing player returns `NotFound`.
* Deleting a missing player returns `NotFound`.
* Creating a player with a blank display name returns `Validation` if currently possible.

### Guild

* Updating a missing guild returns `NotFound`.
* Deleting a missing guild returns `NotFound`.
* Adding a player to a missing guild returns `NotFound`.
* Adding a missing player to a guild returns `NotFound`.

### Character

* Creating a character for a missing player returns `NotFound`.
* Updating a missing player or character returns `NotFound`.
* Setting a missing character as main returns `NotFound`.
* Invalid class/specialization combination returns `Validation` or `BusinessRule`.

### Raid Team

* Creating a raid team for a missing guild returns `NotFound`.
* Adding a player to a missing raid team returns `NotFound`.
* Adding a missing player to a raid team returns `NotFound`.
* Adding a player without a main character returns `BusinessRule`.
* Adding a player who is already in the raid team returns `Conflict` or `BusinessRule`.
* Removing a missing player from a raid team returns `NotFound` or `BusinessRule`, depending on current domain semantics.

### Guild Member Roles

* Adding an additional role to a missing guild member returns `NotFound`.
* Removing an additional role from a missing guild member returns `NotFound`.
* Adding an already assigned additional role returns `Conflict` or `BusinessRule`.

## UI Feedback Requirements

Update the current Web verification UI to consume Application results.

The UI should:

* Show success messages for successful commands.
* Show readable failure messages for failed results.
* Avoid displaying raw exception text for expected outcomes.
* Keep unexpected exceptions visible enough for development diagnosis.
* Avoid injecting repositories or `DbContext`.

Preferred behavior in `Home.razor`:

* `RunActionAsync` should accept an async action returning `Result` or should have helper overloads for `Result` and `Result<T>`.
* On success, refresh data.
* On expected failure, show the failure message.
* On unexpected exception, show a generic unexpected failure message and optionally include exception details only in Development if already available.

Keep UI design changes minimal.

This feature is not a UI redesign.

## Testing Requirements

Update tests to assert result outcomes instead of expected exceptions for converted command handlers.

### Unit Tests

Add or update unit tests for:

* Success result outcomes.
* NotFound result outcomes.
* Validation result outcomes where applicable.
* Conflict or BusinessRule outcomes where applicable.
* Query handlers returning `null` or empty lists.

Use assertions such as:

```csharp
result.IsSuccess.Should().BeTrue();
```

or existing assertion style.

For failed results:

```csharp
result.IsFailure.Should().BeTrue();
result.Failure!.Type.Should().Be(FailureType.NotFound);
```

Use the existing test framework and assertion style.

### Integration Tests

Update integration tests if handler return types change.

Existing persistence behavior should remain covered.

Integration tests should still use Testcontainers PostgreSQL and must not depend on local Docker Compose PostgreSQL.

### Architecture Tests

Architecture tests should continue to pass.

No new dependency direction should be introduced.

## Documentation Requirements

Update `CHANGELOG.md` under `[Unreleased]`.

Suggested entry:

```text
Added Application result handling for expected use-case outcomes and UI feedback.
```

Update `README.md` only if there is a meaningful developer-facing behavior change worth documenting.

Do not add heavy documentation unless useful.

## Suggested Implementation Slices

### 0005a: Result Primitives

Implement:

* `Result`
* `Result<T>`
* `Failure`
* `FailureType`
* unit tests for result primitives if useful
* changelog entry if this slice is committed independently

Suggested commit:

```text
feat: add application result primitives
```

### 0005b: Convert Core Command Handlers

Convert the most important command handlers first:

* `CreateCharacter`
* `SetMainCharacter`
* `AddPlayerToGuild`
* `AddPlayerToRaidTeam`
* `DeletePlayer`

Update tests for these handlers.

Suggested commit:

```text
refactor: return results from core roster commands
```

### 0005c: Update Web UI Feedback

Update `Home.razor` to consume result outcomes.

Implement:

* success message from successful results
* failure message from failed results
* expected failure display
* unexpected exception fallback

Suggested commit:

```text
feat: show application result feedback in roster setup UI
```

### 0005d: Convert Remaining Command Handlers

Convert remaining command handlers consistently.

Update tests.

Suggested commit:

```text
refactor: return results from remaining roster commands
```

### 0005e: Failure Outcome Tests

Add or expand tests for expected failure paths.

Focus on:

* NotFound
* Validation
* Conflict
* BusinessRule

Suggested commit:

```text
test: cover expected roster command failure outcomes
```

## Acceptance Criteria

### Result Model

* Application contains minimal result primitives.
* Result primitives do not depend on Web, Infrastructure or Domain.
* Result primitives use the term `Failure`.
* Failure types distinguish at least:

    * NotFound
    * Validation
    * Conflict
    * BusinessRule

### Query Behavior

* Query handlers may return `null` for missing single entities.
* Query handlers may return empty collections.
* Query handlers are not forced into `Result<T>` unless there is a strong reason.

### Command Behavior

* Command handlers return `Result` or `Result<T>`.
* Expected command failures return structured failures.
* NotFound command outcomes do not throw normal Application exceptions.
* Validation/business-rule command outcomes are represented as failures where practical.
* Real technical failures may still throw exceptions.

### UI Behavior

* Web UI displays expected command failures clearly.
* Web UI still displays success messages for successful commands.
* Web UI does not inject repositories.
* Web UI does not inject `DbContext`.
* Web UI still uses Application handlers.

### Architecture

* Domain remains EF-free.
* Application remains EF-free.
* Infrastructure remains persistence-focused.
* Web consumes Application results.
* Architecture tests pass.

### Tests

* Unit tests cover success and failure result outcomes.
* Integration tests still pass.
* Testcontainers-based persistence tests still pass.
* `dotnet build Guildwise.sln` succeeds.
* `dotnet test Guildwise.sln` succeeds.
* GitHub Actions CI passes.

### Changelog

* `CHANGELOG.md` is updated under `[Unreleased]`.

## Manual Verification

After implementation, verify locally:

1. Start local PostgreSQL:

   ```bash
   docker compose up -d
   ```

2. Apply migrations:

   ```bash
   dotnet tool run dotnet-ef database update \
     --project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
     --startup-project ./src/Guildwise.Infrastructure/Guildwise.Infrastructure.csproj \
     --context GuildwiseDbContext
   ```

3. Run the Web app:

   ```bash
   dotnet run --project ./src/Guildwise.Web/Guildwise.Web.csproj
   ```

4. Perform successful actions in the verification UI.

5. Perform expected failing actions if possible, such as:

    * submit a missing required field
    * select no valid player
    * try an invalid operation from the UI

6. Confirm the UI shows readable feedback instead of raw expected exceptions.

7. Run:

   ```bash
   dotnet build Guildwise.sln
   dotnet test Guildwise.sln
   ```

## Risks and Open Questions

### Result Pattern Scope

Converting every handler at once may be a large diff.

If the diff becomes too large, convert command handlers in slices.

### Domain Exceptions

Some expected failures may currently be thrown from Domain methods.

Application handlers should handle predictable cases before calling Domain methods where practical.

Do not weaken Domain invariants.

### Failure Type Naming

`FailureType` is intentionally used instead of `ErrorType`.

This reflects the distinction between expected use-case failures and unexpected technical errors.

### UI Feedback Depth

The current UI is a verification UI, not final product UI.

Keep feedback useful but simple.

### Future API Layer

If Guildwise later adds HTTP APIs, `FailureType` can be mapped to HTTP status codes there.

This mapping is out of scope for this feature.

### Localization

Failure messages are plain English for now.

Localization is out of scope.

## Done Definition

This feature is done when:

* Application result primitives exist.
* Command handlers return `Result` or `Result<T>`.
* Query handlers keep nullable/empty-list semantics where appropriate.
* Expected command outcomes are no longer modeled as normal exceptions.
* UI displays expected result failures clearly.
* Real technical failures can still surface as exceptions.
* Domain invariants remain protected.
* Domain remains independent of Application results.
* Application remains independent of EF Core.
* Infrastructure remains independent of UI concerns.
* Existing persistence behavior remains unchanged.
* No EF migration is created.
* Unit tests pass.
* Integration tests pass.
* Architecture tests pass.
* GitHub Actions CI passes.
* `CHANGELOG.md` has an entry under `[Unreleased]`.
