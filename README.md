# Astrarium for Avalonia

This repository contains a cross-platform Avalonia port of [Astrarium](https://github.com/Astrarium/Astrarium), the open-source desktop planetarium. The upstream app is a substantial Windows WPF/WinForms application; this project preserves its astronomical purpose and calculations while rebuilding the desktop experience for Windows, macOS, and Linux.

## Current status

The observing workflow loads a licensed catalog of 5,070 naked-eye stars, computes local sky positions with Astrarium's VSOP87/ELP2000 algorithms, and draws a horizon chart with featured asterisms and solar-system bodies. It supports site/time changes, search and selection, keyboard object navigation, drag-to-pan, wheel zoom, Home reset, chart-layer toggles, and object details. The Windows Release app has built, launched, and received a visual review. The complete plugin catalog is not ported.

Hosted CI passed solution tests and self-contained packaging on Windows x64, Linux x64, macOS Intel, and macOS Apple silicon. A new runner smoke check now starts the desktop window on each OS; its result is pending. Only Windows has had a local GUI launch and visual check. Public release assets and the final measured migration report are still open.

## Before and after

The original screenshot is linked from the pinned upstream repository revision; it is not copied into this repository. The Avalonia screenshot is captured from the running port.

| Original Astrarium (WPF) | Astrarium for Avalonia |
| --- | --- |
| <img src="https://github.com/Astrarium/Astrarium/blob/75cbdda75b0e6a51bbf601d08ab89bc471e6038a/Screenshot.png?raw=true" alt="Original Astrarium screenshot" width="900"> | <img src="docs/images/astrarium-avalonia.jpg" alt="Astrarium Avalonia screenshot" width="900"> |

## Migration report

### What moved and what changed

The upstream application targets .NET Framework 4.5.1 and composes a WPF/WinForms desktop UI around a plugin system. The astronomy library was the cleanest reusable boundary: `Astrarium.Algorithms` and its coefficient resources were retargeted to .NET 10, while the app shell, MVVM presentation, horizon-map renderer, input flow, and packaging were rebuilt with Avalonia. The port carries forward the upstream MIT notices. The 5,070-star HYG subset has its own CC BY-SA 4.0 attribution and reproducible source record in [Data notices](docs/DATA_NOTICES.md).

The first port preserves a useful observing loop—set a site and UTC time, inspect and navigate a calculated sky, search/select targets, and review their coordinates and altitude. It does not port the upstream plugin host or claim parity for deep-sky catalogs, telescope control, satellites, event prediction, or other plugins. Those are deferred rather than represented by placeholders.

### Migration effort and cost

This is a solo, AI-assisted port. The first public implementation commit was made on **October 17, 2026**; the release report will update the elapsed engineering window when the release candidate is frozen. We did not use a stopwatch, so the repository will distinguish commit-to-release elapsed time from active person-hours instead of inventing a labor-hour total. No paid API, dataset, or commercial UI component is required; the app is offline by default and uses the upstream MIT algorithms plus the separately licensed HYG subset.

### What was harder than expected

- Retargeting the algorithm project was easier than moving the application: the original shell and most plugins depend on WPF, WinForms, `System.Drawing`, or a WinForms-hosted OpenGL renderer.
- Reusing a code license did not clear the catalog licenses. The port therefore ships a smaller, separately attributed catalog rather than copying every upstream asset.
- One inherited refraction test passed solar diameter into the current API's air-pressure parameter. The test now calls the API correctly. The legacy Carrington fit also differs from a modern 2021 reference by about 0.0447°; that inherited limitation is recorded rather than hidden behind a looser tolerance.
- CI package success is not the same as launching the native window. The workflow now has per-OS launch smoke checks; actual CI results and local-device checks are listed separately below.

### Current limits

This first port includes 5,070 naked-eye stars, the Sun, Moon, and seven planets, but not the full upstream object/plugin catalog. The chart uses a custom polar horizon projection with a few featured asterisms; it is not a general-purpose telescope planner. The Windows app has been launched and visually exercised locally. Linux/macOS launch checks are runner-based, and no physical test devices for those systems are claimed.

## Run from source

Install the .NET 10 SDK, then run `dotnet run --project src/Astrarium.Avalonia/Astrarium.Avalonia.csproj`. The bundled catalog and ephemerides work offline; no account, API key, or network connection is needed for the observing view.

## Project notes

- [Astrarium migration brief](docs/ASTRARIUM_MIGRATION_BRIEF.md) — upstream facts, licensing risks, port boundary, and migration decisions.
- [Astrarium delivery plan](docs/ASTRARIUM_PORT_PLAN.md) — criteria mapping, acceptance checks, and six-day execution schedule.
- [Data notices](docs/DATA_NOTICES.md) — separate catalog/source licenses and reproducible catalog provenance.
- [Challenge plan](docs/PORT_CHALLENGE_PLAN.md) — official rules and entry checklist.
- [Upstream Astrarium](https://github.com/Astrarium/Astrarium) — behavioral reference and original project.

## Verification so far

- `dotnet test Astrarium.Port.sln --configuration Release --no-restore`: 97 tests passed (92 retained Astrarium algorithm checks and 5 port-core checks), zero failures, zero warnings.
- `dotnet build src/Astrarium.Avalonia/Astrarium.Avalonia.csproj --configuration Release --no-restore`: Windows-host Release build succeeded with zero warnings.
- Hosted CI run [#39030942026](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39030942026) passed tests and self-contained packaging on `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64` for the previous public revision. The newest layout, pan/zoom, and per-OS desktop launch smoke steps are pending in the next run.
- Self-contained `dotnet publish` also succeeded locally for all four runtime identifiers.
- The packaged Windows x64 executable opened and remained responsive during a local launch smoke check. On the source build, search, empty search, map selection, list selection, arrow navigation, pan, zoom, and reset were exercised manually. The newest source revision's CI launch smoke is pending on Windows/macOS/Linux runners.

This is a focused first port of Astrarium's observing workflow; it does not claim parity with the original plugin ecosystem.

## Challenge

The Avalonia Port Challenge closes **October 23, 2026 at 23:59 UTC** (**18:59 America/Chicago**). Entry requirements and judging criteria are tracked in `docs/PORT_CHALLENGE_PLAN.md` and must be checked again before submission.
