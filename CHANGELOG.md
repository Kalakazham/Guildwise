# Changelog

All notable changes to Guildwise will be documented in this file.

The format is based on Keep a Changelog, and this project uses Semantic Versioning.

## [Unreleased]

### Added

- Added the raid event planning foundation with Domain, Application and persistence support.
- Added the raid event overview and detail UI.
- Added raid event creation UX on the raid events page.
- Added raid event lifecycle foundation with scheduled/cancelled status and update/cancel use cases.
- Added development-only startup migration application for local Postgres-backed Web runs.
- Added raid event edit UX for scheduled events.
- Added raid event cancellation UX with inline confirmation.
- Added raid event signup foundation with Domain, Application and persistence support.
- Added raid event signup overview UI with summary, role composition and signup list display.
- Added raid event signup management UX for setting Signed, Tentative and Declined statuses.
- Added code quality guardrails for file-size targets, known-debt baselines and CI checks.

### Changed

### Fixed

## [0.4.0] - 2026-07-11

### Added

- ADR for the Guildwise frontend UI stack and Web styling boundaries.
- Documented external integration boundaries for future WoWAudit, Blizzard, Raider.IO and Warcraft Logs integrations.
- Raid team composition polish with class overview, neutral hints and available-player controls.
- Raid team member assignment UX for adding and removing players from raid teams.
- Raid team management overview and in-page detail layout.
- Roster overview UI shell with reusable Web layout components and a focused Application query.
- WoW-inspired roster badges and visual identity for class, role, guild and raid team metadata.
- Client-side roster search, filter, sort controls and no-match empty state.

### Changed

### Fixed

## [0.3.0] - 2026-06-28

### Added

- Application result primitives for expected use-case outcomes.
- Tests for expected Application result failure outcomes.
- Architecture documentation for Application result handling and transaction boundaries.
- Developer setup documentation for local PostgreSQL, migrations, Web startup and tests.
- GitHub Actions CI for restore, build and tests.
- Persistence transaction runner abstraction with EF and in-memory implementations.
- Repository line-ending normalization rules.

### Changed

- Improved Web feedback for expected Application result failures.
- Made non-development persistence configuration fail fast for missing or unsafe settings.
- Made player deletion persistence atomic across guild and player changes.
- Made player main-character persistence transactional during insert.
- Refactored core roster command handlers to return structured Application results for expected outcomes.
- Refactored remaining roster command handlers to return structured Application results.
- Refactored persistence-facing repositories and use case handlers to async APIs.

### Fixed

## [0.2.0] - 2026-06-28

### Added

- Initial modular monolith solution structure.
- Project guardrails for AI-assisted development.
- Architecture documentation and ADRs.
- Unit, integration and architecture test projects.
- GitVersion setup for automated version calculation.
- Manual guild roster domain model and unit tests for the first feature slice.
- Application use case handlers for manual guild roster setup.
- Temporary in-memory storage for guild and player aggregate roots.
- Minimal Blazor verification UI for manual guild roster setup.
- EF Core PostgreSQL persistence foundation.
- Initial EF Core mappings and migration for guild roster persistence.
- EF-backed repositories for persistent guild roster storage.
- Testcontainers-based PostgreSQL persistence integration tests.
- Wired the Web app to use configurable PostgreSQL persistence in development.

### Changed

- Refactored the Application layer from a single roster application service into explicit use case handlers.

### Fixed
