# Feature 0011: Raid Event Lifecycle UX

## Tracking

GitHub Issue: TBD
Branch: `feature/0011-raid-event-lifecycle-ux`
Milestone: TBD

## Goal

Make raid events manageable after creation.

Raid leads should be able to correct planned raid events, cancel raid events and see event lifecycle state in the raid event overview and detail UI.

This feature builds on the raid event foundation from Feature 0010.

Guildwise remains manual-first, but raid event lifecycle behavior must not block later WoWAudit event, schedule or signup integration.

This feature does not implement external API integration.

## User Value

After this feature, a guild or raid organizer should be able to:

* Correct mistakes in planned raid events.
* Move raid dates or update raid details.
* Cancel raid events without losing the event history.
* See clearly whether a raid event is scheduled or cancelled.
* Build later signup and attendance workflows on top of a more stable event lifecycle.

## Technical Value

This feature should:

* Introduce a simple provider-neutral raid event status.
* Add Application use cases for updating and cancelling raid events.
* Strengthen the foundation for later signups, attendance and WoWAudit sync.
* Reuse existing `Result`, `Result<T>`, `Failure` and `FailureType` patterns.
* Preserve existing transaction and persistence rules.
* Avoid external API clients in this feature.
* Avoid `Source`, `ExternalId`, sync or import metadata unless the implementation plan explicitly justifies why this manual lifecycle feature already needs it.

## In Scope

This feature includes:

* Add `RaidEventStatus` or an equivalent provider-neutral status concept.
* Support at least `Scheduled` and `Cancelled`.
* Optionally support `Completed` only if the implementation plan clearly shows it is needed for this feature; otherwise defer it.
* Add an update or edit raid event Application use case.
* Add a cancel raid event Application use case.
* Add edit raid event UI on `/raid-events`.
* Add cancel raid event UI on `/raid-events`.
* Display status in the event overview.
* Display status in the event detail panel.
* Add a status badge or similar Web-only status presentation.
* Show expected Application result failures in the UI.
* Show success messages for update and cancel actions.
* Reload the overview after update or cancel and preserve the current selection where possible.
* Preserve the existing create raid event UX.
* Preserve the existing roster and raid team pages.
* Update `CHANGELOG.md` under `[Unreleased]` when implementing the feature.

## Out of Scope

This feature does not include:

* Delete raid event.
* Hard delete.
* Signups.
* Attendance.
* Calendar sync.
* WoWAudit API integration.
* Blizzard API integration.
* Raider.IO integration.
* Warcraft Logs integration.
* External API clients.
* Source, external id or sync metadata.
* Background jobs.
* Sync engine.
* Import UI.
* Conflict resolution UI.
* Authentication.
* Permission implementation.
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

Raid event lifecycle UX should build on the Guildwise Web UI from Features 0007, 0008, 0009 and 0010.

Preferred direction:

* Dark Guildwise/WoW-inspired admin and raid-tool UI.
* Reuse the existing app shell and navigation.
* Reuse existing content panels, summary cards, empty states and loading states.
* Reuse existing badge components where event context needs raid team or roster metadata.
* Make event status clearly visible without overdecorating the page.
* Make cancel a deliberate action without making it unnecessarily dramatic.
* Use AdminLTE only as structural inspiration, not as a direct dependency.

Avoid:

* A new design system.
* New UI library adoption.
* Styling concepts leaking into Domain or Application.
* UI scope drifting into signups or attendance.

## Future External Integration Considerations

WoWAudit is a planned future source for:

* Raid events.
* Raid schedules.
* Signups.

This feature does not implement the WoWAudit API.

The lifecycle must work manual-first. Status and lifecycle behavior must not block later external sources.

A local cancelled status can later create conflict questions for imported or synced events. Conflict resolution is not part of this feature.

Do not introduce provider-specific API DTOs into Domain.

Do not introduce WoWAudit-specific DTOs into Domain.

Web must not call external APIs directly.

Source, external id and sync metadata remain deferred unless the implementation plan explicitly justifies why this feature already needs them.

## Architecture Rules

The existing architecture rules remain in force.

