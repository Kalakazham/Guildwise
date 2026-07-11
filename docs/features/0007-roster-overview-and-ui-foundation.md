# Feature 0007: Roster Overview and UI Foundation

## Tracking

GitHub Issue: TBD
Branch: `feature/0007-roster-overview-and-ui-foundation`
Milestone: `v0.4.0`

## Goal

Make the existing guild roster foundation more visible, usable and product-like through a modern roster overview and reusable Web UI building blocks.

Guildwise already models guilds, players, characters and raid teams. This feature turns that foundation into a clearer roster experience without adding a new persistence foundation or external integrations.

The UI direction should be dark, responsive and World of Warcraft-inspired, while still feeling like Guildwise rather than a generic admin template.

## User Value

After this feature, a guild or raid organizer should be able to:

* Open a roster overview page.
* See existing guild, player, character and raid team data in a structured way.
* Quickly understand roster composition by class, role, status, main/alt state and item level where data exists.
* Scan the roster through summary cards, tables or responsive lists.
* Use a more polished Guildwise interface as the foundation for future roster workflows.

This feature improves visibility and usability of existing data. It does not introduce external data sources.

## Technical Value

This feature establishes:

* A reusable app/page layout foundation for Guildwise Web.
* A stronger visual identity for roster management.
* Reusable UI components for headers, panels, cards, badges and roster display.
* A responsive structure that can be improved for mobile in later features.
* A product-facing roster page that still respects the existing Clean Architecture boundaries.

## In Scope

This feature includes:

* Add a roster overview page.
* Add or refine an app/page layout foundation.
* Add a reusable page header pattern.
* Add reusable content panels or cards.
* Add or refine sidebar/navigation foundation only where it naturally fits the current Web layout.
* Add roster summary cards.
* Add a roster table or responsive roster list.
* Display class, role and main/alt badges.
* Display status and item-level badges only where the current model or DTOs provide those data.
* Add a dark, Guildwise/WoW-inspired visual direction.
* Add responsive structure so the layout does not immediately break on smaller viewports.
* Add loading, empty and expected failure states where appropriate.
* Use existing Application queries or add view-near Application DTOs/queries when useful for roster display.
* Update `CHANGELOG.md` under `[Unreleased]` when implementing the feature.

## Search, Filter and Sort

Search, filter and sort are part of the intended roster overview experience.

They should be implemented as the final focused slice `0007c` within this feature. They are not required for the first PR if the initial UI shell and roster layout are delivered separately.

Initial useful controls:

* Search by player or character name.
* Filter by raid team.
* Filter by character class.
* Filter by role.
* Filter by roster status if status exists.
* Sort by character name, player name, role, class or item level where data exists.

Do not introduce advanced analytics or external profile lookup for these controls.

## Out of Scope

This feature does not include:

* New Domain aggregates, except minimal changes that are clearly required for the roster display.
* New persistence foundation.
* EF Core redesign.
* New migrations unless a minimal display field is explicitly added.
* Blizzard API integration.
* Raider.IO integration.
* Warcraft Logs integration.
* External item-level synchronization.
* Raid event planning.
* Attendance.
* Recruitment board.
* Applicant tracking.
* Authentication.
* Permission implementation.
* Billing.
* Discord bot.
* AI recommendations.
* Full design system.
* Theme editor.
* Mobile-perfect layout.
* Advanced analytics.

Responsive behavior should be prepared, but a fully polished mobile layout is out of scope.

## Product Direction

The roster overview should feel like a Guildwise product screen.

Preferred direction:

* Dark interface.
* High-contrast roster information.
* Subtle WoW-inspired class and role treatment.
* Clear visual distinction between tanks, healers and damage dealers.
* Class colors or class-tinted badges where appropriate.
* Compact but readable roster density.
* Page structure that supports repeated roster management work.

Avoid:

* Generic Bootstrap/admin-dashboard styling.
* Marketing-style hero sections.
* Decorative UI that reduces roster readability.
* Overbuilt theme systems.
* UI styling concepts leaking into Domain or Application.

## Roster Overview Page

Add a product-facing roster page in `Guildwise.Web`.

The page should show:

