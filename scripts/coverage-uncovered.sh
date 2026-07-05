#!/usr/bin/env bash

# Derives two file-level lists from the per-suite cobertura reports and appends
# them to the coverage PR comment:
#
#   - files changed in this PR NOT fully covered by unit tests
#   - files changed in this PR NOT fully covered by integration tests
#
# Codecov's merged view cannot show this — a line covered by either suite reads
# as covered — so the two suites are read separately here, straight from the
# cobertura XML each suite uploads. A file is listed for suite X when it has
# coverable lines and suite X leaves at least one of them unhit (line coverage
# below 100%), with its per-suite percentage shown next to it.
#
# Scope is restricted to the files added/modified by the PR when a changed-files
# list is supplied (fourth argument), so the comment mirrors the PR diff instead
# of the whole repository. Ignored paths (mirroring codecov.yml `ignore`) are
# never listed.
#
# Usage: coverage-uncovered.sh <unit-dir> <integration-dir> <output.md> [changed-files.txt]
#   <unit-dir> / <integration-dir> are searched recursively for
#   coverage.cobertura.xml. Output is APPENDED to <output.md>.
#   [changed-files.txt] is a newline-separated list of repo-relative paths; when
#   omitted or empty every covered source file is considered.

set -euo pipefail

UNIT_DIR="${1:-coverage/unit}"
INT_DIR="${2:-coverage/integration}"
OUT="${3:-coverage/comment.md}"
CHANGED="${4:-}"

python3 - "$UNIT_DIR" "$INT_DIR" "$OUT" "$CHANGED" <<'PY'
import sys, glob, os
import xml.etree.ElementTree as ET

unit_dir, int_dir, out, changed_path = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]


def normalize(filename):
    """Reduce any absolute/CI path to a repo-relative 'src/...' path."""
    path = filename.replace("\\", "/").strip()
    marker = path.find("/src/")
    if marker != -1:
        return path[marker + 1:]
    return path


def is_ignored(name):
    """Paths excluded from coverage reporting (mirrors codecov.yml `ignore`)."""
    slashed = "/" + name
    if "/Migrations/" in slashed or "/tests/" in slashed:
        return True
    if name.endswith(".Designer.cs") or name.endswith("Program.cs"):
        return True
    return False


def collect(directory):
    """Map each source file to (covered_line_count, coverable_line_count)."""
    covered, total = {}, {}
    pattern = os.path.join(directory, "**", "coverage.cobertura.xml")
    for report in glob.glob(pattern, recursive=True):
        try:
            root = ET.parse(report).getroot()
        except ET.ParseError:
            continue
        for cls in root.iter("class"):
            name = normalize(cls.get("filename", ""))
            if not name.startswith("src/") or is_ignored(name):
                continue
            lines = cls.find("lines")
            if lines is None:
                continue
            hit = sum(1 for ln in lines.findall("line") if int(ln.get("hits", "0")) > 0)
            count = len(lines.findall("line"))
            covered[name] = covered.get(name, 0) + hit
            total[name] = total.get(name, 0) + count
    return covered, total


# The set of PR-changed source files, when supplied. None means "no scope".
changed = None
if changed_path and os.path.exists(changed_path):
    changed = set()
    with open(changed_path, encoding="utf-8") as handle:
        for line in handle:
            name = normalize(line)
            if name.startswith("src/") and not is_ignored(name):
                changed.add(name)

unit_covered, unit_total = collect(unit_dir)
int_covered, int_total = collect(int_dir)

all_total = {}
for src in (unit_total, int_total):
    for name, count in src.items():
        all_total[name] = max(all_total.get(name, 0), count)

files = [name for name, count in all_total.items() if count > 0]
if changed is not None:
    files = [name for name in files if name in changed]


def gaps(covered):
    """Files below 100% line coverage for a suite, worst first, as (file, covered, total)."""
    rows = []
    for name in files:
        total = all_total[name]
        hit = covered.get(name, 0)
        if hit < total:
            rows.append((name, hit, total))
    rows.sort(key=lambda r: (r[1] / r[2], r[0]))
    return rows


scoped = changed is not None
not_unit = gaps(unit_covered)
not_int = gaps(int_covered)


def section(title, rows):
    subject = "changed source file" if scoped else "source file"
    out_lines = [f"\n### {title} ({len(rows)})\n\n"]
    if not rows:
        out_lines.append(f"_None — every {subject} is fully covered by this suite._\n")
    else:
        out_lines.append("<details><summary>Show files</summary>\n\n")
        for name, hit, total in rows:
            pct = 100.0 * hit / total
            out_lines.append(f"- `{name}` — {pct:.1f}% ({hit}/{total} lines)\n")
        out_lines.append("\n</details>\n")
    return "".join(out_lines)


heading = "Changed files below 100% coverage, per suite" if scoped else "Files below 100% coverage, per suite"
md = f"\n## {heading}\n"
if scoped and not files:
    md += "\n_No covered source files changed in this PR._\n"
else:
    md += "\nCovered by the *other* suite does not count here — each list is that suite alone.\n"
    md += section("⚠️ Not fully covered by **unit** tests", not_unit)
    md += section("⚠️ Not fully covered by **integration** tests", not_int)

with open(out, "a", encoding="utf-8") as handle:
    handle.write(md)

print(f"scoped={scoped} below-100-unit={len(not_unit)} below-100-integration={len(not_int)}")
PY
