#!/usr/bin/env bash

# Builds the security PR comment: scanner outcomes, then the findings they raised against
# this commit.
#
# CodeQL and Trivy both publish SARIF to GitHub code scanning, so alerts are read back
# from the code-scanning API rather than re-parsed — the comment then shows exactly what
# the Security tab shows, including anything a reviewer has dismissed.
#
# Usage: security-comment.sh <repo> <ref> <codeql> <trivy> <sarif-dir> <output.md>
#   <ref>           a full git ref such as refs/heads/develop or refs/pull/12/merge.
#                   The code-scanning API rejects a bare commit SHA and answers with an
#                   empty list, which reads as a clean bill of health.
#   <codeql|trivy>  job results: success | failure | cancelled | skipped
#   <sarif-dir>     directory holding the downloaded trivy-*.sarif, may be absent

set -euo pipefail

REPO="${1}"
REF="${2}"
CODEQL_RESULT="${3}"
TRIVY_RESULT="${4}"
SARIF_DIR="${5:-sarif}"
OUT="${6:-security-comment.md}"

icon() {
    case "$1" in
        success) echo "✅" ;;
        failure) echo "❌" ;;
        cancelled) echo "⚪" ;;
        skipped) echo "⏭️" ;;
        *) echo "❔" ;;
    esac
}

{
    echo "## 🔒 Security"
    echo ""
    echo "| Scanner | Result | Covers |"
    echo "| --- | :---: | --- |"
    echo "| [CodeQL](https://github.com/${REPO}/security/code-scanning?query=tool%3ACodeQL) | $(icon "$CODEQL_RESULT") | taint analysis of this repository's C# |"
    echo "| [Trivy](https://github.com/${REPO}/security/code-scanning?query=tool%3ATrivy) | $(icon "$TRIVY_RESULT") | image CVEs, OS packages, Dockerfile config |"
    echo ""
} > "$OUT"

# ---------------------------------------------------------------- code scanning alerts
ALERTS_JSON="$(mktemp)"
if gh api "repos/${REPO}/code-scanning/alerts?ref=${REF}&state=open&per_page=100" \
    > "$ALERTS_JSON" 2>/dev/null; then
    python3 - "$ALERTS_JSON" "$OUT" "$REPO" "$REF" <<'PY'
import json
import pathlib
import sys
from collections import Counter

alerts_path, out_path, repo, ref = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]

try:
    alerts = json.loads(pathlib.Path(alerts_path).read_text())
except json.JSONDecodeError:
    alerts = []

if not isinstance(alerts, list):
    alerts = []

lines = []
if not alerts:
    lines += [f"No open code-scanning alerts for `{ref}`. ✅", "",
              "If a scanner has not analysed this ref yet, that shows as zero here too.", ""]
else:
    by_severity = Counter(
        (alert.get("rule") or {}).get("security_severity_level")
        or (alert.get("rule") or {}).get("severity")
        or "unknown"
        for alert in alerts
    )
    summary = ", ".join(f"**{count} {name}**" for name, count in by_severity.most_common())
    lines += [f"### Open alerts ({len(alerts)}) — {summary}", ""]
    lines += ["| Severity | Tool | Rule | Location |", "| --- | --- | --- | --- |"]

    def rank(alert):
        order = {"critical": 0, "high": 1, "error": 1, "medium": 2, "warning": 3, "low": 4}
        rule = alert.get("rule") or {}
        key = rule.get("security_severity_level") or rule.get("severity") or "zzz"
        return order.get(key, 9)

    for alert in sorted(alerts, key=rank)[:15]:
        rule = alert.get("rule") or {}
        severity = rule.get("security_severity_level") or rule.get("severity") or "?"
        tool = (alert.get("tool") or {}).get("name", "?")
        description = (rule.get("description") or rule.get("id") or "?").replace("|", "\\|")
        instance = alert.get("most_recent_instance") or {}
        location = (instance.get("location") or {}).get("path", "")
        line = (instance.get("location") or {}).get("start_line", "")
        where = f"`{location.split('/')[-1]}:{line}`" if location else "—"
        url = alert.get("html_url", "")
        link = f"[{description[:70]}]({url})" if url else description[:70]
        lines.append(f"| {severity} | {tool} | {link} | {where} |")

    if len(alerts) > 15:
        lines.append(f"| … | | _{len(alerts) - 15} more_ | |")
    lines += ["", f"Full list: [Security tab](https://github.com/{repo}/security/code-scanning)", ""]

with pathlib.Path(out_path).open("a", encoding="utf-8") as handle:
    handle.write("\n".join(lines))
PY
else
    {
        echo "_Code-scanning alerts unavailable for this run._"
        echo ""
    } >> "$OUT"
fi
rm -f "$ALERTS_JSON"

# ---------------------------------------------------------------- trivy, read locally
# Image CVEs are reported against the base image rather than a file in the diff, so they
# do not always surface as code-scanning alerts on the PR head. Read them from the SARIF.
if compgen -G "${SARIF_DIR}/trivy-image.sarif" > /dev/null; then
    python3 - "${SARIF_DIR}/trivy-image.sarif" "$OUT" <<'PY'
import json
import pathlib
import sys

sarif_path, out_path = sys.argv[1], sys.argv[2]

try:
    sarif = json.loads(pathlib.Path(sarif_path).read_text())
except (json.JSONDecodeError, OSError):
    sys.exit(0)

results = [r for run in sarif.get("runs", []) for r in run.get("results", [])]

lines = ["### Container image", ""]
if not results:
    lines += ["No HIGH or CRITICAL findings with a fix available. ✅", ""]
else:
    lines += [
        f"**{len(results)}** HIGH/CRITICAL finding(s) with a fix available.",
        "",
        "| Package | Vulnerability |",
        "| --- | --- |",
    ]
    seen = set()
    for result in results[:10]:
        rule = result.get("ruleId", "?")
        message = (result.get("message") or {}).get("text", "").split("\n")[0]
        if rule in seen:
            continue
        seen.add(rule)
        lines.append(f"| `{message[:60].replace('|', '')}` | {rule} |")
    if len(results) > 10:
        lines.append(f"| … | _{len(results) - 10} more_ |")
    lines.append("")

with pathlib.Path(out_path).open("a", encoding="utf-8") as handle:
    handle.write("\n".join(lines))
PY
fi

echo "Security comment written to $OUT"