* Current guild context if available.
* Raid team context if selected.
* Roster summary cards.
* Main roster table or responsive list.
* Clear loading state.
* Clear empty state when no roster data exists.
* Clear expected failure state if Application results or queries cannot provide data.

The page may replace or sit alongside the existing verification UI depending on the current Web structure.

The existing setup functionality should not be removed unless the implementation provides an equivalent way to verify roster data.

## Layout Foundation

Add or refine reusable layout pieces in Web.

Possible components:

* App shell.
* Page header.
* Page actions area.
* Content panel.
* Summary card.
* Navigation/sidebar item, where it fits the current layout.
* Roster badge.
* Empty state.
* Loading state.

Component names should use clear Guildwise language and should not be overabstracted.

Good examples:

```text
RosterOverview
RosterSummaryCard
RosterTable
RosterBadge
PageHeader
ContentPanel
```

Avoid vague component names that do not communicate product intent.

## Roster Display

The roster display should prioritize useful scanning.

Expected roster fields, where available:

* Player display name.
* Main character name.
* Character class.
* Specialization.
* Role.
* Guild rank.
* Additional guild roles.
* Raid team membership.
* Main/alt indicator.
* Roster status if the current model supports it.
* Item level if the current model supports it.

If roster status or item level are not yet in the current model, the UI may reserve clear display patterns for them but should not invent external synchronization or speculative domain behavior.

## Badges

Add visual badge patterns for roster metadata.

Useful badge types:

* Character class.
* Character role.
* Roster status.
* Main or alt.
* Item level.
* Guild rank.
* Additional guild role.

Badges should be readable in dark UI and should not rely only on color. Text labels or icons should make the meaning clear.

Domain and Application must not contain CSS class names, color tokens or UI styling rules.

## Architecture Rules

The existing architecture rules remain in force.

* Domain must not reference Web, Infrastructure, Blazor or styling concepts.
* Application must not reference Web or Infrastructure.
* Application must not contain CSS class names, layout names or visual theme rules.
* Infrastructure remains persistence-focused.
* Web startup/composition may reference Infrastructure for dependency injection.
* Blazor pages and components must not use Infrastructure services directly.
* Blazor pages and components must not inject `DbContext`.
* Blazor pages and components must not inject repository implementations directly.
* Blazor pages and components should call Application handlers or services.
* Application may add view-near DTOs or queries only where they are useful for roster display.
* Existing `Result`, `Result<T>`, `Failure` and `FailureType` patterns should be reused.
* Existing `ITransactionRunner` patterns should not be redesigned.
* No external API DTOs should leak into Domain.

## Application Query Guidance

If existing queries do not provide the shape needed by the roster overview, add focused Application query DTOs.

Possible query:

```text
GetRosterOverviewQuery
```

Possible DTOs:

```text
RosterOverviewDto
RosterMemberDto
RosterSummaryDto
```

These DTOs may be view-near, but they should remain UI-agnostic:

* Include roster facts.
* Do not include CSS class names.
* Do not include color values.
* Do not include component names.
* Do not depend on Blazor types.

Query handlers should follow the existing Application result conventions for missing data and empty collections. Empty rosters should be represented as empty collections, not as failures.

## Suggested PR Slices

### 0007a: UI Shell and Roster Layout

Implement:

* Roster overview route.
* App/page layout foundation.
* Page header.
* Content panels or cards.
* Main roster table or responsive list shell.
* Loading and empty states.

Suggested commit:

```text
feat: add roster overview layout foundation
```

### 0007b: WoW Badges and Visual Identity

Implement:

* Class badges.
* Role badges.
* Main/alt badges.
* Status and item-level badge patterns where the current model or DTOs provide those data.
* Dark Guildwise visual styling.
* Reusable badge components without overabstracting.

Suggested commit:

```text
feat: add roster visual identity and badges
```

### 0007c: Search, Filter, Sort and Empty States

Implement:

* Search by player or character name.
* Filter by raid team, class and role.
* Sort important roster columns.
* Improved empty states for no data and no filter matches.
* Expected failure display where applicable.

Suggested commit:

```text
feat: add roster search filter and sort controls
```

## Acceptance Criteria

### Roster Overview

