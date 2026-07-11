# ADR 0008: Frontend UI Stack and Web Styling Boundaries

## Status

Accepted

## Context

Guildwise now has a Blazor web interface with a roster overview, reusable Web components, a dark Guildwise/WoW-inspired visual direction, roster badges and client-side roster search, filter and sort controls.

Long term, Guildwise should move toward a modern admin dashboard structure with sidebar navigation, topbar space, cards, panels, tables, forms, widgets and dashboard navigation.

AdminLTE is a useful structural and visual reference for those patterns, but Guildwise needs its own roster and raid-tool identity. UI framework decisions must also preserve the existing Clean Architecture boundaries.

## Decision

Guildwise Web will use custom Blazor components and custom CSS for now.

AdminLTE may be used as structural and visual inspiration, but it is not a direct Guildwise dependency.

Bootstrap 5 may be evaluated later as a possible layout, form and utility basis. This ADR does not introduce Bootstrap.

New UI frameworks, component libraries or admin templates require a separate explicit ADR before they are introduced.

## Rationale

Guildwise needs a distinct WoW, roster and raid-tool visual identity.

A ready-made admin template could dictate structure, CSS and JavaScript conventions too strongly for the current product stage.

The current Guildwise Web components are sufficient for layout, panels, badges, tables and controls.

Introducing a UI library too early could add unnecessary dependencies and coupling.

Clean Architecture must not be diluted by UI framework details.

## Rules

- UI dependencies must stay in `Guildwise.Web`.
- Domain must not know about UI, CSS, colors, Blazor, icons or component concepts.
- Application must not contain CSS classes, color values, icon names, component names or Blazor types.
- Infrastructure must remain free of UI concepts.
- Application DTOs must remain UI-agnostic.
- Display mapping, labels, badge colors and WoW class colors belong in the Web project.
- AdminLTE may be used as a reference for layout patterns, but must not be copied into Guildwise or added as a direct dependency.
- Bootstrap, AdminLTE, MudBlazor, Blazorise or other UI libraries must not be introduced without an ADR.
- Existing Guildwise Web components should be evolved before introducing external UI libraries.

## Consequences

### Positive

- Guildwise keeps more control over its own visual identity.
- The Web layer can evolve around roster-specific workflows.
- External framework conventions are less likely to dictate architecture or design.
- A later UI library introduction remains possible after deliberate evaluation.

### Negative

- Guildwise must implement more UI component behavior itself at first.
- The Web project must maintain its own styling consistency.
- Future UI library adoption may require migration work if custom components grow significantly.

## Not Decided

- Whether Bootstrap 5 will be introduced.
- Whether an icon system will be introduced.
- Whether a dashboard template will be used later.
- Whether a Blazor component library will be introduced.
- Whether a topbar or dashboard shell will be built as a separate feature.
