# 0021 - Playwright UI Smoke Tests

## Ziel

Guildwise soll echte Chromium-Smoke-Tests gegen die tatsächlich gestartete Blazor-Web-App erhalten.

## Testgrenze

Playwright prüft:

* App-Startup
* Browser-Rendering
* Blazor-Interaktivität
* Navigation
* vollständige komponentenübergreifende UI-Flows

Playwright ersetzt nicht:

* Domain- oder Application-Unit-Tests
* bUnit-Komponententests
* Persistence-Integrationstests
* Architecture Tests

## Persistenzstrategie

Der erste E2E-Slice verwendet ausschließlich InMemory-Persistenz. Playwright-Tests dürfen keine bestehende Entwicklerdatenbank verwenden oder verändern.

PostgreSQL bleibt durch die vorhandenen Integrationstests geschützt.

## Geplante Slices

* `0021a – Playwright host foundation`: abgeschlossen mit App-Host-Fixture und Navigationstest.
* `0021b – Playwright CI and diagnostics`: umgesetzt mit separatem CI-Job, Chromium-Installation und Fehlerartefakten.
* `0021c – Initial Playwright smoke flows`: offen für fachliche Browserflows.

## Out of Scope

* externe APIs
* Browsermatrix
* visuelle Regressionstests
* Performance-Tests
* E2E-Coverage

## Gesamtakzeptanzkriterien

* echter Chromium-Browser
* echte HTTP-App
* deterministischer InMemory-Start
* CI-Ausführung
* Fehlerdiagnose
* wenige wertvolle Browserflows
