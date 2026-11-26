#!/usr/bin/env bash

# Pre-commit quality and security gate, shared by the agent workflow and by hand.
#
# Three checks, all local and all fast enough to sit in front of every commit:
#   format  csharpier, on the changed files only
#   crap    crap4dotnet, solution-wide then filtered to the changed files
#   deps    `dotnet list package --vulnerable`, the dependency half of what Trivy
#           scans in CI — the image half needs a Docker build and stays in CI
#
# CodeQL is deliberately absent: its CLI is a ~500MB download and building a
# database across 39 projects takes minutes. Read its findings from the last CI
# run with `scripts/ai-ci-findings.sh` instead.
#
# Usage: ai-precheck.sh [--baseline] [--all] [--staged]
#   --baseline  write crap/baseline.json and exit, for before/after refactors
#   --all       check the whole tree rather than the changed files
#   --staged    compare against the index rather than the working tree
#   --force-deps  run the transitive package audit even when no package file changed
#   --file FILE   check that one file only, and stay silent unless it breaches the
#                 threshold — cheap enough to run on every edit

set -uo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.." || exit 1

SOLUTION="./116_backend.sln"
CRAP_DIR="crap"
BASELINE="${CRAP_DIR}/baseline.json"
CURRENT="${CRAP_DIR}/current.json"

MODE="changed"
DIFF_BASE="worktree"
WANT_BASELINE=false
FORCE_DEPS=false
SINGLE_FILE=""

while [ $# -gt 0 ]; do
    case "$1" in
        --baseline) WANT_BASELINE=true ;;
        --all) MODE="all" ;;
        --force-deps) FORCE_DEPS=true ;;
        --file) shift; SINGLE_FILE="${1:-}" ;;
        --staged) DIFF_BASE="staged" ;;
        *) echo "unknown argument: $1" >&2 ; exit 2 ;;
    esac
    shift
done

RED=$'\033[0;31m'; GREEN=$'\033[0;32m'; YELLOW=$'\033[1;33m'; NC=$'\033[0m'
FAILED=0

