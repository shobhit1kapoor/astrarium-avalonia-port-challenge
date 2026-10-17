# Astrarium to Avalonia — migration brief

**Updated:** October 17, 2026  
**Source:** [Astrarium/Astrarium](https://github.com/Astrarium/Astrarium)  
**Inspected upstream snapshot:** `75cbdda75b0e6a51bbf601d08ab89bc471e6038a` (October 2026 working tree)  
**Challenge deadline:** October 23, 2026 at 23:59 UTC (18:59 America/Chicago)

## Candidate fit

Astrarium is a desktop planetarium for exploring the sky from a chosen place and time. Its source README describes stars, planets, minor bodies, comets, deep-sky objects, satellites, event predictions, and ephemerides. The application is written in C# and uses MVVM and plugins.

The current upstream application targets .NET Framework 4.5.1 and uses WPF plus WinForms. Its sky renderer is a custom OpenGL control hosted through WinForms interop. This makes the project a strong **Best Legacy Revival** candidate and gives the migration real UI, rendering, dependency, and platform work. Rebuilding its main interaction in Avalonia also fits **Best Cross-Platform Port**. Astronomy software is useful to amateur observers, educators, and astronomy clubs; the first judging build must demonstrate that value directly.

The upstream repository is MIT-licensed. Keep the upstream license and attribution when reusing code. **The MIT license does not automatically relicense every bundled dataset, texture, font, or icon.** Upstream `Credits.md` identifies assets/catalogs with distinct terms, including NGC/IC data whose stated permission is non-commercial. The port must inventory each redistributed asset and use only data whose terms allow the planned distribution. Keep dataset attribution and licenses separate from the application-code license.

## Source architecture and portability findings

- `Astrarium` is a WPF/WinForms desktop application targeting `net451`; its app shell, dialogs, settings UI, and plugin composition depend on Windows UI frameworks.
- `Astrarium.Types` is also tied to WPF/WinForms and contains shared UI/rendering abstractions. It is not a portable domain layer.
- `Astrarium.Algorithms` targets `net451` but is a promising portable seam: its source implements coordinate transforms, time, planetary/lunar positions, eclipses, and visibility calculations. It embeds VSOP87/ELP data. We will retarget and validate it independently before relying on it in the Avalonia app.
- Most feature plugins contain renderer or view code that depends on `Astrarium.Types`, WPF, WinForms, and `System.Drawing`; they cannot be brought over unchanged as a single cross-platform plugin host.
- The default app composition includes many separately built plugins, while ASCOM/telescope integration depends on Windows hardware/software. Hardware integrations need explicit per-platform capability boundaries.
- The upstream repository includes a large range of data catalogs and assets. Reusing all of them immediately would create unnecessary legal, size, and engineering risks. The implementation uses a separately licensed HYG v4.1 subset for naked-eye stars; its provenance and CC BY-SA obligations are tracked in [data notices](DATA_NOTICES.md).

## Port boundary

**Reuse after compatibility and license checks:** upstream astronomical calculation source and its associated coefficient data, retaining copyright/license notices and original namespaces where practical. A pinned HYG v4.1 subset supplies naked-eye star positions under its own CC BY-SA 4.0 terms.

**Rebuild:** the desktop shell, view models, accessible controls, chart drawing, input handling, persistence/preferences, packaging, and platform services using Avalonia and portable .NET APIs.

**Initial workflow:**

1. Open to an observing session with a visible sky chart, observer location, and current or chosen date/time.
2. Change the location and time; recompute the sky from actual astronomy calculations.
3. Explore the chart with pan/zoom and toggle core layers such as stars, constellation lines, horizon, and solar-system bodies.
4. Search for or select an object and inspect useful identity/position/visibility details.
5. Plan an observing night: identify astronomical darkness and target windows that meet altitude, solar-altitude, magnitude, and minimum-duration filters, then jump to a recommended target's best sampled time.

This boundary prioritizes a coherent, accurate end-to-end app over a superficial port of every upstream screen. Deferred plugins and integrations must be named honestly in the README and migration report.

## Data and attribution policy

- Preserve the upstream MIT `LICENSE` for any copied upstream source.
- Keep code licenses distinct from data licenses. Record the source, required attribution, and redistribution terms for every bundled catalog, image, font, and icon in a checked-in notices file.
- Do not redistribute an upstream dataset merely because it sits in the MIT-licensed repository. In particular, do not include NGC/IC data or other assets with non-commercial, unclear, or incompatible terms in an open-source release until separately resolved.
- Prefer a small catalog with documented redistribution rights; preserve provenance and be able to reproduce it from a source record.
- Avoid runtime network dependence for the basic planetarium view; identify any live-data features as optional and handle offline/error states explicitly.

## Port verification and known algorithm limits

The port runs 92 retained upstream algorithm tests, 15 focused core tests, and four Avalonia UI tests (111 total). Preview 8 source commit `c4ad856` passed all 111 tests and window-open checks on four hosted desktop runners, with separate WebAssembly build and Pages deployment checks; the exact builds and hashes are in [Preview 8](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/releases/tag/v0.1.0-preview.8). The browser chart → planner → chart flow was exercised on the deployed build. The inherited refraction test was corrected to call the current `Flattening(h, P, T)` API without passing the solar diameter as atmospheric pressure. The legacy Carrington Fourier fit is retained unchanged, but its old reference test assumed agreement with the modern 2021 boundary to 0.02°; against the 2021 boundary now listed by the University of Maryland page, the inherited fit differs by about 0.0447°. The test records that actual legacy result and the README does not present the fit as modern precision. The star catalog generator was rerun against its pinned source file; the generated JSON is byte-for-byte identical to the bundled catalog.

## Risks and feasibility

Six days is not enough to reproduce Astrarium's complete plugin ecosystem and verify three operating systems to production depth. A winning-quality entry is still possible if the central workflow is accurate, stable, polished, and delivered consistently on all three desktop OSes. No outcome can be guaranteed, and broad scope will be cut before cross-platform quality.

| Risk | Response |
|---|---|
| Algorithms fail to retarget cleanly | Build the algorithm library in isolation first; fix only actual compatibility issues and preserve its existing published reference values. |
| Astronomical data terms differ from the code license | Audit before copying assets; ship only clearly permitted data and include separate attribution/notices. |
| Full plugin system cannot be ported in the time available | Design a small portable layer/render-feature boundary; deliver a complete core observing flow and list deferred plugins. |
| OpenGL/WPF renderer is not portable | Start with Avalonia's portable drawing surface and profile the representative chart before adding a rendering backend. |
| macOS/Linux cannot be physically tested on the developer machine | Add CI builds and smoke checks on hosted macOS/Linux runners; do not describe builds as device-tested unless they are. |
| Astrarium source may change during the port | Record the inspected upstream commit and compare against that snapshot. |

## Decisions

### 2026-10-17 — select Astrarium as the challenge project

**Decision:** Port Astrarium as an open-source Avalonia desktop application, primarily competing for Best Legacy Revival and Best Cross-Platform Port.

**Reason:** It combines a real .NET Framework 4.5.1/WPF/WinForms migration with an unusually visual and useful end product. Its calculation library appears separable from the Windows UI, allowing a defensible engineering story and an accurate core experience.

**Boundary:** The entry does not claim full plugin parity. The released desktop builds implement the core location/time/sky-map/object-inspection and observing-planner workflows on Windows, macOS, and Linux. A shared WebAssembly host is also released as an extra target; it is a static site with no backend.

**Status after Preview 8:** Catalog redistribution and source provenance are documented in [data notices](DATA_NOTICES.md); the upstream algorithms are retargeted and validated; the planner scope and its 10-minute sampling/interpolated boundary limits are documented; before-and-after screenshots and downloadable builds are published. Remaining unverified areas are full upstream plugin/catalog parity, native Windows monitor-scale changes, and physical Linux/macOS hardware validation. Those limitations are stated in the README and delivery plan; hosted runner checks are not represented as physical-device testing.
