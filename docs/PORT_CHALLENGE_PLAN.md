# Avalonia Port Challenge plan

## Goal

Deliver a credible, polished Avalonia port or rewrite of an application that existed before the contest opened. Make the migration easy to evaluate: provide runnable builds, before-and-after evidence, and an honest account of the work.

## Rules and submission requirements

- [x] Confirm the original application existed before the contest opened; preserve the pinned upstream source revision and its dated repository history.
- [x] Confirm the upstream MIT source license permits this open-source port; separately record the HYG catalog's CC BY-SA 4.0 obligations.
- [x] Choose the open-source entry track.
- [x] Keep the entry to one human contributor, within the three-person limit.
- [x] Produce self-contained builds for Windows x64, Linux x64, macOS Intel, and macOS Apple silicon with the planner and accessible control names in [Preview 7](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.7). Preview 7 passed 109 tests and native window-open checks on all four hosted runners; the Windows Preview 4 archive and planner were also launched and reviewed locally.
- [x] Add a separately built static WebAssembly host using the shared Avalonia UI, verify the sky/planner/recommendation flow, and deploy the app as a [public Pages demo](https://shobhit1kapoor.github.io/astrarium-avalonia-port-challenge/). The browser target supplements the desktop builds; it is static and has no backend.
- [x] Capture before-and-after screenshots; the source screenshot is linked at the pinned upstream revision rather than redistributed, and the Avalonia image is a deterministic render of the production window. The packaged Windows app was separately launched and reviewed.
- [x] Write a migration report covering elapsed release window and cost, what carried over, redesign, surprises, compromises, and runner-only platform checks; actively tracked person-hours were not recorded and are not claimed.
- [ ] Submit through the [official entry form](https://docs.google.com/forms/d/e/1FAIpQLSfJsgz1HRHKsxH0yRnn2wcvbZ-tSkmnjDXxIhE5YjM1v3lVkg/viewform) before **2026-10-23 23:59 UTC** (**2026-10-23 18:59 America/Chicago**); retain the confirmation.

The official announcement also asks for an honest migration account, says AI-assisted code is allowed, and permits solo entries or teams of up to three. It evaluates migration difficulty/completeness, resulting-app quality, cross-platform implementation, engineering quality, and usefulness of the write-up. Mobile/WebAssembly is additional recognition, after desktop quality. These details and the deadline were rechecked against the [official challenge announcement](https://avaloniaui.net/blog/avalonia-port-challenge) on October 17, 2026.

Open-source track:

- [x] Publish the source in a public GitHub repository under the MIT license.
- [x] Put platform build links, before/after screenshots, and the migration write-up in the repository README.

Closed-source track:

- [ ] Publish screenshots and the write-up somewhere judges can access, such as a blog post, release page, or PDF.
- [ ] Provide direct build links and any required trial credentials or license keys on the entry form.

## How to optimize for judging

Judges evaluate migration difficulty and completeness, application quality, cross-platform implementation, engineering quality, and the migration write-up. The categories are Best Cross-Platform Port, Best Legacy Revival, Best Everyday Tool, and Best IT Pro Tool. A single codebase reaching Windows, macOS, and Linux is central to the cross-platform category. Mobile or WebAssembly support can earn additional recognition, but should only be pursued after the core desktop experience is complete.

Working priorities:

1. Choose a small, coherent slice that can be finished to a shippable standard.
2. Preserve the source app's recognizable purpose and important workflows while improving usability where the old UI does not translate well.
3. Prove the core workflow on each selected OS early; don't leave packaging or platform gaps until the final days.
4. Keep the engineering story visible: record decisions, compatibility issues, workarounds, and time spent as they happen.
5. Prefer a complete, polished port over broad but unfinished feature coverage.
6. Treat browser/WebAssembly as additional recognition after desktop quality; build it from shared views, run it offline from a static package, and state the single-view design and static-host limitation clearly.

### Criterion-by-criterion evidence and honest gaps

| Judging criterion | Evidence in this entry | Remaining gap or caveat |
| --- | --- | --- |
| Migration difficulty and completeness | The source targets .NET Framework 4.5.1 and couples WPF/WinForms UI to a plugin system. This port retargets and tests the astronomy algorithms, then rebuilds a recognizable observing workflow in Avalonia, including a portable planner. | The port is intentionally a complete observing slice, not a port of every plugin, deep-sky catalog, telescope integration, or satellite/event workflow. |
| Resulting application quality | Usable chart and planner, search and selection, keyboard navigation, pan/zoom/reset, persisted observer/time/layer preferences, accessible control names, and an honest before/after comparison. Main/planner layouts are exercised at four simulated scale factors and two window sizes. | Visual QA on physical hardware has been local Windows-only; layout-scale simulation is not equivalent to testing native display scaling. |
| Cross-platform implementation | Shared Avalonia views compile and open on Windows x64, Linux x64, macOS Intel, and Apple silicon in hosted CI. A separate single-view WebAssembly host runs the same core observing flow from a static offline bundle and a public Pages demo. | macOS/Linux checks use hosted runners, not physical devices; the browser demo is static and has no backend. |
| Engineering quality | 109 tests (92 algorithm, 15 Core, 2 UI), four-RID CI build/publish/window-open checks, separate WebAssembly packaging checks, source-generated JSON metadata, explicit preference-storage boundary, license notices, and checksums for release archives. | CI execution is strong build/runtime evidence but does not prove every hardware/driver combination or every upstream feature. |
| Usefulness of the migration write-up | README and linked migration notes cover the source boundary, licensing, elapsed release timeline, costs, surprises, compromises, test evidence, reproducible builds, and deferred work. | Wall-clock release intervals are available; active person-hours were not tracked and are not claimed. |

The official challenge lists Best Cross-Platform Port, Best Legacy Revival, Best Everyday Tool, and Best IT Pro Tool. Astrarium most directly fits Legacy Revival and Cross-Platform Port; it is a less direct fit for Everyday Tool and is not positioned as an IT Pro tool. The official announcement says mobile/WebAssembly can receive additional recognition, after desktop quality is established. This entry therefore keeps the four desktop targets as the baseline and includes WebAssembly as a verified extra.

## Current project and remaining work

Astrarium is the selected **open-source** submission, targeting **Best Legacy Revival** and **Best Cross-Platform Port**. See the [migration brief](ASTRARIUM_MIGRATION_BRIEF.md) and [delivery plan](ASTRARIUM_PORT_PLAN.md) for the source audit, port boundary, implementation status, evidence map, risks, and dated schedule.

- [x] Confirm the original Windows planetarium predates the challenge and its code license allows an open-source port.
- [x] Establish a portable .NET/Avalonia solution, run the observing flow on Windows, and pass an initial hosted build/test/package matrix on Windows, macOS, and Linux.
- [x] Pass the planner source's full tests, self-contained publishing, and explicit `Window.Opened` marker check on Windows x64, Linux x64, macOS Intel, and macOS Apple silicon ([run #39035676876](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39035676876)); verify and publish the resulting artifacts in [Preview 4](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.4).
- [x] Run all 109 tests, self-contained publishing, packaging, and window-open checks for all four RIDs on source commit `8e84fa5` ([run #39038065940](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39038065940)); publish those exact packages with SHA-256 checksums in [Preview 5](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.5).
- [x] Re-run the 109-test four-RID matrix on follow-up test/evidence commit `b17e3db` after adding the default-size layout check and refreshing judging evidence ([run #39039239143](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39039239143)).
- [x] Pass all 109 tests, self-contained publishing, packaging, and window-open checks for all four RIDs on Preview 7 source commit `fa82fc2` ([run #39041985725](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39041985725)).
- [x] Build the browser target with native Skia/WebAssembly assets, verify static output, package it as a site, then run the downloaded artifact and hosted Pages demo in a browser and exercise the chart → planner → chart flow ([Preview 7 artifact run #39041985734](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39041985734); [Pages deployment #39042339405](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39042339405)).
- [x] Render the production main and planner windows at 100%, 125%, 150%, and 200% Avalonia headless scaling at their minimum logical sizes; assert accessible names, finite bounds, stable logical sizes, and matching raster dimensions. This is simulated layout evidence; native display-scale behavior remains unchecked.
- [x] Review the current observing flow at the minimum window size on Windows; keyboard navigation, search, map selection, panning, zoom, reset, and planner scrolling have been checked. The review used the host's default display scale.
- [x] Publish judge-downloadable release builds for each target OS and the static browser site. The public MIT source repository is available at [shobhit1kapoor/astrarium-avalonia-port-challenge](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge); current published binaries are in [Preview 7](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.7), with SHA-256 checksums for all five archives. The [hosted Pages demo](https://shobhit1kapoor.github.io/astrarium-avalonia-port-challenge/) was deployed and exercised.
- [x] Finish the honest README migration report with observed elapsed engineering time, migration cost, and remaining limits.
- [ ] Verify the current-source build at native non-default Windows monitor scaling if the environment permits.
- [ ] Submit through the official form before the deadline and save the confirmation.
