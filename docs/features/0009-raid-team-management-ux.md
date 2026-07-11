# Feature 0009: Raid Team Management UX

## Tracking

GitHub Issue: TBD
Branch: `feature/0009-raid-team-management-ux`
Milestone: TBD

## Goal

Make raid teams visible and manageable through a product-facing Guildwise Web experience.

Guildwise already has Domain, Application and persistence foundations for guilds, players, characters, main characters, guild members, raid teams and raid team membership. This feature moves raid team management out of setup and verification flows into a real user workflow.

Raid leads should be able to see:

* Which raid teams exist.
* Who belongs to each raid team.
* Which roles and classes are present in a team.
* Which players are not assigned to any raid team.
* Which players do not have a main character.

This feature builds on the existing roster and raid team foundation. It does not introduce raid events, attendance or external integrations.

## User Value

After this feature, a guild or raid organizer should be able to:

* Review raid teams more easily.
* Inspect team members without using setup or verification screens.
* Understand basic role composition for a team.
* Identify missing tanks, healers or damage dealers.
* See players who are not assigned to a raid team.
* Add players to a raid team.
* Remove players from a raid team.
* Prepare the product surface for later raid event planning.

## Technical Value

This feature should:

* Use existing raid team Application handlers and DTOs where possible.
* Extend the product-facing Web UI beyond the roster overview.
* Reuse UI building blocks from Features 0007 and 0008.
* Validate existing `Result`, `Result<T>`, `Failure` and `FailureType` handling in real UI flows.
* Stay within the existing Clean Architecture boundaries.
* Avoid a new persistence foundation.

## In Scope

This feature includes:

* Add a raid team management page.
* Show the current guild context.
* Operate raid team management within a selected guild.
* Make clear which guild is currently managed if multiple guilds exist.
* Add a raid team overview page or overview region.
* Add a raid team detail page or detail panel.
* List all raid teams.
* Add raid team summary cards.
* Display raid team member lists.
* Display role composition for tanks, healers and DPS.
* Reuse class and role badges.
* Reuse main/alt, guild rank and raid team badges where useful.
* Display players without a raid team.
* Display players without a main character.
* Add players to a raid team.
* Remove players from a raid team.
* Show expected Application result failures in the UI.
* Add empty states for no raid teams, teams without members and no available players to add.
* Add loading states where data is loaded asynchronously.
* Update `CHANGELOG.md` under `[Unreleased]` when implementing the feature.

## Optional Follow-up Within Feature

The feature may include focused follow-up polish if the core flow is already usable:

* Simple search or filtering for available players.
* Filter available players by role or class.
* Small composition hints such as `No tank assigned`.

These follow-ups must remain simple. Do not add automatic recommendations, AI logic or team optimization.

## Out of Scope

This feature does not include:

* Raid event planning.
* Calendar views.
* Raid signup.
* Attendance.
* Recruitment board.
* Applicant tracking.
* Blizzard API integration.
* Raider.IO integration.
* Warcraft Logs integration.
* Discord bot.
* AI recommendations.
* Automatic team optimization.
* Drag and drop.
* Authentication.
* Permission implementation.
* New UI library.
* New design system.
* New Domain aggregates, except minimal changes that are strictly required.
* New persistence foundation.
* EF Core redesign.
* New migrations, unless a clearly justified minimal field is explicitly required.

## Product Direction

The raid team management experience should build on the dark Guildwise/WoW-inspired UI direction from Features 0007 and 0008.

The page should feel like a productive admin and raid-management screen:

* Reuse the existing app shell and sidebar where appropriate.
* Reuse existing content panels, summary cards, empty states and loading states.
* Reuse existing badge components for roster metadata.
* Use clear tables, lists and panels for scanning team composition.
* Keep readability and repeated use more important than decoration.

AdminLTE may inspire structural patterns such as sidebar navigation, cards, tables, forms and widgets. It must not be added as a direct dependency.

Avoid:

* Generic admin-template styling.
* Overloaded fantasy decoration.
* A new design system.
* UI library adoption without a separate ADR.
* Styling concerns leaking into Domain or Application.

## Architecture Rules

The existing architecture rules remain in force.

