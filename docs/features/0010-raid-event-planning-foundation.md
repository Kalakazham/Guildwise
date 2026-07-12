# Feature 0010: Raid Event Planning Foundation

## Tracking

GitHub Issue: TBD
Branch: `feature/0010-raid-event-planning-foundation`
Milestone: TBD

## Goal

Introduce raid events as a first-class Guildwise planning concept.

Raid leads should be able to plan raid dates and connect them to a raid team. A raid event initially contains manual planning data:

* Title.
* Raid team.
* Date and start time.
* Optional end time or duration.
* Instance or raid name.
* Difficulty.
* Notes.

This feature creates the foundation for later signup and attendance features.

Guildwise remains manual-first, but raid event planning must not block later WoWAudit event, schedule or signup integration.

This feature does not implement external API integration.

## User Value

After this feature, a guild or raid organizer should be able to:

* See planned raid dates in Guildwise.
* Understand which raid team a planned raid belongs to.
* Continue planning from existing roster and raid team data.
* Use Guildwise as the first product surface for raid scheduling.
* Build later signup and attendance workflows on top of raid events.

## Technical Value

This feature should:

* Introduce a raid event foundation model.
* Keep event planning separate from later signup and attendance behavior.
* Use existing guild, raid team, Application and Result patterns.
* Prepare for later WoWAudit import or sync capability.
* Preserve Clean Architecture boundaries.
* Avoid external API clients in this feature.

## In Scope

This feature includes:

* Add a raid event Domain, Application and persistence foundation if one does not already exist.
* Add raid event as a plannable object.
* Assign each raid event to a raid team.
* Derive guild context from the raid team and make it visible in the UI.
* Add a raid event overview page or overview region.
* Add raid event detail display.
* Create raid events.
* Optionally edit raid events if that keeps the first product slice coherent.
* Optionally cancel or delete raid events if the product flow needs it, without overbuilding.
* Select a raid team when creating an event.
* Display event title.
* Display raid team context.
* Display date and start time.
* Display optional duration or end time.
* Display instance or raid name.
* Display difficulty.
* Display notes.
* Add empty state when no raid teams exist.
* Add empty state when no raid events exist.
* Add loading and error states where appropriate.
* Show expected Application result failures in the UI.
* Update `CHANGELOG.md` under `[Unreleased]` when implementing the feature.

## Future External Integration Considerations

WoWAudit is a planned future source for:

* Raid events.
* Raid schedules.
* Signups.

This feature does not implement the WoWAudit API.

Raid event planning starts manual-first. The model and Application flows must not block later external sources.

Imported events will likely need source metadata later when sync or conflict behavior becomes relevant.

Manual events and imported events must be distinguishable later where sync, conflicts or resync behavior matter.

Do not introduce provider-specific API DTOs into Domain.

Do not introduce WoWAudit-specific DTOs into Domain.

Web must not call external APIs directly.

Possible later source metadata:

```text
Source
ExternalId
LastSyncedAt
SyncStatus
SourceVersion
```

Not every field needs to exist in 0010.

Source metadata should not be introduced in 0010 unless the implementation plan explicitly justifies why the manual foundation already needs it. Avoid speculative `Source`, `ExternalId`, `SyncStatus` or similar fields in 0010a without clear justification.

## Out of Scope

This feature does not include:

* WoWAudit API integration.
* Blizzard API integration.
* Raider.IO integration.
* Warcraft Logs integration.
* External API clients.
* Auth, API key or OAuth configuration.
* Background jobs.
* Sync engine.
* Import UI.
* Conflict resolution UI.
* Signups.
* Attendance.
* Calendar sync.
* Discord bot.
* AI recommendations.
* Automatic raid planning.
* Team optimization.
* Loot.
* Performance analysis.
* Recruitment.
* New UI library.
* New design system.
* Mobile-perfect calendar UI.

## Product Direction

Raid event planning should build on the Guildwise Web UI from Features 0007, 0008 and 0009.

Preferred direction:

* Dark Guildwise/WoW-inspired admin and raid-tool UI.
* Reuse the existing app shell and navigation.
* Reuse existing content panels, summary cards, empty states and loading states.
* Reuse existing badge components where event context needs roster or raid team metadata.
* Keep event UI productive and readable.
* Use AdminLTE only as structural inspiration, not as a direct dependency.

Avoid:

* Decorative calendar UI that reduces readability.
* Full design system work.
* New UI library adoption.
* Styling concepts leaking into Domain or Application.

## Architecture Rules

The existing architecture rules remain in force.

