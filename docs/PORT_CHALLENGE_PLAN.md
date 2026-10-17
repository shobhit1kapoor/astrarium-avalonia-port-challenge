# Avalonia Port Challenge plan

## Goal

Deliver a credible, polished Avalonia port or rewrite of an application that existed before the contest opened. Make the migration easy to evaluate: provide runnable builds, before-and-after evidence, and an honest account of the work.

## Rules and submission requirements

- [ ] Confirm the original application existed before the contest opened; preserve dated evidence such as releases, repository history, or screenshots.
- [ ] Confirm we own the source application or that its license permits this port and the planned distribution model.
- [ ] Decide the entry track: open source or closed source.
- [ ] Limit the team to three people; only one prize is awarded per team.
- [ ] Produce a runnable build for every platform we claim to support. Closed-source builds must be usable by judges; arrange a trial, temporary key, or demo account if needed.
- [ ] Capture comparable before-and-after screenshots.
- [ ] Write a migration report covering actual effort and cost, what carried over, what needed redesign, surprises, compromises, and platform-specific differences.
- [ ] Submit through the official entry form before **2026-10-23 23:59 UTC** ( **2026-10-23 18:59 America/Chicago** ).

The official announcement also asks for an honest migration account, says AI-assisted code is allowed, and permits solo entries or teams of up to three. It evaluates migration difficulty/completeness, resulting-app quality, cross-platform implementation, engineering quality, and usefulness of the write-up. Mobile/WebAssembly is additional recognition, after desktop quality. These details and the deadline were rechecked against the [official challenge announcement](https://avaloniaui.net/blog/avalonia-port-challenge) on October 17, 2026.

Open-source track:

- [ ] Publish the source in a public GitHub repository under an OSI-approved license.
- [ ] Put platform build links, screenshots, and the migration write-up in the repository, with the write-up in the README as specified by the announcement.

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
- [x] Establish a portable .NET/Avalonia solution, run the first observing flow on Windows, and add CI packaging for Windows, macOS, and Linux.
- [ ] Run the CI matrix and launch macOS/Linux artifacts; record CI-only versus physical checks.
- [ ] Finish the focused observing flow and review keyboard use, resizing, and scaling.
- [ ] Publish the source repository under an OSI-approved license and judge-downloadable builds for each target OS.
- [ ] Add comparable before/after screenshots and an honest README migration report with actual effort and cost.
- [ ] Submit through the official form before the deadline and save the confirmation.
