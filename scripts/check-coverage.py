#!/usr/bin/env python3
"""Fail CI when Cobertura line or branch coverage falls below configured thresholds."""

from __future__ import annotations

import argparse
import glob
import sys
import xml.etree.ElementTree as ET


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--pattern", required=True, help="Glob for Cobertura XML files.")
    parser.add_argument("--min-line", type=float, required=True, help="Minimum line coverage percentage.")
    parser.add_argument("--min-branch", type=float, required=True, help="Minimum branch coverage percentage.")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    files = sorted(glob.glob(args.pattern, recursive=True))

    if not files:
        print(f"No coverage files matched: {args.pattern}", file=sys.stderr)
        return 1

    lines_covered = 0
    lines_valid = 0
    branches_covered = 0
    branches_valid = 0

    for path in files:
        root = ET.parse(path).getroot()
        lines_covered += int(root.attrib.get("lines-covered", "0"))
        lines_valid += int(root.attrib.get("lines-valid", "0"))
        branches_covered += int(root.attrib.get("branches-covered", "0"))
        branches_valid += int(root.attrib.get("branches-valid", "0"))

    line_percent = 100.0 if lines_valid == 0 else lines_covered / lines_valid * 100.0
    branch_percent = 100.0 if branches_valid == 0 else branches_covered / branches_valid * 100.0

    print(
        "Coverage: "
        f"lines={line_percent:.2f}% ({lines_covered}/{lines_valid}), "
        f"branches={branch_percent:.2f}% ({branches_covered}/{branches_valid})"
    )
    print(f"Required: lines>={args.min_line:.2f}%, branches>={args.min_branch:.2f}%")

    failures: list[str] = []
    if line_percent < args.min_line:
        failures.append(f"line coverage {line_percent:.2f}% is below {args.min_line:.2f}%")
    if branch_percent < args.min_branch:
        failures.append(f"branch coverage {branch_percent:.2f}% is below {args.min_branch:.2f}%")

    if failures:
        print("Coverage gate failed: " + "; ".join(failures), file=sys.stderr)
        return 1

    print("Coverage gate passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
