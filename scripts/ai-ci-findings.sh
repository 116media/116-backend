#!/usr/bin/env bash

# Pulls the security findings CodeQL and Trivy raised in CI into the terminal, so the
# scanners' output is available while writing code rather than only after a PR is open.
#
# Neither tool runs locally: the CodeQL CLI is a ~500MB download whose database build
# takes minutes across 39 projects, and Trivy's image scan needs a Docker build. Both
# publish SARIF to GitHub code scanning, so their findings are read back from there.
# `dotnet list package --vulnerable`, in `ai-precheck.sh`, is the part that does run
# locally and needs nothing installed.
#
# Usage: ai-ci-findings.sh [ref]
#   [ref]  branch or commit to report on (default: the current branch)

set -uo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.." || exit 1

REF="${1:-$(git rev-parse --abbrev-ref HEAD)}"
REPO="$(gh repo view --json nameWithOwner --jq .nameWithOwner 2>/dev/null)"

if [ -z "$REPO" ]; then
    echo "gh is not authenticated for this repository — run: gh auth login" >&2
    exit 1
fi

echo "Code-scanning alerts on ${REPO}@${REF}"
echo

ALERTS="$(mktemp)"
trap 'rm -f "$ALERTS"' EXIT

if ! gh api "repos/${REPO}/code-scanning/alerts?ref=${REF}&state=open&per_page=100" \
    > "$ALERTS" 2>/dev/null; then
    echo "No code-scanning data for this ref."
    echo "Either the Security workflow has not run on it, or code scanning is not enabled."
    exit 0
fi

python3 - "$ALERTS" <<'PY'
import json
import pathlib
import sys
from collections import Counter

alerts = json.loads(pathlib.Path(sys.argv[1]).read_text())
if not isinstance(alerts, list) or not alerts:
    print("No open alerts.")
    raise SystemExit(0)

by_tool = Counter((alert.get("tool") or {}).get("name", "?") for alert in alerts)
print(f"{len(alerts)} open: " + ", ".join(f"{count} from {name}" for name, count in by_tool.items()))
print()

ORDER = {"critical": 0, "high": 1, "error": 1, "medium": 2, "warning": 3, "low": 4}


def severity(alert):
    rule = alert.get("rule") or {}
    return rule.get("security_severity_level") or rule.get("severity") or "unknown"


for alert in sorted(alerts, key=lambda a: ORDER.get(severity(a), 9)):
    rule = alert.get("rule") or {}
    instance = alert.get("most_recent_instance") or {}
    location = instance.get("location") or {}
    path = location.get("path", "")
    line = location.get("start_line", "")
    where = f"{path}:{line}" if path else "—"
    print(f"[{severity(alert):<8}] {(alert.get('tool') or {}).get('name', '?')}  {rule.get('id', '?')}")
    print(f"           {rule.get('description') or ''}")
    print(f"           {where}")
    print()
PY
