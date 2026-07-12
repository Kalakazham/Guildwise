# Feature 0012: Raid Event Signup Foundation

## Tracking

GitHub Issue: TBD
Branch: `feature/0012-raid-event-signup-foundation`
Milestone: TBD

## Goal

Introduce raid event signups as a first-class Guildwise planning concept.

Players or guild members should be able to sign up for raid events.

Raid leads should be able to see who is signed, tentative or declined for an event.

Signups build on existing raid events, raid teams, guild members and main characters.

Cancelled raid events should not be normally signup-capable.

This feature creates the foundation for later attendance features.

Guildwise remains manual-first, but raid event signup behavior must not block later WoWAudit signup integration.

This feature does not implement external API integration.

## User Value

After this feature, a guild or raid organizer should be able to:

* See who wants to attend a raid event.
* Keep signup state inside Guildwise instead of only outside the tool.
* Make event planning more concrete and raid-focused.
* See early whether enough tanks, healers and DPS are signed.
* Build later attendance workflows on top of raid event signups.

## Technical Value

This feature should:

* Introduce a signup model for raid events.
* Keep signup behavior separate from later attendance behavior.
* Use existing raid event, raid team, guild member and player structures.
* Reuse existing `Result`, `Result<T>`, `Failure` and `FailureType` patterns.
* Prepare for later WoWAudit signup synchronization.
* Avoid external API clients in this feature.
* Avoid `Source`, `ExternalId`, sync or import metadata unless the implementation plan explicitly justifies why this manual signup feature already needs it.

## In Scope

This feature includes:

* Add a signup Domain, Application and persistence foundation if one does not already exist.
* Add signup as a registration for a raid event.
* Assign each signup to a raid event.
* Assign each signup to a player or guild member.
* Show main character context for signups where available.
* Add a provider-neutral signup status.
* Support at least:
  * `Signed`.
  * `Tentative`.
  * `Declined`.
* Optionally support `Bench` or `Confirmed` only if the implementation plan clearly shows they are needed now; otherwise defer them.
* Add create or update signup use case.
* Add list signups for raid event use case.
* Add signup overview in the raid event detail UI on `/raid-events`.
* Add signup action for available guild members.
* Display signup summary:
  * Signed.
  * Tentative.
  * Declined.
  * Missing response or not signed.
* Display signup role composition:
  * Tanks.
  * Healers.
  * DPS.
* Show expected Application result failures in the UI.
* Add empty states for:
  * Event without signups.
  * No available guild members.
  * Cancelled event is not signup-capable.
* Update `CHANGELOG.md` under `[Unreleased]` when implementing the feature.

## Future External Integration Considerations

WoWAudit is a planned future source for:

* Raid events.
* Raid schedules.
* Signups.

This feature does not implement the WoWAudit API.

Raid event signup starts manual-first. The model and Application flows must not block later WoWAudit signups.

Manual signups and imported or synced signups must remain distinguishable later where sync, conflicts or resync behavior matter.

Do not introduce provider-specific API DTOs into Domain.

Do not introduce WoWAudit-specific DTOs into Domain.

Web must not call external APIs directly.

Source, external id and sync metadata remain deferred unless the implementation plan explicitly justifies why this feature already needs them.

WoWAudit sync conflicts, source precedence and conflict resolution UI are not part of this feature.

## Out of Scope

This feature does not include:

* Attendance.
* Attendance records.
* Check-in or actual attendance.
* Warcraft Logs attendance inference.
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
* Calendar sync.
* Discord bot.
* AI recommendations.
* Automatic signup or roster optimization.
* Team optimization.
* Loot.
* Performance analysis.
* Recruitment.
* Authentication.
* Permission implementation.
* New UI library.
* New design system.
* Mobile-perfect calendar UI.

## Product Direction

Raid event signup UX should build on the Guildwise Web UI from Features 0007, 0008, 0009, 0010 and 0011.

Preferred direction:

* Dark Guildwise/WoW-inspired admin and raid-tool UI.
* Show signup state inside the raid event detail experience.
* Reuse the existing app shell and navigation.
* Reuse existing content panels, summary cards, empty states and loading states.
* Reuse existing class, role, guild rank, main character and raid team badge components where useful.
* Make signup status clearly visible without overdecorating the page.
* Use AdminLTE only as structural inspiration, not as a direct dependency.

Avoid:

* Attendance UI.
* Signup optimization or roster recommendation behavior.
* A new design system.
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
* Transaction boundaries must be considered for mutating flows.
* No external API DTOs should be introduced into Domain.
* No WoWAudit-specific concepts should be hard-coded into Domain.
* Do not add source or sync fields without a consciously justified decision.

