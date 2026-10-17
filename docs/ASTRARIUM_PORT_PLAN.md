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

**Implemented:** separate portable algorithm, core, and Avalonia UI projects; bundled licensed HYG magnitude-6 subset; actual topocentric positions for 5,070 stars, Sun, Moon, and seven planets; a polar horizon chart with featured asterisms, collision-aware labels, compass rotation, drag pan, wheel zoom, and Home reset; location presets and coordinate entry; UTC date/time and 24-hour scrubber; search with an explicit empty state; object selection and position details; keyboard arrow navigation; separate star/asterism/body chart layers; and atomic, versioned local persistence for observer site, UTC time, and layer visibility. A manual close/relaunch check restored a custom Mauna Kea site, a changed UTC time, and a hidden-body layer. The 92 upstream algorithm checks and seven port-core checks pass locally. Windows Release builds with no warnings and has been visually exercised on Windows.

**Still unproven or incomplete:** a full observing-night planner, physical Linux/macOS device checks, hosted CI for the new persistence code and actual `Window.Opened` marker, and a responsive review at the minimum supported window size. The public preview release and hosted run #39032101742 are from the preceding code revision; the run passed tests, packaging, and process-live startup checks for `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64`. Only Windows has had a local desktop launch and visual check. The current check count does not establish astronomical accuracy for every object/date; the inherited low-accuracy Carrington fit has an explicit 2021 residual check.

**Next order:** (1) publish the persistence implementation and pass the four-target test, package, and GUI-launch matrix; (2) replace the preview archives with verified builds from that revision; (3) review the UI at its minimum supported size and test common scaling; (4) finish the migration report and submit early on October 23. The matrix includes macOS Intel and Apple silicon so architecture gaps are visible.

## Acceptance criteria for the first release

### Product

- [ ] The app starts directly into a useful observing session; no account, network connection, or manual setup is needed to see the sample sky.
- [x] Observer coordinates/location and date/time can be changed, validated, saved across restarts, and used to recompute the chart.
- [ ] The chart is driven by real Astrarium calculations and a clearly licensed catalog, not hard-coded screenshot coordinates.
- [x] The user can pan/zoom, toggle core chart layers, search/select objects, and see object identity plus coordinates/altitude or visibility data.
- [x] The above-horizon list shows a filtered set of bright targets for the selected instant; it is labeled as a current-sky view, not a nightly observing planner.
- [ ] The chart and controls remain usable with keyboard navigation, resizing, high DPI scaling, and reduced window dimensions.
- [ ] Errors and missing/unsupported data/services are surfaced clearly. Core viewing remains available offline.

### Architecture and migration

- [ ] The Avalonia app targets a current portable .NET runtime and contains no WPF/WinForms dependencies.
- [ ] Astronomy calculations, app/domain models, and UI are separate components with narrow boundaries.
- [ ] Upstream code retains its license and attribution; every distributed dataset/asset has its own source and license record.
- [ ] There is a recorded comparison of source behavior with the Avalonia implementation; unsupported plugins/integrations are enumerated rather than implied to work.
- [ ] Upstream changes are anchored to a commit SHA in the migration brief.

### Platforms and entry

- [ ] One source tree builds for Windows, macOS, and Linux; the same release feature set is available on each.
- [ ] Each platform has a downloadable artifact with clear launch instructions and no unexplained missing native library.
- [ ] Each platform has an actual launch check, or the evidence explicitly labels it as CI-only.
- [ ] README includes original and migrated screenshots, platform builds, the short migration report, and actual effort/cost.
- [ ] Public repository uses an OSI-approved license for the application source; third-party/data notices are accurate.
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
| Oct 17 | Pin the upstream snapshot; audit source/data licenses; retarget the algorithm library; implement first observing chart; add core tests and cross-OS packaging workflow. | Windows vertical slice builds/runs; 97 tests pass; CI matrix is ready to execute. |
| Oct 18 | Re-run the matrix after the latest UI changes; check downloaded artifacts and arrange macOS/Linux launches. | Latest source packages on each target; physical versus CI-only checks are recorded accurately. |
| Oct 19 | Complete the central observing flow: reliable chart interactions, search/selection, useful above-horizon list, location/time, and saved session if stable. | End-to-end use works offline and handles invalid input and slow calculations. |
| Oct 20 | Improve keyboard access, resizing/scaling, visual hierarchy, performance, and error states; freeze feature scope. | Release candidate has no known blocker in the main workflow. |
| Oct 21 | Clean-checkout build, dependency/data/license review, screenshots, and honest migration-write-up draft. | Judge-facing evidence is complete except final release links and form. |
| Oct 22 | Publish public source and downloadable artifacts; verify downloads from clean folders; finalize README and entry details. | All required open-source materials are accessible and runnable. |
| Oct 23 | Final contingency and entry submission; stop feature work early enough to submit before 23:59 UTC. | Confirmation of submission saved. |

The schedule is aggressive. If a release blocker appears, cut optional features such as telescope integrations, broad catalog coverage, or mobile/WebAssembly before sacrificing three-desktop build quality or migration evidence.

## Release checklists

### Before implementation expands

- [ ] Inspect and record `git rev-parse HEAD` for the upstream checkout.
- [ ] Build `Astrarium.Algorithms` after retargeting and document changes.
- [ ] List every bundled asset/data source and decide include/omit/replace.
- [ ] Build and open the Avalonia shell on Windows.
- [ ] Verify scientific calculations against existing upstream reference tests.

### Before the release candidate

- [ ] Validate date/time timezone and location coordinate conventions; test representative northern and southern latitudes and dates.
- [ ] Verify chart projection near the horizon/zenith and across angle wraparound.
- [x] Verify search, selection, persistence, keyboard paths, and recovery from malformed location input.
- [ ] Verify chart resize and the minimum window size at common display scaling levels.
- [ ] Build Windows, macOS, and Linux; launch the artifacts; record OS versions and environment.
- [ ] Scan dependencies/assets and check all notices and license conditions.

### Submission evidence

- [ ] Original screenshot sourced from upstream release/current app with its provenance stated.
- [ ] Migrated screenshot captured from the actual Avalonia build.
- [ ] README covers actual migration effort, retained code, rewrites, surprises, compromises, platform differences, known limits, and build/run instructions.
- [ ] Public source repository and OSI-approved code license.
- [ ] Downloadable builds per target OS and checksum/release version.
- [ ] Official entry form submission confirmation.
