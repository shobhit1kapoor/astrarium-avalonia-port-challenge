#!/usr/bin/env python3
"""Create the compact, reproducible HYG v4.1 naked-eye catalog used by Astrarium."""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import urllib.request
from pathlib import Path


SOURCE_URL = (
    "https://raw.githubusercontent.com/astronexus/HYG-Database/"
    "3bf37f4b2d5460e1278286320d1d62fab9b493c1/"
    "hyg/CURRENT/hygdata_v41.csv"
)
SOURCE_SHA256 = "d9f69fd86bbf90a4e4d52b4c5c53eacfa6dfc0bfdef85bfd94f095e0bebe4ebd"


def read_source(path: Path) -> list[dict[str, object]]:
    digest = hashlib.sha256(path.read_bytes()).hexdigest()
    if digest != SOURCE_SHA256:
        raise SystemExit(
            f"Unexpected HYG source checksum {digest}; expected {SOURCE_SHA256}. "
            "Refusing to generate an unpinned catalog."
        )

    stars: list[dict[str, object]] = []
    with path.open(newline="", encoding="utf-8") as source:
        for row in csv.DictReader(source):
            try:
                magnitude = float(row["mag"])
                ra_hours = float(row["ra"])
                dec_degrees = float(row["dec"])
                distance = float(row["dist"])
                position = [float(row[field]) for field in ("x", "y", "z")]
                velocity = [float(row[field]) for field in ("vx", "vy", "vz")]
            except (KeyError, TypeError, ValueError):
                continue

            if magnitude > 6.0 or distance == 0.0 or not all(
                value == value and abs(value) != float("inf")
                for value in (magnitude, ra_hours, dec_degrees, distance, *position, *velocity)
            ):
                continue

            stars.append(
                {
                    "id": row["id"],
                    "hip": row["hip"],
                    "hr": row["hr"],
                    "hd": row["hd"],
                    "proper": row["proper"],
                    "designation": row["bf"],
                    "constellation": row["con"],
                    "raDegreesJ2000": round(ra_hours * 15.0, 7),
                    "decDegreesJ2000": round(dec_degrees, 7),
                    "magnitude": round(magnitude, 2),
                    "colorIndex": _round_optional(row["ci"], 3),
                    "positionParsecsJ2000": [round(value, 9) for value in position],
                    "velocityParsecsPerYear": [round(value, 12) for value in velocity],
                }
            )

    stars.sort(key=lambda star: int(str(star["id"])))
    return stars


def _round_optional(value: str, digits: int) -> float | None:
    try:
        number = float(value)
        return round(number, digits) if abs(number) != float("inf") else None
    except (TypeError, ValueError):
        return None


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "source",
        nargs="?",
        type=Path,
        default=Path("hygdata_v41.csv"),
        help="HYG v4.1 CSV; downloads the pinned source if missing",
    )
    parser.add_argument(
        "output",
        nargs="?",
        type=Path,
        default=Path("src/Astrarium.Port.Core/Data/stars.json"),
    )
    args = parser.parse_args()

    if not args.source.exists():
        args.source.parent.mkdir(parents=True, exist_ok=True)
        urllib.request.urlretrieve(SOURCE_URL, args.source)

    stars = read_source(args.source)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("w", encoding="utf-8", newline="\n") as output:
        json.dump(stars, output, ensure_ascii=False, separators=(",", ":"))
        output.write("\n")

    print(f"Wrote {len(stars):,} stars to {args.output} ({args.output.stat().st_size:,} bytes)")


if __name__ == "__main__":
    main()
