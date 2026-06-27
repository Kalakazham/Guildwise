# Feature 0001: Manual Guild Roster Setup

## Tracking

GitHub Issue: #1
Branch: `feature/0001-manual-guild-roster-setup`
Milestone: `v0.1.0`

## Goal

As a guild or raid organizer, I want to manually create and manage the basic roster structure for a guild, so that Guildwise can model guilds, players, characters and raid teams before external API integrations exist.

This feature establishes the domain and application foundation for future roster dashboards.

It does not aim to build the final polished dashboard UI yet. The goal is to create, update, delete and connect the core roster concepts correctly.

## User Value

After this feature, Guildwise should be able to represent and manage these facts:

* A guild exists.
* Players exist.
* Characters exist.
* Players can own multiple characters.
* One character can be selected as the player's main character.
* Raid teams exist.
* Raid teams belong to a guild.
* Players can be added to a raid team if they have a main character.
* Basic CRUD operations exist for guilds, players, characters and raid teams.

This gives Guildwise the basic model needed for later dashboard, preparation, attendance, loot, recruiting and external integration features.

## In Scope

This feature includes:

* Create, read, update and delete guilds.
* Create, read, update and delete players.
* Create, read, update and delete characters.
* Create, read, update and delete raid teams.
* Add raid teams to a guild.
* Assign characters to players.
* Select one main character for a player.
* Change a player's main character.
* Add players to a raid team if they have a main character.
* Remove players from a raid team.
* Prevent duplicate players in the same raid team.
* Model guild membership with base ranks.
* Model additional guild roles.
* Add `RaidLead` as an additional guild role.
* Add `Recruiter` as an additional guild role.
* Add domain unit tests for the core rules.
* Add application tests where useful.
* Keep architecture tests passing.

## Out of Scope

This feature does not include:

* Final polished roster dashboard UI.
* Advanced dashboard summaries.
* Authentication.
* User accounts.
* Authorization middleware.
* Login.
* Database persistence.
* EF Core.
* PostgreSQL.
* Raider.IO integration.
* Blizzard API integration.
* Warcraft Logs integration.
* Discord bot.
* Attendance tracking.
* Loot wishlist.
* Raid events.
* Preparation checks.
* Recruiting workflow.
* Applicant tracking.
* Advanced permission management.
* Multi-guild account management.
* Production-ready UI polish.

## Domain Concepts

### Guild

A guild represents an organized in-game guild.

Initial properties:

* Id
* Name
* Region
* Realm
* RaidTeams
* Members

Initial behavior:

* A guild can be created with a name, region and realm.
* A guild can be renamed.
* A guild's region and realm can be updated if needed.
* A guild can contain raid teams.
* A guild can contain guild members.
* A guild can be deleted.
* A guild should reject empty name, region or realm.
* A guild should not contain duplicate raid team names.
* A guild should not contain the same player twice as a guild member.

### Player

A player represents a real person or user identity within Guildwise.

A player is not the same as a World of Warcraft character.

Initial properties:

* Id
* DisplayName
* Characters
* MainCharacterId

Initial behavior:

* A player can be created with a display name.
* A player can be renamed.
* A player can be deleted.
* A player can own multiple characters.
* A player can select one of their characters as main character.
* A player can change their main character.
* A player cannot select a character as main if that character does not belong to them.
* A player without a main character cannot be added to a raid team.

### Character

A character represents a playable World of Warcraft character.

Initial properties:

* Id
* PlayerId
* Name
* Region
* Realm
* CharacterClass
* Specialization
* Role

Initial behavior:

* A character can be created with name, region, realm, class, specialization and role.
* A character can be updated.
* A character can be deleted.
* A character must have a name, region and realm.
* A character belongs to exactly one player.
* Character identity is based on normalized region, realm and character name.
* A player should not have duplicate characters with the same region, realm and name.
* If a player's main character is deleted, the player must no longer have that character as main.

### Guild Member

A guild member connects a player to a guild.

Initial properties:

* Id
* GuildId
* PlayerId
* Rank
* AdditionalRoles

Initial behavior:

