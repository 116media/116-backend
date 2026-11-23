#!/usr/bin/env bash

# Adapter between Claude Code hooks and `ai-precheck.sh`. Reads the hook payload on
# stdin and emits the hook's JSON response on stdout.
#
#   commit  blocks `git commit` when the staged change fails the gate
#   edit    reports CRAP on a single edited file, without blocking
#
# Both modes fail open: a missing script, a missing tool or an unparseable payload
# leaves the tool call untouched rather than wedging the session. Set SKIP_PRECHECK=1
# to turn the commit gate off for one shell.

set -uo pipefail

MODE="${1:-commit}"
PAYLOAD="$(cat)"
ROOT="${CLAUDE_PROJECT_DIR:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"
PRECHECK="${ROOT}/scripts/ai-precheck.sh"

[ -r "$PRECHECK" ] || exit 0

strip_colour() {
    sed $'s/\033\\[[0-9;]*m//g'
}

case "$MODE" in
    commit)
        [ "${SKIP_PRECHECK:-}" = "1" ] && exit 0

        REPORT="$(bash "$PRECHECK" --staged 2>&1)"
        STATUS=$?
        [ "$STATUS" -eq 0 ] && exit 0

        REASON="$(printf '%s' "$REPORT" | strip_colour)"
        jq -nc --arg reason "$REASON" '{
            hookSpecificOutput: {
                hookEventName: "PreToolUse",
                permissionDecision: "deny",
                permissionDecisionReason: ("Pre-commit gate failed. Fix these, then commit again — or set SKIP_PRECHECK=1 to bypass deliberately.\n\n" + $reason)
            }
        }'
        ;;

    edit)
        FILE="$(printf '%s' "$PAYLOAD" | jq -r '.tool_input.file_path // .tool_response.filePath // empty')"
        case "$FILE" in
            *.cs) ;;
            *) exit 0 ;;
        esac
        case "$FILE" in
            */tests/*|*/Migrations/*) exit 0 ;;
        esac

        REPORT="$(bash "$PRECHECK" --file "$FILE" 2>/dev/null | strip_colour)"
        [ -z "$REPORT" ] && exit 0

        jq -nc --arg report "$REPORT" '{
            suppressOutput: true,
            hookSpecificOutput: {
                hookEventName: "PostToolUse",
                additionalContext: $report
            }
        }'
        ;;

    *)
        exit 0
        ;;
esac
