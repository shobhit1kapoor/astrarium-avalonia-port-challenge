# Astrarium for Avalonia

This repository contains a cross-platform Avalonia port of [Astrarium](https://github.com/Astrarium/Astrarium), the open-source desktop planetarium. The upstream app is a substantial Windows WPF/WinForms application; this project preserves its astronomical purpose and calculations while rebuilding the desktop experience for Windows, macOS, and Linux.

## Current status

The first Windows vertical slice now runs: it loads a licensed catalog of 5,070 naked-eye stars, computes local sky positions with Astrarium's VSOP87/ELP2000 algorithms, draws a horizon chart with constellation guides and solar-system bodies, and supports observer/time changes, search, and object inspection. The Windows Release app has built and remained running in a launch smoke check; a human visual review and macOS/Linux launch checks are still pending. The complete plugin catalog is not ported.

The immediate priority is to prove the same workflow on Windows, macOS, and Linux, then finish the observing experience, package clean downloads, capture before/after screenshots, and write the measured migration report. CI packaging is defined in [`.github/workflows/astrarium-port.yml`](.github/workflows/astrarium-port.yml); no CI run or public release is claimed yet.

## Project notes

- [Astrarium migration brief](docs/ASTRARIUM_MIGRATION_BRIEF.md) — upstream facts, licensing risks, port boundary, and migration decisions.
- [Astrarium delivery plan](docs/ASTRARIUM_PORT_PLAN.md) — criteria mapping, acceptance checks, and six-day execution schedule.
- [Data notices](docs/DATA_NOTICES.md) — separate catalog/source licenses and reproducible catalog provenance.
- [Challenge plan](docs/PORT_CHALLENGE_PLAN.md) — official rules and entry checklist.
- [Upstream Astrarium](https://github.com/Astrarium/Astrarium) — behavioral reference and original project.

## Verification so far

- `dotnet test Astrarium.Port.sln --configuration Release --no-restore`: 97 tests passed (92 retained Astrarium algorithm checks and 5 port-core checks), zero failures, zero warnings.
- `dotnet build src/Astrarium.Avalonia/Astrarium.Avalonia.csproj --configuration Release --no-restore`: Windows-host Release build succeeded with zero warnings.
- Self-contained `dotnet publish` succeeded locally for `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64`; this is a cross-compile check, not an OS launch check.
- The packaged Windows x64 executable opened and remained responsive during a local launch smoke check. Linux/macOS launches, visual QA, and CI runs remain open.

This is a focused first port of Astrarium's observing workflow; it does not claim parity with the original plugin ecosystem.

## Challenge

The Avalonia Port Challenge closes **October 23, 2026 at 23:59 UTC** (**18:59 America/Chicago**). Entry requirements and judging criteria are tracked in `docs/PORT_CHALLENGE_PLAN.md` and must be checked again before submission.