* A player can be a member of a guild.
* Guild membership has a base rank.
* Additional roles can extend the member's permissions later.
* Additional roles are additive.
* A guild member can be assigned additional roles.
* A guild member can have additional roles removed.
* For this feature, guild membership is modeled but no real authentication is implemented.

### Guild Rank

A guild rank describes the member's base rank within a guild.

Initial ranks:

* Member
* Officer
* GuildLead

Initial meaning:

* Member can view roster information later.
* Officer can manage roster data later.
* GuildLead can manage all guild-related data later.

This feature only models the rank. It does not implement full authorization middleware.

### Additional Guild Role

An additional guild role grants extra permissions on top of the base guild rank.

Initial additional roles:

* RaidLead
* Recruiter

Additional roles are not base ranks. They are additive roles.

This means a guild member can be:

* Member
* Member + Recruiter
* Officer
* Officer + RaidLead
* Officer + Recruiter
* Officer + RaidLead + Recruiter
* GuildLead
* GuildLead + RaidLead
* GuildLead + Recruiter
* GuildLead + RaidLead + Recruiter

For this feature, additional roles are represented in the domain model, but role-specific behavior is mostly out of scope.

Future features may add RaidLead-specific permissions such as:

* Managing raid events.
* Managing attendance.
* Managing raid preparation checks.
* Managing raid notes.
* Assigning raid strategy responsibilities.

Future features may add Recruiter-specific permissions such as:

* Managing applicants.
* Tracking recruitment needs.
* Managing trial players.
* Creating recruitment notes.
* Reviewing external character profiles.

### Raid Team

A raid team represents a raid group within a guild.

Initial properties:

* Id
* GuildId
* Name
* Members

Initial behavior:

* A raid team can be created with a name.
* A raid team can be renamed.
* A raid team can be deleted.
* A raid team belongs to one guild.
* A raid team can contain players.
* A player can be added to a raid team only if they have a main character.
* A player can be removed from a raid team.
* A player cannot be added twice to the same raid team.
* A raid team name must not be empty.

### Raid Team Member

A raid team member connects a player to a raid team.

Initial properties:

* Id
* RaidTeamId
* PlayerId

Initial behavior:

* A raid team member represents one player in one raid team.
* The player's main character is used as the primary roster representation.
* The raid team does not directly contain characters.
* Removing a raid team member removes the player from that raid team, but does not delete the player.

## Core Domain Rules

### Player and Character Rules

* A player can have multiple characters.
* A character belongs to exactly one player.
* A player can have exactly one main character.
* The main character must belong to the player.
* A player can change their main character.
* A player without a main character cannot be added to a raid team.
* A player cannot have duplicate characters with the same normalized region, realm and character name.
* Deleting a character must not leave an invalid main character reference behind.

### Guild and Raid Team Rules

* A guild can have multiple raid teams.
* A raid team belongs to exactly one guild.
* Raid team names must be unique within the same guild.
* A raid team contains players, not characters.
* A player can only be added once to the same raid team.
* A player must have a main character before being added to a raid team.
* Removing a player from a raid team does not delete the player.
* Deleting a raid team removes the raid team membership links for that raid team.
* When a player is shown in a roster, their main character represents them.

### Guild Membership and Role Rules

* Guild membership has a base rank.
* Guild membership may have additional roles.
* Additional roles are additive.
* `RaidLead` is modeled as an additional role, not as a base rank.
* `Recruiter` is modeled as an additional role, not as a base rank.
* This feature does not implement full authentication or authorization.
* Future application use cases should use the rank and additional roles to decide what actions are allowed.

## CRUD Requirements

### Guild CRUD

The system should support:

* Create guild.
* Read guild by id.
* List guilds.
* Update guild name, region and realm.
* Delete guild.

Validation rules:

* Name is required.
* Region is required.
* Realm is required.

### Player CRUD

The system should support:

* Create player.
* Read player by id.
* List players.
* Update player display name.
* Delete player.

Validation rules:

* Display name is required.

Behavior rules:

