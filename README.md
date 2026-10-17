# Astrarium for Avalonia

This repository contains a cross-platform Avalonia port of [Astrarium](https://github.com/Astrarium/Astrarium), the open-source desktop planetarium. The upstream app is a substantial Windows WPF/WinForms application; this project preserves its astronomical purpose and calculations while rebuilding the experience for Windows, macOS, Linux, and WebAssembly browsers from a shared Avalonia UI.

## Current status

The observing workflow loads 5,070 naked-eye stars and 531 curated bright deep-sky objects, computes local sky positions with Astrarium's VSOP87/ELP2000 algorithms, and draws a horizon chart with featured asterisms and solar-system bodies. Search accepts common names and spaced or compact Messier, NGC, and IC designations. It supports quick sites or custom coordinates with elevation, UTC time changes, and persisted observer/time/layer choices, plus object selection, keyboard navigation, drag-to-pan, wheel zoom, Home reset, chart-layer toggles, and position details. Its observation planner searches a UTC observing window and reports astronomical darkness plus star, deep-sky, and solar-system target windows filtered by minimum altitude, Sun altitude, magnitude, and minimum duration; choosing a result jumps to that target's best sampled time. The deep-sky data is an offline, reproducibly curated OpenNGC subset under CC BY-SA 4.0; see [Data notices](docs/DATA_NOTICES.md).

The current [Preview 10 release](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.10) packages the shared Avalonia desktop app and static WebAssembly host, including 531 curated deep-sky objects in search, the horizon chart, and the observing planner. It was built from commit `c498c2b`. All 115 tests pass locally in Debug and Release, and the isolated Release build completes with zero warnings or errors. [Desktop CI run #39070451360](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39070451360) passed the solution tests, self-contained publishing, and native-window launch checks on Windows x64, Linux x64, macOS Intel, and Apple silicon. [Browser CI run #39070451439](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39070451439) passed the WebAssembly build, packaging, and Pages deployment. The [live browser demo](https://shobhit1kapoor.github.io/astrarium-avalonia-port-challenge/) was checked on this release: its catalog count, M31 search/Andromeda selection, and planner view work. The chart canvas has a browser-accessible name and interaction summary; WebAssembly still exposes the app as one canvas rather than separate browser controls. Desktop CI uses hosted runners; macOS/Linux were not physically tested on local hardware.

## Download and run

The desktop archives are self-contained .NET 10 builds; no .NET installation is required. Extract your operating-system archive and launch the executable from that folder. The browser archive is a static site: extract it, then serve the folder over HTTP (for example, `python -m http.server 8000 --directory <folder>`) and open the local address in a WebAssembly-capable browser. Do not open `index.html` through `file://` because browser module loading requires HTTP.

| Platform | Download | Launch |
| --- | --- | --- |
| Windows x64 | [astrarium-windows-x64.tar.gz](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/download/v0.1.0-preview.10/astrarium-windows-x64.tar.gz) | `Astrarium.Avalonia.exe` |
| Linux x64 | [astrarium-linux-x64.tar.gz](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/download/v0.1.0-preview.10/astrarium-linux-x64.tar.gz) | `./Astrarium.Avalonia` in a desktop session |
| macOS Intel | [astrarium-macos-x64.tar.gz](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/download/v0.1.0-preview.10/astrarium-macos-x64.tar.gz) | `./Astrarium.Avalonia` |
| macOS Apple silicon | [astrarium-macos-arm64.tar.gz](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/download/v0.1.0-preview.10/astrarium-macos-arm64.tar.gz) | `./Astrarium.Avalonia` |
| Browser (WebAssembly) | [astrarium-browser-wasm.tar.gz](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/download/v0.1.0-preview.10/astrarium-browser-wasm.tar.gz) | Serve extracted files over HTTP, then open in a WebAssembly-capable browser |

SHA-256 hashes for all five archives are in [`SHA256SUMS.txt`](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/download/v0.1.0-preview.10/SHA256SUMS.txt). Desktop builds are portable archives rather than installers. The live demo and downloadable browser archive are static WebAssembly sites with no backend; the archive can also be served locally over HTTP.

## Try the observing flow

1. Launch the app; it opens directly into an offline sky for Greenwich.
2. Choose a quick location or enter decimal-degree coordinates and elevation above sea level, apply the site, and set a date and time in UTC.
3. Search for a star, planet, galaxy, nebula, or cluster by name or catalog designation (for example, M31 or NGC224), select it from the horizon list, and inspect its altitude, azimuth, right ascension, declination, and magnitude. Drag the chart to pan, use the wheel to zoom, and press Home to reset.
4. Select **Plan an observing night**, set a UTC time range and visibility limits, then build the plan. Select **View** on a recommendation to return to the chart at that target's best sampled time.

## Before and after

The original screenshot is linked from the pinned upstream repository revision; it is not copied into this repository. The Avalonia images are headless renders of the production main and planner windows at their 1,080 × 720 and 780 × 600 minimum logical sizes; the packaged Windows app was launched and reviewed separately.

| Original Astrarium (WPF) | Astrarium for Avalonia |
| --- | --- |
| <img src="https://github.com/Astrarium/Astrarium/blob/75cbdda75b0e6a51bbf601d08ab89bc471e6038a/Screenshot.png?raw=true" alt="Original Astrarium screenshot" width="900"> | <img src="docs/images/astrarium-avalonia-current.png" alt="Astrarium Avalonia screenshot with a curated deep-sky catalog and observing-night planner" width="900"> |

### Observation planner

The Avalonia planner carries over the upstream planner's central observing filters for the objects included in this port. Plan times are UTC. The current implementation samples the calculated sky every 10 minutes by default and linearly interpolates window edges, so event times are estimates at that resolution.

<img src="docs/images/astrarium-avalonia-planner-current.png" alt="Astrarium observation planner with darkness and recommended target windows for deep-sky and solar-system objects" width="900">

## Migration report

### What moved and what changed

The upstream application targets .NET Framework 4.5.1 and composes a WPF/WinForms desktop UI around a plugin system. The astronomy library was the cleanest reusable boundary: `Astrarium.Algorithms` and its coefficient resources were retargeted to .NET 10, while the app shell, MVVM presentation, horizon-map renderer, input flow, and packaging were rebuilt with Avalonia. The main and observing-planner workflows now live in `Astrarium.Avalonia.Shared`; thin desktop and browser hosts adapt window-based and single-view lifetimes. Preferences use a Core interface with atomic file persistence on desktop and browser `localStorage` with an in-memory fallback. Source-generated JSON metadata supports trimming in the browser publish. The port carries forward the upstream MIT notices. The HYG star subset and OpenNGC deep-sky subset each have separate CC BY-SA 4.0 attribution and reproducible source records in [Data notices](docs/DATA_NOTICES.md).

The port preserves a useful observing loop—set a site and UTC time, inspect and navigate a calculated sky, search/select stars and deep-sky targets, plan a night under altitude, twilight, and magnitude constraints, and jump from a recommendation to the chart. It includes a curated bright deep-sky subset, not the full upstream catalog or plugin host. Telescope control, satellites, event prediction, saved/imported plans, and other plugins remain deferred rather than represented by placeholders.

### Migration effort and cost

This is a solo, AI-assisted port. The first public implementation commit was made at **11:57:17 UTC on October 17, 2026**. Preview 3 was published at **12:46:32 UTC** (49 minutes 15 seconds later), Preview 4 with the planner at **13:17:39 UTC** (1 hour 20 minutes 22 seconds later), Preview 5 at **13:54:05 UTC** (1 hour 56 minutes 48 seconds later), Preview 6 with the shared browser host at **14:38:08 UTC** (2 hours 40 minutes 51 seconds later), Preview 7 with the updated shared app and downloadable browser build at **14:52:50 UTC** (2 hours 55 minutes 33 seconds later), Preview 8 with custom site elevation at **15:16:56 UTC** (3 hours 19 minutes 39 seconds later), Preview 9 with the browser chart's accessible name at **15:36:19 UTC** (3 hours 39 minutes 2 seconds later), and Preview 10 with the curated deep-sky catalog at **21:25:19 UTC** (9 hours 28 minutes 2 seconds later). These are elapsed wall-clock windows, not active person-hours; active hours were not tracked. No paid API, dataset, or commercial UI component is required. The app is offline by default and uses the upstream MIT algorithms plus separately licensed HYG and OpenNGC subsets.

### What was harder than expected

- Retargeting the algorithm project was easier than moving the application: the original shell and most plugins depend on WPF, WinForms, `System.Drawing`, or a WinForms-hosted OpenGL renderer.
- Reusing a code license did not clear the catalog licenses. The port therefore uses smaller, separately attributed HYG and OpenNGC subsets rather than copying every upstream asset; each derivative keeps its own CC BY-SA terms and reproducible source record.
- Preferences needed their own migration boundary: required schema fields, bounded reads, safe fallback for corrupt data, and a flushed temporary file before replacement keep restarts predictable without entangling settings with chart calculations.
- One inherited refraction test passed solar diameter into the current API's air-pressure parameter. The test now calls the API correctly. The legacy Carrington fit also differs from a modern 2021 reference by about 0.0447°; that inherited limitation is recorded rather than hidden behind a looser tolerance.
- CI package success is not the same as launching the native window. The workflow now has per-OS launch smoke checks; actual CI results and local-device checks are listed separately below.
- The observation planner needed a portable time-window implementation rather than a WPF dialog. Its result edges are interpolated from a bounded sampling interval, and the README states that precision explicitly instead of implying exact rise/set timestamps. It plans against the bundled star, curated deep-sky, Moon, and planet targets; it does not reproduce the original plugin ecosystem or full catalogs.
- Extending the desktop port to WebAssembly exposed a second Avalonia lifetime model: browser apps are single-view, so the desktop planner window became an inline planner panel in the shared browser view. The browser host uses a static package and JavaScript storage bridge; the desktop still presents the planner as a native window.
- Hosting the browser build required configuring a project Pages deployment and validating the app at the repository's project subpath. The hosted version remains static and has no server-side state.

### Current limits

This port includes 5,070 naked-eye stars, 531 curated bright deep-sky objects, the Sun, Moon, and seven planets, but not the full upstream object/plugin catalog. The planner includes deep-sky targets and does not yet model object-specific observing constraints, Moon separation, weather, saved/imported plans, or telescope integration. It uses 10-minute default samples with linear boundary interpolation. The chart uses a custom polar horizon projection with a few featured asterisms; deep-sky marker extents are visual approximations. The browser version is a static WebAssembly site with no backend or server-side state, available as a hosted Pages demo and a downloadable package. Its renderer exposes one named canvas with an interaction summary; individual chart controls are not exposed as separate browser controls. Desktop Avalonia control names are covered by UI tests; no physical screen-reader session is claimed. The Windows app and both planner/main windows have been visually exercised locally, including their declared minimum dimensions at the host's default display scale. Linux/macOS launch checks are runner-based, and no physical test devices for those systems are claimed.

## Run from source

Install the .NET 10 SDK, then run `dotnet run --project src/Astrarium.Avalonia/Astrarium.Avalonia.csproj`. The bundled catalog and ephemerides work offline; no account, API key, or network connection is needed for the observing view. The browser host is built separately by `.github/workflows/astrarium-browser.yml`; the desktop solution does not require WebAssembly workloads for ordinary builds and tests.

## Project notes

- [Astrarium migration brief](docs/ASTRARIUM_MIGRATION_BRIEF.md) — upstream facts, licensing risks, port boundary, and migration decisions.
- [Astrarium delivery plan](docs/ASTRARIUM_PORT_PLAN.md) — criteria mapping, acceptance checks, and six-day execution schedule.
- [Data notices](docs/DATA_NOTICES.md) — separate catalog/source licenses and reproducible catalog provenance.
- [Challenge plan](docs/PORT_CHALLENGE_PLAN.md) — official rules and entry checklist.
- [Upstream Astrarium](https://github.com/Astrarium/Astrarium) — behavioral reference and original project.

## Verification so far

- Preview 10's exact four desktop archives passed all 115 tests, self-contained publishing, packaging, and `Window.Opened` checks on `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64` in [desktop CI run #39070451360](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39070451360). Those artifacts are published in [Preview 10](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.10) from source commit `c498c2b`.
- The separate browser workflow installed the WebAssembly workload, published Avalonia with native Skia rendering assets, checked the static output, packaged the site, and deployed Pages in [run #39070451439](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39070451439). The hosted build was manually checked for the 531-object count, M31 search/selection, and planner view; the accessibility tree still exposes a named canvas rather than separate browser controls.
- SHA-256 digests reported by the GitHub release service for all five Preview 10 archives match the published [`SHA256SUMS.txt`](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/download/v0.1.0-preview.10/SHA256SUMS.txt). The [hosted browser demo](https://shobhit1kapoor.github.io/astrarium-avalonia-port-challenge/) is deployed from `main` and uses the same observing workflow as Preview 10.
- The actual main and planner windows are rendered at 100%, 125%, 150%, and 200% with Avalonia's headless platform at their minimum logical sizes. The main window is also checked at its 1,500 × 960 default logical size. Tests check accessible names on interactive fields and lists, finite layout bounds, stable logical client sizes, and raster dimensions that track the requested scale; captures at fractional and 200% scales were visually reviewed. This simulates Avalonia scaling and does not verify native Windows monitor changes.
- The packaged Windows x64 Preview 4 app and planner opened locally. The planner generated darkness and target windows, and a recommendation returned to the chart at its best sampled UTC time. Minimum dimensions were reviewed for the 1,080 × 720 main window and 780 × 600 planner. Search, empty search, map/list selection, arrow navigation, pan, zoom, and reset were checked on the Windows host. Preview 5 carries the added accessibility names and passed the hosted Windows window-open check.
- Linux and macOS launch checks are hosted-runner-only; no physical devices are claimed. Native Windows monitor scaling has not been checked.

The observation planner was also exercised end to end on Windows: it generated darkness and target windows, scrolled the recommendation list at its minimum size, and returned a selected target to the chart at its best sampled UTC time. Native Windows display scaling and physical macOS/Linux devices have not been checked.

This is a focused first port of Astrarium's observing workflow; it does not claim parity with the original plugin ecosystem.

## Challenge

The Avalonia Port Challenge closes **October 23, 2026 at 23:59 UTC** (**18:59 America/Chicago**). Entry requirements and judging criteria are tracked in `docs/PORT_CHALLENGE_PLAN.md` and must be checked again before submission.
