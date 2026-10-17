#!/usr/bin/env python3
"""Build Astrarium's curated bright deep-sky catalog from pinned OpenNGC data."""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import math
import urllib.request
from pathlib import Path


SOURCE_COMMIT = "36cb178a0f69dba8bfc03a99c10512831edf1c6b"
SOURCE_BASE_URL = f"https://raw.githubusercontent.com/mattiaverga/OpenNGC/{SOURCE_COMMIT}"
SOURCES = {
    "NGC.csv": {
        "url": f"{SOURCE_BASE_URL}/database_files/NGC.csv",
        "sha256": "840fe0c9ee1332e551b2e722a0e92726cd7b157914a3d2177602832aadd3aa9e",
    },
    "addendum.csv": {
        "url": f"{SOURCE_BASE_URL}/database_files/addendum.csv",
        "sha256": "1d8f0914e643ada325a5a94d88d8fefad6a4937a2f77cc34f21483af22b11983",
    },
}
OBJECT_TYPES = {
    "G": "Galaxy",
    "GPair": "Galaxy pair",
    "GTrpl": "Galaxy triplet",
    "GGroup": "Galaxy group",
    "OCl": "Open cluster",
    "GCl": "Globular cluster",
    "PN": "Planetary nebula",
    "Neb": "Nebula",
    "HII": "H II region",
    "DrkN": "Dark nebula",
    "RfN": "Reflection nebula",
    "SNR": "Supernova remnant",
    "EmN": "Emission nebula",
    "Cl+N": "Cluster with nebula",
    "*Ass": "Stellar association",
}
MAGNITUDE_LIMIT = 10.0


def source_path(directory: Path, filename: str) -> Path:
    path = directory / filename
    if not path.exists():
        path.parent.mkdir(parents=True, exist_ok=True)
        urllib.request.urlretrieve(SOURCES[filename]["url"], path)

    actual_digest = hashlib.sha256(path.read_bytes()).hexdigest()
    expected_digest = SOURCES[filename]["sha256"]
    if actual_digest != expected_digest:
        raise SystemExit(
            f"Unexpected OpenNGC {filename} checksum {actual_digest}; "
            f"expected {expected_digest}. Refusing to generate an unpinned catalog."
        )
    return path


def read_rows(path: Path) -> list[dict[str, str]]:
    with path.open(newline="", encoding="utf-8-sig") as source:
        return list(csv.DictReader(source, delimiter=";"))


def parse_angle(value: str, is_right_ascension: bool) -> float:
    parts = value.strip().split(":")
    if len(parts) != 3:
        raise ValueError(f"Invalid sexagesimal coordinate: {value!r}")

    first, minutes, seconds = (float(part) for part in parts)
    sign = -1.0 if first < 0 or value.strip().startswith("-") else 1.0
    degrees = abs(first) + minutes / 60.0 + seconds / 3600.0
    if is_right_ascension:
        degrees *= 15.0
    else:
        degrees *= sign
    if not math.isfinite(degrees):
        raise ValueError(f"Non-finite coordinate: {value!r}")
    return degrees


def parse_optional_number(value: str) -> float | None:
    try:
        number = float(value.strip())
        return number if math.isfinite(number) else None
    except (AttributeError, ValueError):
        return None


def parse_messier(value: str) -> int | None:
    try:
        number = int(value.strip())
        return number if number > 0 else None
    except (AttributeError, ValueError):
        return None


def catalog_label(identifier: str) -> str:
    identifier = identifier.strip()
    for prefix in ("NGC", "IC"):
        if identifier.startswith(prefix):
            suffix = identifier[len(prefix) :]
            digits = "".join(character for character in suffix if character.isdigit())
            remainder = suffix[len(digits) :]
            if digits:
                return f"{prefix} {int(digits)}{remainder}"
    return identifier


def to_object(row: dict[str, str]) -> dict[str, object] | None:
    object_type = OBJECT_TYPES.get(row.get("Type", "").strip())
    magnitude = parse_optional_number(row.get("V-Mag", ""))
    messier = parse_messier(row.get("M", ""))
    identifier = row.get("Name", "").strip()
    if not object_type or magnitude is None or not identifier:
        return None
    if magnitude > MAGNITUDE_LIMIT and messier is None:
        return None

    try:
        right_ascension = parse_angle(row["RA"], is_right_ascension=True)
        declination = parse_angle(row["Dec"], is_right_ascension=False)
    except (KeyError, ValueError):
        return None
    if not 0 <= right_ascension < 360 or not -90 <= declination <= 90:
        return None

    catalog_name = catalog_label(identifier)
    common_names = [
        name.strip()
        for name in row.get("Common names", "").split(",")
        if name.strip()
    ]
    aliases: list[str] = []
    if messier is not None:
        aliases.extend((f"M {messier}", f"M{messier}"))
    aliases.append(catalog_name)
    aliases.append(catalog_name.replace(" ", ""))
    aliases.extend(common_names)
    identifiers = [
        value.strip()
        for value in row.get("Identifiers", "").split(",")[:4]
        if value.strip()
    ]
    aliases.extend(identifiers)
    aliases = list(dict.fromkeys(aliases))

    major_axis = parse_optional_number(row.get("MajAx", ""))
    minor_axis = parse_optional_number(row.get("MinAx", ""))
    return {
        "id": f"openngc:{identifier}",
        "name": common_names[0] if common_names else (f"M {messier}" if messier is not None else catalog_name),
        "catalogName": catalog_name,
        "designation": " · ".join(aliases),
        "objectType": object_type,
        "rightAscensionDegreesJ2000": round(right_ascension, 7),
        "declinationDegreesJ2000": round(declination, 7),
        "magnitude": round(magnitude, 2),
        "majorAxisArcMinutes": round(major_axis, 2) if major_axis is not None else None,
        "minorAxisArcMinutes": round(minor_axis, 2) if minor_axis is not None else None,
        "constellation": row.get("Const", "").strip(),
    }


def build_catalog(source_directory: Path) -> list[dict[str, object]]:
    entries: dict[str, dict[str, object]] = {}
    for filename in SOURCES:
        for row in read_rows(source_path(source_directory, filename)):
            entry = to_object(row)
            if entry is not None:
                entries[str(entry["id"])] = entry
    return sorted(entries.values(), key=lambda item: str(item["catalogName"]).casefold())


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--source-directory",
        type=Path,
        default=Path("data/openngc"),
        help="directory for the pinned OpenNGC CSV inputs (downloaded if missing)",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("src/Astrarium.Port.Core/Data/deepsky.json"),
    )
    args = parser.parse_args()

    catalog = build_catalog(args.source_directory)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("w", encoding="utf-8", newline="\n") as output:
        json.dump(catalog, output, ensure_ascii=False, separators=(",", ":"))
        output.write("\n")
    print(
        f"Wrote {len(catalog):,} deep-sky objects to {args.output} "
        f"({args.output.stat().st_size:,} bytes)"
    )


if __name__ == "__main__":
    main()
