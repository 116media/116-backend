#!/usr/bin/env bash

# Appends the CRAP section to the quality PR comment, reading the report that
# `dotnet-crap analyze` produced. The metric itself is the tool's; this only selects which
# of its rows the comment shows and formats them.
#
# Scope is the production files the PR touched: a CRAP backlog of 40 methods is not
# actionable review feedback, whereas "the method you just edited is complex and
# uncovered" is. The full report ships as the `crap-report` artifact either way.
#
# Usage: crap-comment.sh <report.json> <changed-files.txt> <output.md>

set -euo pipefail

REPORT="${1:-crap/crap-report.json}"
CHANGED="${2:-}"
OUT="${3:-coverage/comment.md}"

if [[ ! -f "$REPORT" ]]; then
    echo "No CRAP report at $REPORT — skipping section" >&2
    exit 0
fi

python3 - "$REPORT" "$CHANGED" "$OUT" <<'PY'
import json
import pathlib
import sys

report_path, changed_path, out_path = sys.argv[1], sys.argv[2], sys.argv[3]

report = json.loads(pathlib.Path(report_path).read_text())
threshold = report.get("threshold", 30)
stats = report.get("stats", {})

changed = set()
if changed_path and pathlib.Path(changed_path).is_file():
    changed = {
        line.strip().replace("\\", "/")
        for line in pathlib.Path(changed_path).read_text().splitlines()
        if line.strip()
    }


def production(method):
    return "/tests/" not in (method.get("filePath") or "").replace("\\", "/")


def touched(method):
    if not changed:
        return True
    path = (method.get("filePath") or "").replace("\\", "/")
    return any(path.endswith(c) or c.endswith(path) for c in changed)


crappy = [m for m in report.get("methods", []) if m.get("isCrappy") and production(m)]
in_diff = [m for m in crappy if touched(m)]
in_diff.sort(key=lambda m: -m.get("crap", 0))

badge = {"critical": "🔴", "high": "🟠", "medium": "🟡"}

lines = [
    "",
    "### 🧪 CRAP",
    "",
    f"`{stats.get('methodCount', 0):,}` methods analysed · "
    f"average **{stats.get('averageCrap', 0):.1f}** · median **{stats.get('medianCrap', 0):.0f}** · "
    f"**{len(crappy)}** above the threshold of {threshold}.",
    "",
]

if not in_diff:
    lines += [
        "No method this PR touches exceeds the threshold. ✅",
        "",
    ]
else:
    lines += [
        f"#### {len(in_diff)} touched by this PR",
        "",
        "`CRAP = complexity² × (1 − coverage)³ + complexity` — high means **complex and",
        "under-tested**. Adding tests or simplifying both bring it down.",
        "",
        "| | CRAP | Method | Complexity | Coverage | File |",
        "| --- | ---: | --- | ---: | ---: | --- |",
    ]
    for method in in_diff[:15]:
        name = f"{method.get('className', '?')}.{method.get('methodName', '?')}".replace("|", "\\|")
        path = (method.get("filePath") or "").replace("\\", "/").split("/")[-1]
        line_no = method.get("lineNumber", "")
        lines.append(
            f"| {badge.get(method.get('severity'), '⚪')} "
            f"| {method.get('crap', 0):.0f} "
            f"| `{name}` "
            f"| {method.get('complexity', 0)} "
            f"| {method.get('coverage', 0):.0%} "
            f"| `{path}:{line_no}` |"
        )
    if len(in_diff) > 15:
        lines.append(f"| | … | _{len(in_diff) - 15} more_ | | | |")
    lines.append("")

lines += ["Full report: the **crap-report** artifact on this run.", ""]

with pathlib.Path(out_path).open("a", encoding="utf-8") as handle:
    handle.write("\n".join(lines))

print(f"CRAP section appended: {len(in_diff)} touched, {len(crappy)} total above {threshold}")
PY