* Deleting a player should remove their raid team memberships.
* Deleting a player should remove or detach their characters according to the chosen implementation.
* For the MVP, deleting a player may delete the player's characters as well.

### Character CRUD

The system should support:

* Create character.
* Read character by id.
* List characters.
* List characters for a player.
* Update character name, region, realm, class, specialization and role.
* Delete character.

Validation rules:

* Name is required.
* Region is required.
* Realm is required.
* Character class is required.
* Specialization is required.
* Role is required.

Behavior rules:

* A character must belong to a player.
* A player cannot have duplicate characters with the same normalized region, realm and name.
* If a character is the player's main character and gets deleted, the player's main character must be cleared or changed.

### Raid Team CRUD

The system should support:

* Create raid team.
* Read raid team by id.
* List raid teams for a guild.
* Update raid team name.
* Delete raid team.

Validation rules:

* Name is required.
* A raid team must belong to a guild.
* Raid team names must be unique within the same guild.

Behavior rules:

* Deleting a raid team removes its raid team memberships.
* Deleting a raid team does not delete players.
* Deleting a raid team does not delete characters.

### Relationship Operations

The system should support:

* Add raid team to guild.
* Add player to guild.
* Assign additional guild roles to guild member.
* Remove additional guild roles from guild member.
* Add character to player.
* Set player's main character.
* Add player to raid team.
* Remove player from raid team.

Validation rules:

* A character can only be assigned to one player.
* A player can only set one of their own characters as main.
* A player must have a main character before being added to a raid team.
* A player cannot be added twice to the same raid team.

## Suggested Implementation Slices

This feature should be implemented in small slices.

### 0001a: Domain Model and Unit Tests

Implement the core domain model:

* Guild
* Player
* Character
* GuildMember
* RaidTeam
* RaidTeamMember
* GuildRank
* AdditionalGuildRole
* CharacterClass
* CharacterRole
* CharacterSpecialization

Add unit tests for:

* Creating a valid guild.
* Rejecting a guild with empty name, region or realm.
* Creating a valid player.
* Rejecting a player with an empty display name.
* Creating a valid character.
* Assigning a character to a player.
* Rejecting duplicate characters for the same player.
* Setting a player's main character.
* Rejecting a main character that does not belong to the player.
* Creating a raid team.
* Adding a raid team to a guild.
* Rejecting duplicate raid team names within the same guild.
* Adding a player with a main character to a raid team.
* Rejecting a player without a main character when adding them to a raid team.
* Rejecting duplicate players in the same raid team.
* Adding additional roles to a guild member.
* Removing additional roles from a guild member.
* Preventing duplicate additional roles.

No web UI, database, EF Core or external API work should be done in this slice.

Suggested commit:

```text
feat: add manual guild roster domain model
```

### 0001b: Application Use Cases

Implement application use cases for the core flows:

* CreateGuild
* GetGuild
* ListGuilds
* UpdateGuild
* DeleteGuild
* CreatePlayer
* GetPlayer
* ListPlayers
* UpdatePlayer
* DeletePlayer
* CreateCharacter
* GetCharacter
* ListCharacters
* ListCharactersForPlayer
* UpdateCharacter
* DeleteCharacter
* CreateRaidTeam
* GetRaidTeam
* ListRaidTeamsForGuild
* UpdateRaidTeam
* DeleteRaidTeam
* AddPlayerToGuild
* AddAdditionalRoleToGuildMember
* RemoveAdditionalRoleFromGuildMember
* AddCharacterToPlayer
* SetMainCharacter
* AddPlayerToRaidTeam
* RemovePlayerFromRaidTeam

Application code should orchestrate domain behavior.

Application code should not contain infrastructure details.

Suggested commit:

```text
feat: add manual guild roster use cases
```

### 0001c: Temporary In-Memory Storage

Implement temporary storage so future web UI work can use real application flows without production database persistence.

Possible implementation:

* Define repository interfaces in Application.
* Implement in-memory repositories in Infrastructure.
* Register them through dependency injection.

This is not the final persistence solution.

Suggested commit:

```text
feat: add in-memory guild roster storage
```

### 0001d: Minimal Verification UI or Debug View

