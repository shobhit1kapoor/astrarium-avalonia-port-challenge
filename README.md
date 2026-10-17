# Astrarium for Avalonia

This repository contains a cross-platform Avalonia port of [Astrarium](https://github.com/Astrarium/Astrarium), the open-source desktop planetarium. The upstream app is a substantial Windows WPF/WinForms application; this project preserves its astronomical purpose and calculations while rebuilding the desktop experience for Windows, macOS, and Linux.

## Current status

The first Windows vertical slice now runs: it loads a licensed catalog of 5,070 naked-eye stars, computes local sky positions with Astrarium's VSOP87/ELP2000 algorithms, draws a horizon chart with constellation guides and solar-system bodies, and supports observer/time changes, search, and object inspection. The Windows Release app has built and remained running in a launch smoke check; a human visual review and macOS/Linux launch checks are still pending. The complete plugin catalog is not ported.

The initial hosted CI run passed solution tests and self-contained packaging on Windows x64, Linux x64, macOS Intel, and macOS Apple silicon. A follow-up run for the latest chart-layer change is pending. CI verifies builds and packages; only Windows has had a local GUI launch check so far. Public release assets, before/after screenshots, and the measured migration report are still open.

## Project notes

- [Astrarium migration brief](docs/ASTRARIUM_MIGRATION_BRIEF.md) — upstream facts, licensing risks, port boundary, and migration decisions.
- [Astrarium delivery plan](docs/ASTRARIUM_PORT_PLAN.md) — criteria mapping, acceptance checks, and six-day execution schedule.
- [Data notices](docs/DATA_NOTICES.md) — separate catalog/source licenses and reproducible catalog provenance.
- [Challenge plan](docs/PORT_CHALLENGE_PLAN.md) — official rules and entry checklist.
- [Upstream Astrarium](https://github.com/Astrarium/Astrarium) — behavioral reference and original project.

## Verification so far

- `dotnet test Astrarium.Port.sln --configuration Release --no-restore`: 97 tests passed (92 retained Astrarium algorithm checks and 5 port-core checks), zero failures, zero warnings.
- `dotnet build src/Astrarium.Avalonia/Astrarium.Avalonia.csproj --configuration Release --no-restore`: Windows-host Release build succeeded with zero warnings.
- Hosted CI run [#39030662449](https://github.com/shobhit1kapoor/astrarium-avalonia-port-challenge/actions/runs/39030662449) passed tests and self-contained packaging on `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64` for the first public revision.
- Self-contained `dotnet publish` also succeeded locally for all four runtime identifiers.
- The packaged Windows x64 executable opened and remained responsive during a local launch smoke check. Linux/macOS launches and visual QA remain open.

This is a focused first port of Astrarium's observing workflow; it does not claim parity with the original plugin ecosystem.

## Challenge

The Avalonia Port Challenge closes **October 23, 2026 at 23:59 UTC** (**18:59 America/Chicago**). Entry requirements and judging criteria are tracked in `docs/PORT_CHALLENGE_PLAN.md` and must be checked again before submission.
