#!/bin/bash

# Script to run tests with code coverage locally
# Usage: ./scripts/run-tests-with-coverage.sh [unit|integration|all]

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

cleanup() {
    print_header "Cleaning up previous coverage data"
    rm -rf "$COVERAGE_DIR"
    mkdir -p "$COVERAGE_DIR/unit"
    mkdir -p "$COVERAGE_DIR/integration"
}

run_unit_tests() {
    print_header "Running Unit Tests"
    # Using coverlet.msbuild for accurate coverage with C# 12 primary constructors.
    # Endpoint classes are excluded from the unit accounting: routing is owned by
    # the integration suite, which keeps counting them.
    dotnet test "$PROJECT_ROOT/tests/Unit" \
        --configuration Release \
        --logger "console;verbosity=normal" \
        /p:CollectCoverage=true \
        /p:CoverletOutputFormat=opencover \
        /p:CoverletOutput="$COVERAGE_DIR/unit/coverage.opencover.xml" \
        /p:ExcludeByFile="\"**/Migrations/**,**/obj/**/*.g.cs,**/*.Designer.cs,**/*EndpointV1.cs\"" \
        /p:ExcludeByAttribute="\"GeneratedCodeAttribute,CompilerGeneratedAttribute\""
}

run_integration_tests() {
    print_header "Running Integration Tests"
    print_warning "Note: Integration tests require Docker to be running"

    if ! docker info > /dev/null 2>&1; then
        print_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi

    # Using coverlet.msbuild for accurate coverage with C# 12 primary constructors
    DOTNET_ENVIRONMENT=Testing dotnet test "$PROJECT_ROOT/tests/Integration" \
        --configuration Release \
        --logger "console;verbosity=normal" \
        /p:CollectCoverage=true \
        /p:CoverletOutputFormat=opencover \
        /p:CoverletOutput="$COVERAGE_DIR/integration/coverage.opencover.xml" \
        /p:ExcludeByFile="\"**/Migrations/**,**/obj/**/*.g.cs,**/*.Designer.cs\"" \
        /p:ExcludeByAttribute="\"GeneratedCodeAttribute,CompilerGeneratedAttribute\""
}

# Generate a standalone coverage report for a single test suite.
# Each suite gets its own report directory so unit and integration
# coverage are reported separately instead of being merged together.
generate_suite_report() {
    local suite="$1"
    local label="$2"
    local report="$COVERAGE_DIR/$suite/coverage.opencover.xml"

    if [ ! -f "$report" ]; then
        print_warning "No $label coverage found at $report — skipping"
        return
    fi

    reportgenerator \
        -reports:"$report" \
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
    echo "Note: This script uses coverlet.msbuild for accurate coverage"
    echo "      with C# 12 primary constructors."
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
    all)
        cleanup
        run_unit_tests
        run_integration_tests
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
