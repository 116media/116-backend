#!/usr/bin/env bash

# Writes the Codecov-style report header + patch verdict to the top of the
# coverage PR comment (TRUNCATING it), so the single comment we own opens with
# the same block Codecov would post:
#
#   ## [Codecov](...) Report
#   ✅ All modified and coverable lines are covered by tests.   (or a ⚠️ list)
#
# The verdict is patch coverage of the union of both suites: it inspects the
# lines added/modified by the PR (git diff, + side) and flags any that are
# coverable (tracked in cobertura) yet hit by neither unit nor integration
# tests. Ignored paths (mirroring codecov.yml `ignore`) never count.
#
# Usage: coverage-header.sh <pr-number> <repo-slug> <unit-dir> <integration-dir> <diff-u0.txt> <output.md>

set -euo pipefail

PR="${1:-0}"
REPO="${2:-}"
UNIT_DIR="${3:-coverage/unit}"
INT_DIR="${4:-coverage/integration}"
DIFF="${5:-coverage/diff-u0.txt}"
OUT="${6:-coverage/comment.md}"

python3 - "$PR" "$REPO" "$UNIT_DIR" "$INT_DIR" "$DIFF" "$OUT" <<'PY'
import sys, glob, os, re
import xml.etree.ElementTree as ET

pr, repo, unit_dir, int_dir, diff_path, out = sys.argv[1:7]


def normalize(filename):
    path = filename.replace("\\", "/").strip()
    marker = path.find("/src/")
    return path[marker + 1:] if marker != -1 else path


def is_ignored(name):
    slashed = "/" + name
    if "/Migrations/" in slashed or "/tests/" in slashed:
        return True
    return name.endswith(".Designer.cs") or name.endswith("Program.cs")


def union_hits(directories):
    """Map (file, line-number) to combined hit count across both suites."""
    hits = {}
    for directory in directories:
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
                for ln in lines.findall("line"):
                    key = (name, int(ln.get("number", "0")))
                    hits[key] = hits.get(key, 0) + int(ln.get("hits", "0"))
    return hits


hits = union_hits([unit_dir, int_dir])

# Added/modified lines from the diff (the '+' side of each hunk).
changed = {}
current = None
hunk = re.compile(r"^@@ -\d+(?:,\d+)? \+(\d+)(?:,(\d+))? @@")
if os.path.exists(diff_path):
    with open(diff_path, encoding="utf-8", errors="replace") as handle:
        for line in handle:
            if line.startswith("+++ b/"):
                current = normalize(line[6:])
            elif line.startswith("@@") and current:
                match = hunk.match(line)
                if match:
                    start = int(match.group(1))
                    count = int(match.group(2) or "1")
                    for number in range(start, start + count):
                        changed.setdefault(current, set()).add(number)

# A modified line is uncovered when it is coverable (present in cobertura) and
# the union of both suites never hit it.
uncovered = []
for name, numbers in changed.items():
    if not name.startswith("src/") or is_ignored(name):
        continue
    for number in sorted(numbers):
        key = (name, number)
        if key in hits and hits[key] == 0:
            uncovered.append((name, number))
uncovered.sort()

link = f"https://app.codecov.io/gh/{repo}/pull/{pr}?dropdown=coverage&src=pr&el=h1"
out_lines = [f"## [Codecov]({link}) Report\n"]
if not uncovered:
    out_lines.append(":white_check_mark: All modified and coverable lines are covered by tests.\n")
else:
    out_lines.append(f":warning: {len(uncovered)} modified line(s) are not covered by tests.\n\n")
    out_lines.append("<details><summary>Uncovered modified lines</summary>\n\n")
    out_lines.extend(f"- `{name}:{number}`\n" for name, number in uncovered)
    out_lines.append("\n</details>\n")

with open(out, "w", encoding="utf-8") as handle:
    handle.write("".join(out_lines))

print(f"uncovered-modified={len(uncovered)}")
PY