* Domain must not reference Web, Infrastructure, Blazor or styling concepts.
* Application must not reference Web or Infrastructure.
* Application must not contain CSS classes, colors, component names or Blazor types.
* Infrastructure remains persistence-focused.
* Web uses Application handlers, services or queries.
* Blazor components must not use `DbContext` directly.
* Blazor components must not inject repository implementations directly.
* Existing `Result`, `Result<T>`, `Failure` and `FailureType` patterns should be reused.
* Expected failures such as `NotFound`, `Conflict`, `Validation` and `BusinessRule` should be displayed clearly.
* Existing transaction patterns should not be redesigned.
* No external API DTOs should be introduced.

## Application Guidance

Prefer existing Application handlers and queries where they provide enough data for the UI.

Available players should be derived from the selected guild context.

Players without a main character should remain visible, but should be clearly marked as not assignable when current business rules require a main character.

The UI must not bypass existing Application business rules for adding raid team members.

Likely useful existing operations include:

* `ListRaidTeamsForGuild`
* `GetRaidTeam`
* `AddPlayerToRaidTeam`
* `RemovePlayerFromRaidTeam`
* Existing roster, player and guild queries.

If the Web layer would otherwise need too much mapping or coordination logic, add a focused UI-agnostic Application query.

Possible query and DTO names:

```text
GetRaidTeamManagementOverviewQuery
RaidTeamManagementOverviewDto
RaidTeamManagementTeamDto
RaidTeamManagementMemberDto
AvailableRaidTeamPlayerDto
```

These DTOs may be view-near, but must remain UI-agnostic:

* No CSS classes.
* No colors.
* No Blazor types.
* No component names.
* No external API DTOs.

Empty raid team lists should be represented as empty collections, not as failures.

## Suggested PR Slices

### 0009a: Raid Team Overview and Detail Layout

* Add route for raid team management.
* Add raid team overview.
* Add team detail panel or detail page.
* Add summary cards.
* Add empty and loading states.
* Reuse existing Guildwise Web components.

### 0009b: Raid Team Member Assignment UX

* Add players to raid teams.
* Remove players from raid teams.
* Display expected Application result failures.
* Display available players.
* Mark players without main characters clearly.

### 0009c: Team Composition and Polish

* Display role composition.
* Display class and role overview.
* Add small composition hints.
* Add small search or filters for available players.
* Polish spacing, states and scanning behavior.

## Acceptance Criteria

This feature is accepted when:

* Raid team management page exists.
* Existing raid teams are displayed.
* Raid team details are visible.
* Team members are visible.
* Role composition is visible.
* Players can be added to a raid team.
* Players can be removed from a raid team.
* Expected failures are displayed in user-understandable language.
* Empty states are clear.
* Existing roster overview remains available.
* Setup or verification UI remains available while it is still needed for local data maintenance.
* No external APIs are introduced.
* No new UI library is introduced.
* No new persistence foundation is introduced.
* Architecture tests pass.
* `dotnet build Guildwise.sln` passes.
* `dotnet test Guildwise.sln` passes.
* `CHANGELOG.md` is updated during implementation.

## Manual Verification

Manual verification should cover:

1. Start the local database.
2. Apply migrations if needed.
3. Start the Web app.
4. Create a sample roster or use existing local data.
5. Open raid team management.
6. Verify that raid teams are visible.
7. Verify that team members are visible.
8. Verify that available players are visible.
9. Add a player to a raid team.
10. Remove a player from a raid team.
11. Verify empty states.
12. Verify expected failure feedback.
13. Verify that the roster overview still works.
14. Run `dotnet build Guildwise.sln`.
15. Run `dotnet test Guildwise.sln`.

## Risks and Open Questions

Known risks:

* Existing Application handlers may not provide the best view shape.
* The UI could accumulate too much mapping logic.
* Slice 0009a should decide the UX structure for separate detail route versus in-page detail panel.
* Add and remove flows need clear `Result` feedback.
* Players without main characters need clear but non-error presentation.
* Scope could drift into raid event planning.
* Automatic team optimization must stay out of this feature.
* Authentication and permission concepts must not be pulled forward.

Open questions:

* Should raid team detail use a separate route or an in-page detail panel?
* Should available players come from the roster overview query or a focused raid team management query?
* How much composition hinting belongs in 0009c before it becomes recommendation logic?

## Done Definition

The feature is done when:

* Raid team management is productively usable.
* Raid teams and members are visible.
* Add and remove member UX works.
* Team composition is visible.
* Empty, error and loading states are handled.
* The UI builds on the 0007 and 0008 foundations.
* Architecture boundaries are preserved.
* No external integrations are introduced.
* Build and tests are green.
* `CHANGELOG.md` is updated.
