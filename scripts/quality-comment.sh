#!/usr/bin/env bash

# Builds the quality PR comment header. SonarQube Cloud decorates the pull request itself
# with inline findings and the quality gate, so this is a pointer to that rather than a copy
# of it — duplicating Sonar's output here would go stale the moment someone resolves an issue.
#
# The CRAP section is appended afterwards by `crap-comment.sh`.
#
# Usage: quality-comment.sh <sonar-result> <crap-result> <output.md>
#   <sonar-result|crap-result>  job results: success | failure | cancelled | skipped

set -euo pipefail

SONAR_RESULT="${1}"
CRAP_RESULT="${2}"
OUT="${3:-quality-comment.md}"

PROJECT="116media_116-backend"
DASHBOARD="https://sonarcloud.io/project/overview?id=${PROJECT}"

icon() {
    case "$1" in
        success) echo "✅" ;;
        failure) echo "❌" ;;
        cancelled) echo "⚪" ;;
        skipped) echo "⏭️" ;;
        *) echo "❔" ;;
    esac
}

case "$SONAR_RESULT" in
    success) NOTE="Analysis uploaded. The quality gate and inline findings are on the pull request." ;;
    failure) NOTE="Analysis failed — the dashboard still shows the last successful run, not this commit." ;;
    cancelled) NOTE="Analysis cancelled." ;;
    skipped) NOTE="Analysis skipped." ;;
    *) NOTE="Analysis state unknown." ;;
esac

{
    echo "## 📊 Quality"
    echo ""
    echo "| Analyser | Result | Covers |"
    echo "| --- | :---: | --- |"
    echo "| [SonarQube Cloud](${DASHBOARD}) | $(icon "$SONAR_RESULT") | maintainability, bugs, duplication, new-code gate |"
    echo "| CRAP | $(icon "$CRAP_RESULT") | complexity weighed against coverage, per method |"
    echo ""
    echo "${NOTE}"
} > "$OUT"

echo "Quality comment header written to $OUT"