* A roster overview page exists.
* The page shows existing guild, player, character and raid team data.
* The roster is easier to scan than the earlier verification UI.
* Existing setup and persistence behavior continues to work.
* No external API dependency is introduced.

### UI Quality

* UI is modern, dark and Guildwise/WoW-appropriate.
* UI does not look like a generic default admin template.
* Roster information remains readable.
* Reusable components are clearly named.
* Components are not overabstracted.
* Page sections, cards and panels are used consistently.
* Class, role and main/alt display patterns are visually clear.
* Status and item-level display patterns are visually clear where the current model or DTOs provide those data.

### Responsive Behavior

* Layout does not immediately break on smaller viewports.
* Roster content can still be accessed on narrow screens.
* Responsive structure is prepared for later mobile polish.
* Mobile-perfect layout is not required.

### States

* Loading state is considered where data loads asynchronously.
* Empty roster state is clear.
* Empty filter result state is clear if filters are implemented.
* Expected failure state is considered where Application results are involved.
* Unexpected technical failures are not hidden as normal UI states.

### Architecture

* Domain remains free of UI concerns.
* Application remains free of Web and Infrastructure dependencies.
* Infrastructure remains free of UI concerns.
* Web components do not inject `DbContext`.
* Web components do not inject repository implementations directly.
* Web uses Application handlers, services or queries.
* Existing Result patterns are reused.
* Existing transaction patterns are not redesigned.
* Architecture tests pass.

### Tests

* Unit tests are added or updated for meaningful Application query behavior.
* Component or integration tests are added if the project already has a suitable pattern.
* If no suitable Web component test pattern exists yet, this feature does not introduce a new test framework only for UI components.
* Existing Domain tests pass.
* Existing Application tests pass.
* Existing Integration tests pass.
* Existing Architecture tests pass.
* `dotnet build Guildwise.sln` succeeds.
* `dotnet test Guildwise.sln` succeeds.

### Changelog

* `CHANGELOG.md` is updated under `[Unreleased]` when implementing the feature.

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

4. Create or use existing roster data.

5. Open the roster overview page.

6. Verify:

   * Guild and raid team context is visible.
   * Players and main characters are visible.
   * Class and role information is clear.
   * Empty and loading states behave reasonably.
   * The layout remains usable on a narrower viewport.

7. Run:

   ```bash
   dotnet build Guildwise.sln
   dotnet test Guildwise.sln
   ```

## Risks and Open Questions

### Current Data Shape

The current model may not contain every visual field the UI wants, such as item level or roster status.

Do not add speculative external integration or broad domain changes only to fill the UI. If a field is missing, either omit it for now or add the smallest explicit product-backed model change.

### Verification UI Transition

The existing UI may still be useful for manually creating test roster data.

If the roster overview replaces it, a practical way to create, seed or verify local roster data must remain available during local development.

### UI Scope Growth

Roster UI work can grow quickly.

Keep the first slice focused on a useful overview, reusable foundations and clear roster scanning. Recruitment, attendance, raid events and analytics belong to later features.

### Component Abstraction

Reusable components are useful, but a full design system is premature.

Create components for repeated real UI patterns. Avoid generic abstractions before the product surface needs them.

## Done Definition

This feature is done when:

* A roster overview page exists.
* Existing guild, player, character and raid team data is visible in a product-facing UI.
* The UI has a dark Guildwise/WoW-inspired direction.
* Reusable Web components exist for the roster page where useful.
* Class, role and main/alt display patterns are clear.
* Status and item-level display patterns are clear where the current model or DTOs provide those data.
* Search, filter and sort are implemented in the focused `0007c` slice.
* Loading, empty and expected failure states are handled where appropriate.
* The layout is prepared for responsive use.
* No external API integration has been introduced.
* No new persistence foundation has been introduced.
* Domain and Application remain free of UI styling details.
* Web startup/composition references Infrastructure only for dependency injection.
* Blazor pages and components do not directly use Infrastructure services, repositories or `DbContext`.
* Architecture tests pass.
* `dotnet build Guildwise.sln` succeeds.
* `dotnet test Guildwise.sln` succeeds.
* `CHANGELOG.md` has an entry under `[Unreleased]` when the feature is implemented.
