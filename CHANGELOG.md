# Changelog

All notable changes to Guildwise will be documented in this file.

The format is based on Keep a Changelog, and this project uses Semantic Versioning.

## [Unreleased]

### Added

- Developer setup documentation for local PostgreSQL, migrations, Web startup and tests.

### Changed

- Made non-development persistence configuration fail fast for missing or unsafe settings.

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
