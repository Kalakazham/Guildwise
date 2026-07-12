# Code Quality Guardrails

Guildwise uses lightweight, free quality guardrails to keep the modular monolith maintainable as roster, raid team, raid event and signup workflows grow.

These rules are intentionally incremental. Existing large files are treated as known debt, but new files and existing debt should not keep growing unchecked.

## Local Check

Run the quality check from the repository root:

```powershell
pwsh -NoProfile -File tools/check-code-quality.ps1
```

The script scans `.cs` and `.razor` files and reports:

* `Warning`: a file is above the target size but still below the gate.
* `Gate`: a hard violation that should fail local validation and CI.
* `Known debt`: an existing large file tracked in `tools/code-quality-baseline.json`.

## File Size Rules

| File kind | Target | Gate |
| --- | ---: | ---: |
| Razor page under `Components/Pages` | 350 lines | 500 lines |
| Razor shared component under `Components/Shared` or comparable component folders | 150 lines | 250 lines |
| Production `.cs` under `src` | 200 lines | 300 lines |
| Test `.cs` under `tests` | 400 lines | 800 lines |

Target violations are visible warnings. Gate violations fail the script.

Known-debt files are allowed up to their baseline value, but baseline growth is a gate violation.

## Known Debt

The current baseline is stored in `tools/code-quality-baseline.json`.

Known debt includes large files such as:

* `src/Guildwise.Web/Components/Pages/RaidEvents.razor`
* `tests/Guildwise.UnitTests/ApplicationUseCaseTests.cs`
* `tests/Guildwise.UnitTests/DomainModelTests.cs`
* `tests/Guildwise.IntegrationTests/EfApplicationPersistenceTests.cs`
* `src/Guildwise.Web/Components/Pages/RaidTeamManagement.razor`
* `src/Guildwise.Web/Components/Pages/Home.razor`
* `src/Guildwise.Web/Components/Pages/RosterOverview.razor`

Do not raise baseline limits without explicit review. Reducing a baseline after refactoring is encouraged.

`RaidEvents.razor` should be split in later refactoring work. Good extraction candidates are:

* `RaidEventList`
* `RaidEventDetail`
* `RaidEventCreateForm`
* `RaidEventEditForm`
* `RaidEventCancelPanel`
* `RaidEventSignupOverview`
* `RaidEventSignupManagement`

This guardrail slice does not perform that refactor.

## Exclusions

The script ignores:

* `.git`
* `bin`
* `obj`
* EF Core migrations
* EF Core model snapshots
* generated files
* designer files

## Design Targets Not Yet Enforced

These are target rules for reviews and future analyzer work. They are not enforced by `tools/check-code-quality.ps1` yet:

* Methods should usually stay under 60 lines.
* UI event handlers should usually stay under 40 lines.
* Parameter count target is 5 or fewer.
* Methods or constructors with more than 7 parameters should be reviewed.
* DTOs, records and commands are exempt from parameter count pressure when the shape is intentional.
* Cyclomatic complexity target is 10 or lower.
* Cyclomatic complexity above 15 should be reviewed.

## Analyzer Direction

Guildwise already uses `.editorconfig` and built-in .NET analyzer severities.

Roslynator remains deferred for now. NDepend and SonarQube are not required project gates because they would add commercial or operational weight that is not needed for the current product stage.

Future work may add more analyzer rules when the codebase has a clean baseline.
