# Astrarium Avalonia port — delivery plan

## Goal and judging stance

Deliver a credible, polished cross-platform Avalonia port of Astrarium by the challenge deadline. Compete primarily in **Best Legacy Revival** and **Best Cross-Platform Port**. Aim to score well on all five published judging dimensions through evidence, not breadth claims. A win is not predictable: the judges have not published category weights or competitor entries in this plan.

The current entry window closes **October 23, 2026 at 23:59 UTC / 18:59 America/Chicago**. As of October 17, six days remain. The release target is October 22, preserving October 23 as a submission and recovery buffer.

## Judging criteria → evidence we must produce

| Published criterion | How Astrarium can score strongly | Required proof |
|---|---|---|
| Difficulty and completeness of migration | Migrate a substantial WPF/WinForms/.NET Framework planetarium into a real Avalonia app; preserve the central observing workflow and reuse validated astronomy calculations. | Side-by-side workflow evidence, source map, scope table, compatibility notes, upstream commit reference, clear list of migrated/deferred features. |
| Quality of resulting application | Make an accurate and useful planetarium, not a static star-chart mockup. Provide good defaults, readable chart, responsive controls, object details, and useful empty/error/offline behavior. | Runnable end-to-end demo; checked calculated positions; screenshots; usability review at common window sizes and scaling. |
| Strength of cross-platform implementation | Ship the same Avalonia app and core implementation on Windows, macOS, and Linux. Isolate platform-specific behavior. | Release builds and CI build matrix for all three, plus launch/smoke evidence per OS; explicitly identify any OS tested only in CI. |
| Engineering quality | Keep astronomy/domain logic independent from Avalonia, use explicit services for persistence and platform boundaries, keep data licenses traceable, and provide maintainable build/release instructions. | Clean architecture, project builds, focused regression tests, no platform-specific UI references in portable layers, CI and static checks, dependency/data notices. |
| Usefulness of migration write-up | Give concrete, honest information useful to another WPF/WinForms migration. | README write-up with real effort/time, what carried over, what changed, surprises, workarounds, per-OS differences, limitations, screenshots, and build links. |

The challenge announcement says the same codebase on Windows, macOS, and Linux is the cross-platform category target; mobile/WebAssembly is extra recognition. It also says a small immaculate port can beat a sprawling incomplete one. Therefore desktop completeness, build reliability, and evidence outrank bonus targets.

## Baseline at the end of October 17

**Implemented:** separate portable algorithm, core, and Avalonia UI projects; bundled licensed HYG magnitude-6 subset; actual topocentric positions for 5,070 stars, Sun, Moon, and seven planets; a polar horizon chart with featured asterisms, collision-aware labels, compass rotation, drag pan, wheel zoom, and Home reset; location presets plus custom latitude, longitude, and elevation entry; UTC date/time and 24-hour scrubber; search with an explicit empty state; object selection and position details; keyboard arrow navigation; separate star/asterism/body chart layers; and atomic, versioned local persistence for observer site, UTC time, and layer visibility. Preferences use required JSON fields, a bounded read, a safe fallback for damaged files, and a durable temporary-file replacement. The restored observation planner reports astronomical-darkness intervals and target windows filtered by minimum altitude, maximum Sun altitude, magnitude, and minimum duration; a result returns to the chart at its best sampled instant. Planner windows use bounded time samples with linearly interpolated edges, not exact event solving. A manual close/relaunch check restored a custom Mauna Kea site, a changed UTC time, and a hidden-body layer. All 111 tests pass locally and on each hosted desktop runner, including headless layout renders of both production windows at scales 1.0, 1.25, 1.5, and 2.0, main-window layout checks at its 1,500 × 960 default size, and accessibility-name assertions for input and list controls; the solution builds with zero warnings/errors. The packaged Windows Release archive has been launched and visually reviewed.