Optionally add a minimal Blazor page or developer-facing view that proves the application flow works.

This is not the final dashboard.

The view may allow:

* Creating or seeding a guild.
* Creating players.
* Creating characters.
* Selecting a main character.
* Creating a raid team.
* Adding a player to the raid team.
* Viewing the resulting simple roster list.

Visual polish is out of scope.

Suggested commit:

```text
feat: add minimal roster setup view
```

## Acceptance Criteria

### Guilds

* A guild can be created with name, region and realm.
* A guild can be read by id.
* Guilds can be listed.
* A guild can be updated.
* A guild can be deleted.
* A guild rejects empty name, region or realm.
* A guild can have raid teams.
* A guild rejects duplicate raid team names.

### Players

* A player can be created with a display name.
* A player can be read by id.
* Players can be listed.
* A player can be updated.
* A player can be deleted.
* A player rejects an empty display name.
* A player can have multiple characters.
* A player can select one of their characters as main character.
* A player cannot select a character as main if the character does not belong to them.

### Characters

* A character can be created with name, region, realm, class, specialization and role.
* A character can be read by id.
* Characters can be listed.
* Characters for a player can be listed.
* A character can be updated.
* A character can be deleted.
* A character belongs to one player.
* A player cannot have duplicate characters with the same region, realm and name.
* Deleting a main character does not leave an invalid main character reference behind.

### Raid Teams

* A raid team can be created with a name.
* A raid team can be read by id.
* Raid teams for a guild can be listed.
* A raid team can be updated.
* A raid team can be deleted.
* A raid team belongs to a guild.
* A raid team can contain players.
* A player can be added to a raid team only if they have a main character.
* A player can be removed from a raid team.
* A player cannot be added twice to the same raid team.
* Raid teams do not directly contain characters.

### Guild Membership and Roles

* A guild member connects a player to a guild.
* A guild member has a base rank.
* A guild member may have additional roles.
* `RaidLead` is modeled as an additional role.
* `Recruiter` is modeled as an additional role.
* Additional roles are additive.
* Duplicate additional roles are rejected.
* Full authentication and authorization are not implemented in this feature.

### Architecture

* Domain logic stays in `Guildwise.Domain`.
* Application orchestration stays in `Guildwise.Application`.
* Temporary infrastructure stays in `Guildwise.Infrastructure`.
* Web does not use Infrastructure directly except through composition.
* External API integrations are not added.
* EF Core is not added.
* Architecture tests pass.

### Tests

* Unit tests cover the domain rules.
* Application tests are added where useful.
* Architecture tests pass.
* `dotnet build` succeeds.
* `dotnet test` succeeds.

## Technical Notes

* Do not add EF Core in this feature.
* Do not add PostgreSQL in this feature.
* Do not add Raider.IO, Blizzard or Warcraft Logs integrations.
* Do not add authentication.
* Do not create microservices.
* Keep the implementation simple.
* Prefer explicit domain methods over public mutable collections.
* Keep Domain free from ASP.NET Core, EF Core and external DTOs.
* Use clear domain language.
* Do not model raid teams as collections of characters.
* Model raid teams as collections of players.
* Treat `RaidLead` and `Recruiter` as additional roles.
* Do not model `RaidLead` or `Recruiter` as base ranks.

## Done Definition

This feature is done when:

* The domain model for manual guild roster setup exists.
* Players can own characters.
* Players can have a main character.
* Guilds can have raid teams.
* Raid teams can contain players.
* Players without a main character cannot be added to raid teams.
* Duplicate players in the same raid team are rejected.
* CRUD use cases exist for guilds, players, characters and raid teams.
* Additional roles include `RaidLead` and `Recruiter`.
* Duplicate additional roles are rejected.
* Domain behavior is covered by unit tests.
* Application use cases exist for the main setup flows.
* Temporary storage exists if required for the application flow.
* `dotnet build` succeeds.
* `dotnet test` succeeds.
* Architecture tests pass.
* `CHANGELOG.md` has an entry under `[Unreleased]`.
* No external API integration has been introduced.
