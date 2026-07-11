# ADR 0009: External Integration Boundaries and Source Ownership

## Status

Accepted

## Context

Guildwise is currently manual-first.

The current product surface supports manual guild, player, character, main character, guild member, raid team, raid team membership, roster overview and raid team management workflows.

External integrations are a planned part of the product direction, but they should not block early manual workflows.

Likely future providers include:

- WoWAudit
- Blizzard WoW API
- Raider.IO
- Warcraft Logs
- Other relevant WoW, guild or raid APIs

Future features must keep integration paths open when the same data can later come from external sources.

## Decision

Guildwise remains manual-first for now.

External integrations are a fixed part of the roadmap.

Planned integrations include at least WoWAudit, Blizzard WoW API, Raider.IO, Warcraft Logs and possibly other relevant WoW, guild or raid APIs.

Domain must not know about external API DTOs.

Application defines ports and interfaces for external data sources when integrations are implemented.

Infrastructure implements concrete API clients, authentication, HTTP behavior, rate-limit handling and technical mapping details.

Persisted imported data must receive source, external id and sync metadata when the feature stores external data.

Manual and imported data must remain distinguishable.

Features such as roster, raid events, signups, attendance and performance must not be modeled as permanently manual-only when external sources are expected for those areas.

## Rationale

Guildwise should be useful early, so manual workflows are built first.

External APIs can add value later without blocking early product development.

Blizzard WoW API can later provide guild, roster, character profile and realm or region data.

WoWAudit can later provide raid events, signups, raid schedules and possibly guild or roster context.

Raider.IO and Warcraft Logs can later provide character, progression and performance context.

API DTOs and provider-specific concepts must not shape the Domain model.

Import and sync behavior must stay controlled and traceable.

Architecture boundaries reduce vendor lock-in and protect the data model from provider-specific leakage.

## Rules

- External API clients live in Infrastructure.
- External API DTOs live in Infrastructure or integration-specific infrastructure namespaces or projects.
- Domain must not reference external API DTOs.
- Application must not depend on concrete API clients.
- Application may define integration ports or interfaces.
- Application DTOs must remain provider-neutral unless explicitly modeling a source-aware import use case.
- Web must not call external APIs directly.
- Web uses Application use cases and queries.
- Imported persisted entities or records should include source metadata when relevant:
  - `Source`
  - `ExternalId`
  - `LastSyncedAt`
  - optional `SyncStatus`
  - optional `SourceVersion` or equivalent if later needed
- Manual records and imported records must be distinguishable where conflicts or resync behavior matter.
- Feature designs for roster, raid events, signups, attendance and performance must include future external source considerations.
- Do not add real API clients, credentials, background jobs or sync engines without a dedicated feature and ADR or feature documentation.
- Do not leak provider-specific fields into Domain unless they are explicitly modeled as source metadata.

## Source Ownership Guidance

Different data areas may have different future sources.

Guild and roster data:

- Manual first.
- Later possibly Blizzard WoW API and/or WoWAudit.

Characters:

- Manual first.
- Later Blizzard WoW API, Raider.IO and Warcraft Logs context.

Raid events and schedules:

- Manual first if needed.
- Later WoWAudit source expected.

Signups:

- Likely WoWAudit source later.

Attendance:

- May be manual, signup-based, WoWAudit-based or inferred from Warcraft Logs later.

Performance and encounter data:

- Likely Warcraft Logs source later.

Mythic+ and progression context:

- Likely Raider.IO source later.

This guidance is provider-aware but not implementation-specific.

## Consequences

### Positive

- Manual-first features can ship without waiting for external APIs.
- Future integrations can be added incrementally.
- Provider-specific behavior stays outside Domain.
- Imported and manual data can be audited and reconciled more deliberately.

### Negative

- Feature documentation and modeling require more care.
- Early manual features must avoid permanent manual-only assumptions.
- Not every object needs source metadata immediately.
- Source metadata should be introduced where external persistence or sync is actually implemented.
- Conflict resolution between manual and imported data must be decided explicitly later.

## Not Decided

- Concrete API client implementations.
- Auth, API key or OAuth strategies.
- Background jobs or schedulers.
- Sync frequencies.
- Rate-limit strategies.
- Conflict resolution UI.
- Source precedence, for example whether WoWAudit or Guildwise manual data wins.
- Concrete persistence fields for every future object.
- Whether integrations will later be extracted into separate projects.