* Domain must not reference Web, Infrastructure, Blazor, styling concepts or external API DTOs.
* Application must not reference Web, Infrastructure, CSS, Blazor or concrete API clients.
* Infrastructure implements persistence.
* Web uses Application handlers, services or queries.
* Blazor components must not use `DbContext` directly.
* Blazor components must not inject repository implementations directly.
* Expected failures should use existing `Result`, `Result<T>`, `Failure` and `FailureType` patterns.
* Transaction boundaries must be considered for mutating flows.
* No external API DTOs should be introduced into Domain.
* No WoWAudit-specific concepts should be hard-coded into Domain.
* Do not add source or sync fields without a consciously justified decision.

## Domain and Application Guidance

Possible Domain additions:

```text
RaidEventStatus
```

Likely status values:

```text
Scheduled
Cancelled
```

`Completed` should be deferred unless the implementation plan shows it is useful now.

Possible Domain methods:

```text
UpdateDetails(...)
Cancel()
Reschedule(...)
```

`Reschedule(...)` should only be added if `UpdateDetails(...)` is not clear enough.

Possible Application use cases:

```text
UpdateRaidEvent
CancelRaidEvent
GetRaidEvent
ListRaidEvents
```

Guidance:

* Cancel is preferred over hard delete.
* Cancelled events should remain visible.
* Update should return expected validation failures through `Result` or `Result<T>`.
* Cancel should return `NotFound` for missing events.
* Cancelling an already cancelled event should return idempotent success.
* Editing cancelled events should return a `BusinessRule` failure.
* `Completed` remains deferred for this feature.
* Do not introduce signups in this feature.
* Do not introduce attendance records in this feature.
* Do not introduce external source-specific API DTOs.

## Persistence Guidance

If raid event status is persisted, persistence belongs in Infrastructure.

EF Core mappings belong in Infrastructure.

Migrations belong in Infrastructure.

Do not add EF Core attributes to Domain entities.

`IRaidEventRepository` remains the aggregate-root repository for `RaidEvent`.

Do not add repositories for child entities.

Do not introduce broad persistence redesigns.

Do not add source, external id or sync fields in the migration unless the implementation plan explicitly justifies them.

## Suggested PR Slices

### 0011a: Raid Event Status and Lifecycle Foundation

* Add `RaidEventStatus` or an equivalent provider-neutral status concept.
* Add Domain methods for update and cancel.
* Add Application use cases for update and cancel.
* Add persistence and migration if status is newly persisted.
* Add tests for status, update and cancel behavior.
* Do not build a large UI.

### 0011b: Raid Event Edit UX

* Add edit form on `/raid-events`.
* Display expected Application result failures.
* Reload the overview after update and keep the event selected.
* Preserve the existing create, overview and detail UI.

### 0011c: Raid Event Cancel UX and Polish

* Add cancel action on `/raid-events`.
* Make status visible in overview and detail.
* Add cancelled event presentation.
* Display success and failure feedback.
* Polish spacing, states and repeated-use workflow.

## Acceptance Criteria

This feature is accepted when:

* Raid events have a visible lifecycle status.
* Raid events can be edited.
* Raid events can be cancelled.
* Cancelled events remain visible.
* Event overview shows status.
* Event detail shows status.
* Expected validation and business failures use `Result` and `Failure`.
* Hard delete is not the standard behavior.
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
4. Create or use existing guild, roster, raid team and raid event data.
5. Open raid events.
6. Edit a raid event.
7. Verify changes in overview and detail.
8. Cancel a raid event.
9. Verify status in overview and detail.
10. Test invalid input.
11. Verify that create still works.
12. Verify that roster overview still works.
13. Verify that raid team management still works.
14. Run `dotnet build Guildwise.sln`.
15. Run `dotnet test Guildwise.sln`.

## Risks and Open Questions

Known risks:

* Status can later interact with WoWAudit sync conflicts.
* Cancel and delete must remain deliberately separate.
* Event time zones remain a later topic.
* UI scope could drift into signups or attendance.
* Source and sync metadata must not be introduced speculatively.

Open questions:

* Should the edit UI reuse the create form layout or use a separate detail-panel edit mode?

## Done Definition

The feature is done when:

* Raid event lifecycle is documented and implemented.
* Events can be edited.
* Events can be cancelled.
* Status is visible in the UI.
* Empty, error and loading states remain handled.
* Result and Failure patterns are preserved.
* Transaction and persistence rules are followed.
* No external APIs are introduced.
* No signups or attendance behavior is introduced.
* Later WoWAudit integration is not blocked.
* Build and tests are green.
* `CHANGELOG.md` is updated.