* Domain must not reference Web, Infrastructure, Blazor, styling concepts or external API DTOs.
* Application must not reference Web, Infrastructure, CSS, Blazor or concrete API clients.
* Infrastructure implements persistence.
* Web uses Application handlers, services or queries.
* Blazor components must not use `DbContext` directly.
* Blazor components must not inject repository implementations directly.
* Expected failures should use existing `Result`, `Result<T>`, `Failure` and `FailureType` patterns.
* Transaction boundaries must be considered for mutating multi-aggregate flows.
* No external API DTOs should be introduced into Domain.
* No WoWAudit-specific concepts should be hard-coded into Domain except consciously generic source metadata if it is explicitly needed.

## Domain and Application Guidance

Possible Domain concepts:

```text
RaidEvent
RaidEventStatus
RaidDifficulty
RaidInstance
InstanceName
```

Keep the model simple for the first implementation.

`InstanceName` as a string may be sufficient before normalizing raid instances.

`RaidEventStatus` and `RaidDifficulty` should be added only when they are useful for validation and UI behavior in this feature.

Possible Application use cases:

```text
CreateRaidEvent
GetRaidEvent
ListRaidEvents
UpdateRaidEvent
CancelRaidEvent
DeleteRaidEvent
GetRaidEventPlanningOverview
```

Guidance:

* A raid event should be assigned to a raid team.
* Raid team belongs to a guild, so the guild context must be clear.
* Expected validation and business failures should return `Result` or `Result<T>`.
* Do not introduce signups in this feature.
* Do not introduce attendance records in this feature.
* Do not introduce external source-specific API DTOs.

## Persistence Guidance

If raid events are persisted, persistence belongs in Infrastructure.

EF Core mappings belong in Infrastructure.

Migrations belong in Infrastructure.

Do not add EF Core attributes to Domain entities.

Repository design should be decided deliberately:

* Use an existing aggregate if raid events are naturally managed through `Guild`.
* Add a new repository only if `RaidEvent` is intentionally modeled as its own aggregate root.

The implementation plan must justify that choice.

Do not introduce broad persistence redesigns.

## Suggested PR Slices

### 0010a: Raid Event Domain/Application/Persistence Foundation

* Add Domain and Application model for raid events.
* Add persistence and migration if needed.
* Add use cases for create, list and get.
* Add tests for validation and basic flows.
* Do not build a large UI.

### 0010b: Raid Event Overview and Detail UI

* Add route for raid events.
* Add event overview.
* Add event detail display.
* Add empty, loading and error states.
* Show raid team and guild context.
* Reuse existing Guildwise Web components.

### 0010c: Raid Event Create/Edit UX and Polish

* Add event create form.
* Optionally add edit form.
* Display expected Application result failures.
* Polish spacing, states and repeated-use workflow.
* Update `CHANGELOG.md`.

## Acceptance Criteria

This feature is accepted when:

* Raid events can be modeled as a foundation.
* Raid events can be created.
* Raid events can be listed.
* Raid event details are visible.
* Each raid event has clear raid team and guild context.
* Expected validation and business failures use `Result` and `Failure`.
* No signups are introduced.
* No attendance behavior is introduced.
* No external API integration is introduced.
* Later WoWAudit integration is not blocked.
* Architecture tests pass.
* `dotnet build Guildwise.sln` passes.
* `dotnet test Guildwise.sln` passes.
* `CHANGELOG.md` is updated during implementation.

## Manual Verification

Manual verification should cover:

1. Start the local database.
2. Apply migrations if needed.
3. Start the Web app.
4. Create or use existing guild, roster and raid team data.
5. Open raid events.
6. Create a raid event.
7. Verify that the event appears in the overview.
8. Verify event details.
9. Test invalid input.
10. Verify that roster overview still works.
11. Verify that raid team management still works.
12. Run `dotnet build Guildwise.sln`.
13. Run `dotnet test Guildwise.sln`.

## Risks and Open Questions

Known risks:

* `RaidEvent` aggregate root versus management inside `Guild` must be decided deliberately.
* Source and external id metadata should not be overengineered too early.
* The model must keep a later WoWAudit event and signup source possible.
* Event time zones, realm and region may become important later.
* Difficulty and instance may start as simple fields, but should not block later normalization.
* Cancel versus delete needs a product decision.
* UI scope could drift into attendance or signup workflows.
* A full calendar UI would be premature in the first slice.

Open questions:

* Should the first implementation support edit, cancel or delete, or only create and display?
* Should event duration be stored directly or derived from start and end time?
* Should source metadata be introduced in 0010 or deferred until the first import/sync feature?
* Should raid events be queried primarily by guild, raid team or date range?

## Done Definition

The feature is done when:

* Raid event planning foundation is documented and implemented.
* Raid events can be created and displayed.
* Raid team and guild context is clear.
* Empty, error and loading states are handled.
* Result and Failure patterns are preserved.
* Transaction and persistence rules are followed.
* No external APIs are introduced.
* Later WoWAudit integration is not blocked.
* Build and tests are green.
* `CHANGELOG.md` is updated.
