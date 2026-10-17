# Avalonia Port Challenge plan

## Goal

Deliver a credible, polished Avalonia port or rewrite of an application that existed before the contest opened. Make the migration easy to evaluate: provide runnable builds, before-and-after evidence, and an honest account of the work.

## Rules and submission requirements

- [x] Confirm the original application existed before the contest opened; preserve the pinned upstream source revision and its dated repository history.
- [x] Confirm the upstream MIT source license permits this open-source port; separately record the HYG catalog's CC BY-SA 4.0 obligations.
- [x] Choose the open-source entry track.
- [x] Keep the entry to one human contributor, within the three-person limit.
- [x] Produce self-contained builds for Windows x64, Linux x64, macOS Intel, and macOS Apple silicon with the planner and accessible control names in [Preview 5](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.5). The packaged Windows Preview 4 archive and planner were launched and visually reviewed locally; Preview 5 window-open checks ran on hosted runners for all four RIDs.
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

## Current project and remaining work

Astrarium is the selected **open-source** submission, targeting **Best Legacy Revival** and **Best Cross-Platform Port**. See the [migration brief](ASTRARIUM_MIGRATION_BRIEF.md) and [delivery plan](ASTRARIUM_PORT_PLAN.md) for the source audit, port boundary, implementation status, evidence map, risks, and dated schedule.

- [x] Confirm the original Windows planetarium predates the challenge and its code license allows an open-source port.
- [x] Establish a portable .NET/Avalonia solution, run the observing flow on Windows, and pass an initial hosted build/test/package matrix on Windows, macOS, and Linux.
- [x] Pass the planner source's full tests, self-contained publishing, and explicit `Window.Opened` marker check on Windows x64, Linux x64, macOS Intel, and macOS Apple silicon ([run #39035676876](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39035676876)); verify and publish the resulting artifacts in [Preview 4](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.4).
- [x] Run all 109 tests, self-contained publishing, packaging, and window-open checks for all four RIDs on source commit `8e84fa5` ([run #39038065940](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39038065940)); publish those exact packages with SHA-256 checksums in [Preview 5](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.5).
- [x] Render the production main and planner windows at 100%, 125%, 150%, and 200% Avalonia headless scaling at their minimum logical sizes; assert accessible names, finite bounds, stable logical sizes, and matching raster dimensions. This is simulated layout evidence; native display-scale behavior remains unchecked.
- [x] Review the current observing flow at the minimum window size on Windows; keyboard navigation, search, map selection, panning, zoom, reset, and planner scrolling have been checked. The review used the host's default display scale.
- [x] Publish judge-downloadable release builds for each target OS. The public MIT source repository is available at [shobhit1kapoor/astrarium-avalonia-port-challenge](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge); current published binaries are in [Preview 5](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.5), with SHA-256 checksums.
- [x] Finish the honest README migration report with observed elapsed engineering time, migration cost, and remaining limits.
- [ ] Verify the current-source build at native non-default Windows monitor scaling if the environment permits.
- [ ] Submit through the official form before the deadline and save the confirmation.
