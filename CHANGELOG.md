# Changelog

All notable changes to Guildwise will be documented in this file.

The format is based on Keep a Changelog, and this project uses Semantic Versioning.

## [Unreleased]

### Added

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