**Still unproven or incomplete:** full upstream plugin/catalog parity, saved/imported observation plans, deep-sky targets, weather or Moon-separation scoring, native Windows monitor-scale changes, and physical Linux/macOS device checks. Both windows were rendered through Avalonia headless at their declared minimum logical dimensions at 1.0, 1.25, 1.5, and 2.0 scaling; the 1.25 and 2.0 captures were visually reviewed. The main window is also layout-checked at its 1,500 × 960 default size. These renders do not validate actual monitor DPI behavior. Preview 8 source commit `c4ad856` passed all 111 tests, self-contained publishing, packaging, and actual `Window.Opened` checks on `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64` in [CI run #39043898834](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39043898834); those exact packages are published in [Preview 8](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.8). The WebAssembly build and Pages deployment passed in [run #39043898806](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39043898806), and the current hosted demo's chart → planner → chart flow has been exercised. The demo is static and has no backend. The packaged Windows Preview 4 app and planner were launched locally; later Windows and other OS checks are runner-based. Test counts do not establish astronomical accuracy for every object/date; the inherited low-accuracy Carrington fit has an explicit 2021 residual check.

**Remaining checks:** verify native Windows monitor-scale behavior if the environment permits, and submit the completed entry before October 23. Both windows have minimum-size headless layout renders at common scales; native Windows and physical macOS/Linux device checks remain unverified, and runner-only evidence is labeled honestly.

## Acceptance criteria for the first release

### Product

- [x] The app starts directly into a useful observing session; no account, network connection, or manual setup is needed to see the sample sky.
- [x] Observer name, coordinates, elevation, and UTC date/time can be changed, validated, saved across restarts, and used to recompute the chart and observing plan.
- [x] The chart is driven by real Astrarium calculations and a clearly licensed catalog, not hard-coded screenshot coordinates.
- [x] The user can pan/zoom, toggle core chart layers, search/select objects, and see object identity plus coordinates/altitude or visibility data.
- [x] Search, location/time inputs, the time slider, and object lists expose accessible names; the Avalonia headless UI tests assert names for interactive fields and lists.
- [x] The above-horizon list shows a filtered set of bright targets for the selected instant; it is labeled as a current-sky view.
- [x] The planner finds darkness and candidate target windows using the migrated calculations; it filters by altitude, Sun altitude, magnitude, and minimum window duration, and can focus a result on the chart.
- [x] The chart and controls remain usable with keyboard navigation and at the declared minimum dimensions on the Windows host's default display scale; list scrolling was checked at the planner's minimum size.
- [x] Render both production windows at the declared minimum logical sizes with Avalonia headless at 100%, 125%, 150%, and 200%; check bounds, stable logical size, raster dimensions, and visual captures.
- [x] Check the main production window at its 1,500 × 960 default logical size for finite layout bounds and matching raster dimensions.
- [ ] Verify native Windows per-monitor scaling behavior; headless rendering does not replace this check.
- [x] Invalid planner input and empty plan results are surfaced; the core observing view remains available offline.

### Architecture and migration

- [x] The Avalonia app targets a portable .NET runtime and contains no WPF/WinForms dependencies.
- [x] Astronomy calculations, app/domain models, and UI are separate components with narrow boundaries.
- [x] Upstream code retains its license and attribution; every distributed dataset/asset has its own source and license record.
- [x] There is a recorded comparison of source behavior with the Avalonia implementation; unsupported plugins/integrations are enumerated rather than implied to work.
- [x] Upstream changes are anchored to a commit SHA in the migration brief.

### Platforms and entry

- [x] One source tree builds for Windows, macOS, and Linux; the same release feature set is available on each.
- [x] Each platform has a downloadable artifact with clear launch instructions and no unexplained missing native library.
- [x] Each platform has an actual launch check, or the evidence explicitly labels it as CI-only.
- [x] README includes original and migrated screenshots, platform builds, the short migration report, and actual effort/cost.
- [x] Public repository uses an OSI-approved license for the application source; third-party/data notices are accurate.
- [ ] Official submission form is completed before the deadline.

## Architecture target

```text
Astrarium.Avalonia (Avalonia UI, MVVM, portable platform integrations)
              │
              ▼
Astrarium.Port.Core (observing session, chart projection, search, visibility use cases)
              │
              ▼
Astrarium.Algorithms (upstream astronomy algorithms and validated coefficient data)

Optional OS adapters: location lookup, file access, notifications, telescope integrations
```

The chart renderer is an Avalonia control that consumes immutable chart data from the core. Astronomy calculations stay outside rendering code. The first implementation should use Avalonia's cross-platform drawing API; introduce lower-level graphics only after profiling demonstrates a measured need. Hardware or OS-specific integrations report capabilities through interfaces and do not block the basic planetarium workflow.

## Six-day execution order

| Date | Work | Exit evidence |
|---|---|---|
| Oct 17 | Finish the observing planner vertical slice, reconcile challenge criteria, review the current Windows UI, add scaled-layout/accessibility checks, and publish the current preview. | **Completed:** 111 tests locally and in the Preview 8 hosted matrix, zero-warning Release build, four-RID launch checks, Preview 8 artifacts/checksums, production-window screenshots, headless renders at four scales plus the main-window default size, and a hosted WebAssembly demo. |
| Oct 18 | Check native Windows monitor scaling if the environment permits; review invalid/empty-plan behavior and accessibility; validate representative observing cases. | Findings fixed or recorded; simulated scale layout and default-scale dimensions remain verified. |
| Oct 19 | Review astronomy boundary cases, chart projection, dates and locations; run a clean-checkout build and audit dependencies/data notices. | No blocker in the observing/planning workflow; all remaining accuracy limits stated. |
| Oct 20 | Freeze features; inspect current release artifacts and README links from a clean download; prepare a short judge-facing demonstration. | Preview 8 binaries and hashes agree with app source commit `c4ad856`; README links, screenshots, migration details, and the hosted demo are current. |
| Oct 21 | Review the entry against all five judging dimensions; finalize concise migration account and gather missing form details. | Judge-facing evidence complete except final form submission. |
| Oct 22 | Reserve time for the official submission form and handle any access or formatting issue; avoid unnecessary release churn. | Entry ready to submit, with a full day of contingency. |
| Oct 23 | Submit the official entry with time reserved for form issues; stop feature work early enough to beat the 23:59 UTC cutoff. | Submission confirmation saved before deadline. |

The schedule is aggressive. If a release blocker appears, cut optional features such as telescope integrations, broad catalog coverage, or mobile/WebAssembly before sacrificing three-desktop build quality or migration evidence.

## Release checklists

### Before implementation expands

- [x] Inspect and record the upstream commit SHA in the migration brief.
- [x] Build `Astrarium.Algorithms` after retargeting and document the compatibility changes.
- [x] List bundled assets/data sources and record their licenses and provenance.
- [x] Build and open the Avalonia shell on Windows.
- [x] Run the retained upstream astronomy reference tests.

### Before the release candidate

- [ ] Validate date/time timezone and location coordinate conventions; test representative northern and southern latitudes and dates.
- [ ] Verify chart projection near the horizon/zenith and across angle wraparound.
- [x] Verify search, selection, persistence, keyboard paths, and recovery from malformed location input.
- [ ] Verify chart resize and the minimum window size at common non-default display scaling levels.
- [x] Build Windows, macOS, and Linux; launch the artifacts on hosted runners; record the runtime matrix and note that only Windows was also tested on a physical device.
- [ ] Scan dependencies/assets and check all notices and license conditions.

### Submission evidence

- [x] Original screenshot linked to the pinned upstream revision with its provenance stated.
- [x] Migrated screenshots captured from the running Avalonia app and planner.
- [x] README covers observed elapsed migration window, retained code, rewrites, surprises, compromises, platform differences, known limits, and build/run instructions.
- [x] Public source repository and OSI-approved MIT code license.
- [x] Downloadable builds for the current planner and custom-site-elevation source on each target OS, with checksums in [Preview 8](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.8).
- [ ] Official entry form submission confirmation.
