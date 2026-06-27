# ADR 0004: Versioning Strategy

## Status

Accepted

## Context

Guildwise needs a simple and reliable versioning strategy.

The project uses a lightweight branch model:

- `main` for stable states
- `dev` for integration work
- `feature/*` for individual features

The project should be able to:

- Show what changed between versions.
- Calculate versions automatically.
- Avoid manually editing project files for every version change.
- Support future CI/CD, GitHub Releases and deployment automation.

## Decision

Guildwise will use:

- Semantic Versioning
- Git tags
- GitVersion
- A manually maintained `CHANGELOG.md`

Version numbers are calculated from Git history, branch names and tags.

The repository contains these versioning-related files:

```text
GitVersion.yml
dotnet-tools.json
CHANGELOG.md
```

`GitVersion.yml` defines how branches are interpreted.

`dotnet-tools.json` pins the GitVersion tool version used by the repository.

`CHANGELOG.md` documents notable changes in a human-readable form.

## Version Format

Guildwise uses Semantic Versioning:

```text
MAJOR.MINOR.PATCH
```

Examples:

```text
0.1.0
0.2.0
0.2.1
1.0.0
```

Before `1.0.0`, minor versions may represent meaningful feature milestones.

Examples:

```text
0.1.0 - first manual roster dashboard
0.2.0 - persistence added
0.3.0 - first external integration
1.0.0 - first stable public version
```

## Branch Behavior

### `main`

`main` contains stable release history.

Real releases are created by tagging `main`.

Example tags:

```text
v0.1.0
v0.2.0
v0.2.1
v1.0.0
```

### `dev`

`dev` is the integration branch.

It produces pre-release versions such as:

```text
0.1.0-dev.3
0.1.0-dev.4
0.2.0-dev.1
```

### `feature/*`

Feature branches are created from `dev`.

Example:

```text
feature/0001-manual-roster-dashboard
```

Feature branches produce temporary pre-release versions and are merged back into `dev` when complete.

## Release Process

A release is created from `main`.

Typical release flow:

```text
feature/* -> dev -> main -> version tag
```

Example:

```bash
git checkout dev
git merge feature/0001-manual-roster-dashboard

git checkout main
git merge dev

git tag v0.1.0
git push origin main
git push origin v0.1.0
```

The tag marks the release point.

GitVersion uses the tag and Git history to calculate future versions.

## Changelog Rules

`CHANGELOG.md` contains human-readable release notes.

The `[Unreleased]` section collects changes that are not released yet.

When a release is created, entries from `[Unreleased]` are moved into a versioned section.

Example:

```markdown
## [Unreleased]

### Added

### Changed

### Fixed

## [0.1.0] - 2026-06-27

### Added

- Manual guild and raid roster dashboard.
```

The changelog should focus on notable changes, not every commit.

Good changelog entries:

- Added manual roster dashboard.
- Added architecture boundary tests.
- Added GitVersion-based versioning.
- Fixed duplicate roster member handling.

Bad changelog entries:

- Renamed local variable.
- Reformatted file.
- Added empty class.
- Changed whitespace.

## AI Agent Rules

AI agents must not manually increment project versions.

AI agents must not edit `.csproj` files only to change version numbers unless explicitly instructed.

AI agents may update `CHANGELOG.md` when implementing:

- A notable feature
- A user-facing fix
- An architectural change
- A tooling or build-system change

AI agents must not create Git tags unless explicitly instructed by the user.

Useful command:

```bash
dotnet gitversion /showvariable FullSemVer
```

## Consequences

### Positive

- Version numbers are derived consistently from Git history.
- Releases can be identified by Git tags.
- Development builds get meaningful pre-release versions.
- The changelog remains human-readable.
- AI agents have clear versioning rules.
- CI/CD can use the same versioning strategy later.

### Negative

- Version calculation depends on correct branch and tag usage.
- The changelog still needs manual or AI-assisted maintenance.
- CI/CD integration will be needed later to use GitVersion automatically in builds and releases.
- The calculated GitVersion version is not automatically written into assemblies yet.

## Current Scope

For now, GitVersion is used as a local tool.

The current version can be checked with:

```bash
dotnet gitversion /showvariable FullSemVer
```

Assembly version stamping, Docker image tagging and GitHub Release automation are out of scope for now.

## Follow-Up Actions

- Add CI later to run GitVersion, build and tests automatically.
- Use GitHub Releases when the first meaningful version is ready.
- Consider assembly version integration later.
- Consider Docker image version tagging later.
- Keep `CHANGELOG.md` updated during feature work.