# ------------------------------------------------------------- single-file mode
# Runs against one file after an edit. Reports only methods over the threshold, so a
# clean edit produces no output at all.
if [ -n "$SINGLE_FILE" ]; then
    COV_ARGS=()
    for suite in unit integration e2e; do
        while IFS= read -r cov; do
            [ -n "$cov" ] && COV_ARGS+=(--coverage "$cov")
        done < <(find "coverage/$suite" -name 'coverage.cobertura.xml' 2>/dev/null)
    done
    if [ ${#COV_ARGS[@]} -eq 0 ]; then exit 0; fi

    SINGLE_REPORT="$(mktemp)"
    trap 'rm -f "$SINGLE_REPORT"' EXIT
    dotnet tool run dotnet-crap -- analyze "$SINGLE_FILE" \
        "${COV_ARGS[@]}" --min-crap 30 --output "$SINGLE_REPORT" > /dev/null 2>&1

    python3 - "$SINGLE_REPORT" <<'PY'
import json
import pathlib
import sys

try:
    methods = json.loads(pathlib.Path(sys.argv[1]).read_text()).get("methods", [])
except (json.JSONDecodeError, OSError):
    sys.exit(0)

crappy = [method for method in methods if method.get("isCrappy")]
if not crappy:
    sys.exit(0)

print(f"CRAP threshold breached in {crappy[0].get('filePath')}:")
for method in sorted(crappy, key=lambda m: -m.get("crap", 0)):
    coverage = method.get("coverage", 0) * 100
    print(f"  CRAP {method.get('crap'):.1f} (complexity {method.get('complexity')}, "
          f"coverage {coverage:.0f}%) {method.get('methodName')} "
          f"at line {method.get('lineNumber')}")
print("Raise coverage with targeted unit tests, or split the method. Above complexity")
print("30 no amount of tests brings CRAP under the threshold.")
PY
    exit 0
fi


changed_files() {
    if [ "$MODE" = "all" ]; then
        git ls-files '*.cs'
        return
    fi
    {
        if [ "$DIFF_BASE" = "staged" ]; then
            git diff --staged --name-only --diff-filter=ACM -- '*.cs'
        else
            git diff --name-only --diff-filter=ACM HEAD -- '*.cs'
            git ls-files --others --exclude-standard -- '*.cs'
        fi
    } | sort -u
}

# Every test project writes its own report, so one file per suite covers only that
# project. Passing a subset reads as uncovered everywhere else and invents breaches.
suite_coverage() {
    find "coverage/$1" -name 'coverage.cobertura.xml' 2>/dev/null
}

# CRAP is complexity weighed against coverage, so coverage older than the source it
# describes produces numbers that look like findings but are not. Refuse to score those
# rather than report them.
coverage_is_stale() {
    local newest_cov newest_src
    newest_cov="$(find coverage -name 'coverage.cobertura.xml' -newer "$1" -print -quit 2>/dev/null)"
    [ -z "$newest_cov" ]
}

newest_source() {
    find src -name '*.cs' -not -path '*/obj/*' -not -path '*/bin/*' -print0 2>/dev/null \
        | xargs -0 ls -t 2>/dev/null | head -1
}

FILES="$(changed_files)"
FILE_COUNT=$(printf '%s' "$FILES" | grep -c . || true)

# ------------------------------------------------------------------ formatting
echo "▸ format"
if [ "$FILE_COUNT" -eq 0 ]; then
    echo "  no C# files changed"
elif ! printf '%s\n' "$FILES" | xargs dotnet csharpier check > /tmp/csharpier-precheck.log 2>&1; then
    echo "${RED}  formatting violations${NC}"
    sed -n '1,20p' /tmp/csharpier-precheck.log | sed 's/^/    /'
    echo "    fix: dotnet csharpier format ."
    FAILED=1
else
    echo "${GREEN}  ${FILE_COUNT} file(s) formatted correctly${NC}"
fi

# ------------------------------------------------------------------------ crap
echo "▸ crap"
mkdir -p "$CRAP_DIR"

COV_ARGS=()
for suite in unit integration e2e; do
    while IFS= read -r cov; do
        [ -n "$cov" ] && COV_ARGS+=(--coverage "$cov")
    done < <(suite_coverage "$suite")
done

NEWEST_SRC="$(newest_source)"

if [ ${#COV_ARGS[@]} -eq 0 ]; then
    echo "${YELLOW}  no local coverage — run scripts/run-tests-with-coverage.sh first${NC}"
    echo "  (without coverage every method reads as uncovered, so CRAP is meaningless)"
    FAILED=1
elif [ -n "$NEWEST_SRC" ] && coverage_is_stale "$NEWEST_SRC"; then
    echo "${RED}  coverage is older than ${NEWEST_SRC}${NC}"
    echo "  Scoring against it invents breaches. Run scripts/run-tests-with-coverage.sh all"
    FAILED=1
else
    dotnet tool run dotnet-crap -- analyze "$SOLUTION" \
        "${COV_ARGS[@]}" --output "$CURRENT" > /dev/null 2>&1

    if [ "$WANT_BASELINE" = true ]; then
        cp "$CURRENT" "$BASELINE"
        echo "${GREEN}  baseline written to ${BASELINE}${NC}"
        exit 0
    fi

    # The tool's own exit code covers the whole solution, including the debt that
    # predates this change. Only methods in the changed files are blocking here.
    if ! CHANGED_FILES="$FILES" python3 - "$CURRENT" <<'PY'
import json
import os
import pathlib
import sys

RED, YELLOW, NC = "\033[0;31m", "\033[1;33m", "\033[0m"

report = json.loads(pathlib.Path(sys.argv[1]).read_text())
touched = {line.strip() for line in os.environ["CHANGED_FILES"].splitlines() if line.strip()}

for warning in report.get("warnings", []):
    message = (warning.get("message") or "").split(":")[0]
    print(f"  {YELLOW}{warning.get('code')}{NC} {message[:90]}")

stats = report.get("stats", {})
crappy = [
    method
    for method in report.get("methods", [])
    if method.get("isCrappy")
    and not any(part in (method.get("filePath") or "") for part in ("/tests/", "/Migrations/"))
]
in_diff = [
    method
    for method in crappy
    if any((method.get("filePath") or "").endswith(path) for path in touched)
]

print(f"  {stats.get('methodCount', 0)} methods, "
      f"{len(crappy)} over threshold {report.get('threshold')} solution-wide, "
      f"{len(in_diff)} in this change")

if not in_diff:
    sys.exit(0)

print(f"{RED}  CRAP threshold breached by this change:{NC}")
for method in sorted(in_diff, key=lambda m: -m.get("crap", 0)):
    print(f"    CRAP {method.get('crap'):>6.1f}  cx={method.get('complexity')}  "
          f"cov={method.get('coverage', 0) * 100:.0f}%  {method.get('methodName')}")
    print(f"             {method.get('filePath')}:{method.get('lineNumber')}")
print("    raise coverage, or split the method — above complexity 30 no amount")
print("    of tests brings CRAP under the threshold")
sys.exit(1)
PY
    then
        FAILED=1
    fi

    if [ -f "$BASELINE" ]; then
        dotnet tool run dotnet-crap -- diff "$BASELINE" "$CURRENT" \
            --output "${CRAP_DIR}/diff.json" > /dev/null 2>&1
        python3 - "${CRAP_DIR}/diff.json" <<'PY'
import json
import pathlib
import sys

summary = json.loads(pathlib.Path(sys.argv[1]).read_text()).get("summary", {})
load = summary.get("totalCrapLoadAfter", 0) - summary.get("totalCrapLoadBefore", 0)
direction = "down" if load < 0 else ("up" if load > 0 else "flat")
print(f"  vs baseline: crap load {direction} {load:+.1f}, "
      f"{summary.get('newCrappy', 0)} newly crappy, "
      f"{summary.get('fixedCrappy', 0)} fixed")
PY
    fi
fi

# ------------------------------------------------------------- vulnerable deps
# The transitive audit restores the whole solution and costs ~17s, so it only runs
# when something that can change the dependency graph was touched. `--force-deps`
# runs it regardless.
echo "▸ deps"

PACKAGE_FILES="$(git diff --name-only HEAD -- '*.csproj' 'Directory.Packages.props' 'Directory.Build.props' | head -1)"

if [ "$MODE" != "all" ] && [ "$FORCE_DEPS" = false ] && [ -z "$PACKAGE_FILES" ]; then
    echo "  skipped — no project or package file changed (--force-deps to run anyway)"
else
    VULN_LOG=/tmp/vulnerable-precheck.log
    if dotnet list "$SOLUTION" package --vulnerable --include-transitive \
        > "$VULN_LOG" 2>&1; then
        # Every project reports the same transitive package, so collapse to one row each.
        if ! python3 - "$VULN_LOG" <<'PY'
import pathlib
import re
import sys

RED, NC = "\033[0;31m", "\033[0m"
BLOCKING = {"critical", "high"}

rows = {}
for line in pathlib.Path(sys.argv[1]).read_text().splitlines():
    match = re.match(r"\s+> (\S+)\s+(\S+)\s+(?:(\S+)\s+)?(\S+)\s+(https://\S+)", line)
    if match:
        name, resolved, _, severity, url = match.groups()
        rows[(name, resolved)] = (severity, url)

if not rows:
    print("\033[0;32m  no known vulnerable packages\033[0m")
    raise SystemExit(0)

blocking = {k: v for k, v in rows.items() if v[0].lower() in BLOCKING}
print(f"  {len(rows)} vulnerable package(s), {len(blocking)} high or critical")
for (name, resolved), (severity, url) in sorted(rows.items(), key=lambda r: r[1][0]):
    marker = f"{RED}✗{NC}" if severity.lower() in BLOCKING else " "
    print(f"   {marker} {name} {resolved}  {severity}  {url}")

raise SystemExit(1 if blocking else 0)
PY
        then
            FAILED=1
        fi
    else
        echo "${YELLOW}  package audit unavailable (offline, or restore needed)${NC}"
    fi
fi

echo
if [ "$FAILED" -ne 0 ]; then
    echo "${RED}pre-check failed${NC}"
    exit 1
fi
echo "${GREEN}pre-check passed${NC}"