## Domain and Application Guidance

Possible Domain concepts:

```text
RaidEventSignup
RaidEventSignupStatus
SignupStatus
RaidEventSignupSummary
```

Likely signup status values:

```text
Signed
Tentative
Declined
```

`Bench` and `Confirmed` should be deferred unless the implementation plan shows they are useful now.

Possible Application use cases:

```text
SetRaidEventSignup
ListRaidEventSignups
GetRaidEventSignupOverview
ClearRaidEventSignup
RemoveRaidEventSignup
```

Guidance:

* Signup belongs to a raid event.
* Signup should be addressed primarily by `PlayerId`.
* Guild membership should be validated through the guild context of the raid event.
* Do not force a separate `GuildMemberId` concept for this feature.
* Signup should only be normally created or changed for scheduled raid events.
* Cancelled raid events should reject normal signup changes.
* A player should have at most one current signup status per event.
* Missing response or not signed state should be derived from guild members without a signup for the event.
* Missing response should not be modeled as its own stored signup status.
* Signup is not attendance.
* Signup is not performance data.
* Expected validation and business failures should return `Result` or `Result<T>`.
* Do not introduce external source-specific API DTOs.

## Persistence Guidance

If raid event signups are persisted, persistence belongs in Infrastructure.

EF Core mappings belong in Infrastructure.

Migrations belong in Infrastructure.

Do not add EF Core attributes to Domain entities.

Repository design should be decided deliberately:

* Signups should be managed as child entities of `RaidEvent` for the first implementation unless the implementation plan finds a strong reason against it.
* A separate signup repository should be avoided.
* `IRaidEventRepository` should probably remain the persistence access for raid events and their signups.

Do not introduce broad persistence redesigns.

Do not add source, external id or sync fields in the migration unless the implementation plan explicitly justifies them.

## Suggested PR Slices

### 0012a: Signup Domain/Application/Persistence Foundation

* Add signup Domain model.
* Add signup status.
* Add use cases for setting and listing signups.
* Add persistence and migration if needed.
* Add tests for validation and basic flows.
* Do not build a large UI.

### 0012b: Signup Overview UI

* Add signup summary in the raid event detail UI.
* Display signups for the selected event.
* Display signup status badges.
* Display role composition for signups.
* Add empty, loading and error states.

### 0012c: Signup Management UX and Polish

* Set or change signup status.
* Remove or clear signup if this is in scope.
* Display expected Application result failures.
* Polish spacing, states and repeated-use workflow.
* Handle cancelled events clearly in the UI.

## Acceptance Criteria

This feature is accepted when:

* Raid event signups can be modeled as a foundation.
* A player or guild member can receive a signup status for a scheduled raid event.
* Signup status can be displayed.
* Signup summary is visible.
* Each event has no duplicate current signups for the same player.
* Cancelled events are not normally signup-capable.
* Signups remain clearly separate from attendance.
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
5. Open a raid event.
6. Set signup for a guild member.
7. Change signup status.
8. Verify signup overview.
9. Verify cancelled event behavior.
10. Test invalid input.
11. Verify that create, edit and cancel raid event still work.
12. Verify that roster overview still works.
13. Verify that raid team management still works.
14. Run `dotnet build Guildwise.sln`.
15. Run `dotnet test Guildwise.sln`.

## Risks and Open Questions

Known risks:

* Signup as child entity of `RaidEvent` versus its own aggregate root must be decided deliberately.
* Later WoWAudit signups can create sync conflicts.
* Source and sync metadata must not be introduced speculatively.
* Attendance could accidentally drift into signup scope.
* Cancelled event signup behavior must remain clear.
* Removing a signup versus setting status to `Declined` must be decided.
* `Bench` and `Confirmed` may become useful later but should not be introduced prematurely.
* Authentication and permissions remain later concerns.

Open questions:

* Should clearing a signup be supported in the first implementation?
* Should role composition use main character role only, or allow explicit signup role later?

## Done Definition

The feature is done when:

* Raid event signup foundation is documented and implemented.
* Signups can be set and displayed.
* Signup status is clearly visible.
* Signup summary is visible.
* Cancelled events are not normally signup-capable.
* Signups and attendance remain separate.
* Result and Failure patterns are preserved.
* Transaction and persistence rules are followed.
* No external APIs are introduced.
* Later WoWAudit integration is not blocked.
* Build and tests are green.
* `CHANGELOG.md` is updated.
