#!/usr/bin/env bash

# Builds a unit-vs-integration coverage PR comment from two ReportGenerator
# MarkdownSummaryGithub reports (one per test suite). The two suites are reported
# side by side instead of merged, so each module shows its unit and integration
# line/branch coverage in separate columns.
#
# Usage: coverage-comment.sh <unit-summary.md> <integration-summary.md> <output.md>

set -euo pipefail

UNIT_MD="${1:-coverage/report/unit/SummaryGithub.md}"
INT_MD="${2:-coverage/report/integration/SummaryGithub.md}"
OUT="${3:-coverage/comment.md}"

# Extracts the first percentage from a headline row (e.g. "Line coverage:").
headline() {
    grep -m1 -i "$2" "$1" 2>/dev/null | grep -oE '[0-9]+(\.[0-9]+)?%' | head -1
}

unit_line=$(headline "$UNIT_MD" "Line coverage:")
unit_branch=$(headline "$UNIT_MD" "Branch coverage:")
int_line=$(headline "$INT_MD" "Line coverage:")
int_branch=$(headline "$INT_MD" "Branch coverage:")

{
    echo ""
    echo "# Test Coverage (Unit vs Integration)"
    echo ""
    echo "| Metric | 🧪 Unit | 🔗 Integration |"
    echo "|:---|---:|---:|"
    echo "| Line coverage | ${unit_line:-—} | ${int_line:-—} |"
    echo "| Branch coverage | ${unit_branch:-—} | ${int_branch:-—} |"
    echo ""
    echo "## Coverage by module"
    echo ""
    echo "| Module | Unit · Line | Unit · Branch | Integration · Line | Integration · Branch |"
    echo "|:---|---:|---:|---:|---:|"

    # Join the per-assembly bold rows from both reports by module name. The bold
    # "Name" header rows are skipped; an empty branch cell becomes an em dash.
    awk '
        function clean(s) { gsub(/\*\*/, "", s); gsub(/^[ \t]+|[ \t]+$/, "", s); return s }
        function cell(v) { return (v == "" ? "—" : v) }

        FNR == NR {
            if ($0 ~ /^\|\*\*/ && $0 !~ /Name/) {
                split($0, a, "|"); m = clean(a[2])
                ul[m] = clean(a[3]); ub[m] = clean(a[4])
                if (!(m in seen)) { seen[m] = 1; order[++k] = m }
            }
            next
        }
        {
            if ($0 ~ /^\|\*\*/ && $0 !~ /Name/) {
                split($0, a, "|"); m = clean(a[2])
                il[m] = clean(a[3]); ib[m] = clean(a[4])
                if (!(m in seen)) { seen[m] = 1; order[++k] = m }
            }
        }
        END {
            for (i = 1; i <= k; i++) {
                m = order[i]
                printf "| %s | %s | %s | %s | %s |\n", m,
                    cell((m in ul) ? ul[m] : ""), cell((m in ub) ? ub[m] : ""),
                    cell((m in il) ? il[m] : ""), cell((m in ib) ? ib[m] : "")
            }
        }
    ' "$UNIT_MD" "$INT_MD"
} >> "$OUT"
