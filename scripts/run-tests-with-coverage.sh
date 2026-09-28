#!/bin/bash

# Script to run tests with code coverage locally
# Usage: ./scripts/run-tests-with-coverage.sh [unit|integration|architecture|all]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
COVERAGE_DIR="$PROJECT_ROOT/coverage"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_header() {
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}\n"
}

print_success() {
    echo -e "${GREEN}$1${NC}"
}

print_warning() {
    echo -e "${YELLOW}$1${NC}"
}

print_error() {
    echo -e "${RED}$1${NC}"
}

# The CI runsettings turn SourceLink on so Codecov can link each line to GitHub. Locally that
# makes the collector record raw.githubusercontent URLs instead of file paths, and the report
# generator then 404s on every file that is not pushed yet. This writes a local copy with that
# one setting flipped, leaving the CI settings untouched.
local_runsettings() {
    local source="$1"
    local target="$COVERAGE_DIR/$(basename "${source%.runsettings}").local.runsettings"

    mkdir -p "$COVERAGE_DIR"
    sed 's|<UseSourceLink>true</UseSourceLink>|<UseSourceLink>false</UseSourceLink>|' "$source" > "$target"
    echo "$target"
}

cleanup() {
    print_header "Cleaning up previous coverage data"
    rm -rf "$COVERAGE_DIR"
    mkdir -p "$COVERAGE_DIR/unit"
    mkdir -p "$COVERAGE_DIR/integration"
}

run_unit_tests() {
    print_header "Running Unit Tests"
    # The same solution filter, collector and runsettings the CI unit job uses, so local
    # numbers match what Codecov reports. Endpoint classes are excluded from the unit
    # accounting by coverage.unit.runsettings: routing is owned by the integration suite.
    dotnet test "$PROJECT_ROOT/tests/unit.slnf" \
        --configuration Release \
        --settings "$(local_runsettings "$PROJECT_ROOT/tests/coverage.unit.runsettings")" \
        --collect:"XPlat Code Coverage" \
        --results-directory "$COVERAGE_DIR/unit" \
        --logger "console;verbosity=normal"
}

run_integration_tests() {
    print_header "Running Integration Tests"
    print_warning "Note: Integration tests require Docker to be running"

    if ! docker info > /dev/null 2>&1; then
        print_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi

    # The same collector and runsettings as the CI integration job, which keeps endpoints counted.
    DOTNET_ENVIRONMENT=Testing dotnet test "$PROJECT_ROOT/tests/integration.slnf" \
        --configuration Release \
        --settings "$(local_runsettings "$PROJECT_ROOT/tests/coverage.runsettings")" \
        --collect:"XPlat Code Coverage" \
        --results-directory "$COVERAGE_DIR/integration" \
        --logger "console;verbosity=normal"
}

run_architecture_tests() {
    print_header "Running Architecture Tests"
    # No coverage: these rules reflect over assemblies and execute no product code.
    dotnet test "$PROJECT_ROOT/tests/Architecture.Tests" \
        --configuration Release \
        --logger "console;verbosity=normal"
}

# Generate a standalone coverage report for a single test suite.
# Each suite gets its own report directory so unit and integration
# coverage are reported separately instead of being merged together.
generate_suite_report() {
    local suite="$1"
    local label="$2"
    local reports="$COVERAGE_DIR/$suite/**/coverage.opencover.xml"

    if [ -z "$(find "$COVERAGE_DIR/$suite" -name 'coverage.opencover.xml' 2>/dev/null | head -1)" ]; then
        print_warning "No $label coverage found under $COVERAGE_DIR/$suite — skipping"
        return
    fi

    reportgenerator \
        -reports:"$reports" \
        -targetdir:"$COVERAGE_DIR/report/$suite" \
        -reporttypes:"Html;TextSummary;MarkdownSummaryGithub" \
        -assemblyfilters:"-*Tests*;-*Migrations*" \
        -title:"$label Test Coverage"

    print_success "\n$label coverage report: $COVERAGE_DIR/report/$suite/index.html"

    if [ -f "$COVERAGE_DIR/report/$suite/Summary.txt" ]; then
        echo ""
        print_header "$label Coverage Summary"
        cat "$COVERAGE_DIR/report/$suite/Summary.txt"
    fi
}

generate_report() {
    print_header "Generating Coverage Report"

    # Check if reportgenerator is installed
    if ! command -v reportgenerator &> /dev/null; then
        print_warning "Installing reportgenerator..."
        dotnet tool install -g dotnet-reportgenerator-globaltool
    fi

    generate_suite_report "unit" "Unit"
    generate_suite_report "integration" "Integration"
}

show_help() {
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  unit         Run unit tests only"
    echo "  integration  Run integration tests only (requires Docker)"
    echo "  architecture Run architecture rules only (no coverage)"
    echo "  all          Run all tests (default)"
    echo "  report       Generate report from existing coverage data"
    echo "  help         Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0              # Run all tests with coverage"
    echo "  $0 unit         # Run only unit tests with coverage"
    echo "  $0 integration  # Run only integration tests with coverage"
    echo "  $0 report       # Generate report from existing coverage files"
    echo ""
    echo "Note: this runs the same solution filters, collector and runsettings as CI,"
    echo "      so the numbers match what Codecov reports."
}

# Main script
case "${1:-all}" in
    unit)
        cleanup
        run_unit_tests
        generate_report
        ;;
    integration)
        cleanup
        run_integration_tests
        generate_report
        ;;
    architecture)
        run_architecture_tests
        ;;
    all)
        cleanup
        run_unit_tests
        run_integration_tests
        run_architecture_tests
        generate_report
        ;;
    report)
        generate_report
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        print_error "Unknown command: $1"
        show_help
        exit 1
        ;;
esac

print_success "\nDone!"
