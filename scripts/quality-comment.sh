#!/usr/bin/env bash

# Builds the quality PR comment header. SonarQube Cloud runs as Automatic Analysis and
# decorates the pull request itself, so this points at it rather than copying its findings.
#
# The CRAP section is appended afterwards by `crap-comment.sh`.
#
# Usage: quality-comment.sh <crap-result> <output.md>
#   <crap-result>  job result: success | failure | cancelled | skipped

set -euo pipefail

CRAP_RESULT="${1}"
OUT="${2:-quality-comment.md}"

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

{
    echo "## 📊 Quality"
    echo ""
    echo "| Analyser | Result | Covers |"
    echo "| --- | :---: | --- |"
    echo "| CRAP | $(icon "$CRAP_RESULT") | complexity weighed against coverage, per method |"
    echo ""
    echo "Maintainability, bugs and duplication are reported by [SonarQube Cloud](${DASHBOARD}),"
    echo "which analyses this repository automatically and decorates the pull request itself."
} > "$OUT"

echo "Quality comment header written to $OUT"
