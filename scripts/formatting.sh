#!/bin/bash

# C# Code Formatting Script
# Formats code using csharpier with support for staged files and verification

# Colors for output
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

staged_only=false

if ! TEMP=$(getopt -o s --long staged-only -n 'formatting' -- "$@"); then exit 1 ; fi
eval set -- "$TEMP"
while true; do
    case "$1" in
    -s | --staged-only ) staged_only=true; shift 2 ;;
    -- ) shift; break ;;
    * ) break ;;
    esac
done

if $staged_only; then
    echo -e "${BLUE}🔍 Checking for csharpier availability...${NC}"

    dotnet csharpier --version > /dev/null 2>&1;
    format_res=$?
    if [ $format_res -ne 0 ]; then
        echo -e "${RED}❌ ERROR: csharpier is not installed!${NC}"
        echo -e "${YELLOW}📋 Code formatting is required for maintaining code quality${NC}"
        echo -e "${YELLOW}💡 To install csharpier, run the setup.sh script${NC}"
        exit 1;
    fi

    echo -e "${BLUE}🔎 Scanning for staged C# files...${NC}"
    FILES=$(git diff --staged --name-only --diff-filter=ACM "*.cs")

    if [ -z "$FILES" ]; then
        echo -e "${GREEN}✅ No staged C# files to format${NC}"
        exit 0
    fi

    echo -e "${GREEN}🎨 Formatting staged C# files...${NC}"
    echo "$FILES" | xargs dotnet csharpier format

    echo -e "${BLUE}📝 Re-staging formatted files...${NC}"
    echo "$FILES" | xargs git add

    echo -e "${GREEN}✅ Code formatting complete! All staged files are now properly formatted${NC}"
else
    echo -e "${BLUE}🔍 Validating code formatting...${NC}"

    echo -e "${BLUE}📏 Checking code formatting...${NC}"
    dotnet csharpier format --check .
    check_res=$?
    if [ $check_res -ne 0 ]; then
        echo -e "${RED}❌ Code formatting violations detected!${NC}"
        echo -e "${YELLOW}💡 Run 'dotnet csharpier format .' to fix formatting issues${NC}"
        exit 1;
    fi

    echo -e "${GREEN}✅ All code is properly formatted!${NC}"
fi
