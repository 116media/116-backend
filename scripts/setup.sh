#!/bin/bash

# 🛠️  116 Backend Development Setup Script
# Installs csharpier and configures git hooks for code quality

# Colors for output
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 Starting 116 Backend development environment setup...${NC}"

# Check if csharpier is installed
echo -e "${BLUE}🔍 Checking for csharpier installation...${NC}"
dotnet csharpier --version > /dev/null 2>&1;
RESULT=$?

if [ $RESULT -ne 0 ]; then
    echo -e "${YELLOW}⚠️  csharpier is not installed${NC}"
    printf "%b💡 Would you like to install csharpier now? (y/n): %b" "${YELLOW}" "${NC}"
    read -r choice

    case "$choice" in
    y|Y )
        echo -e "${BLUE}📦 Installing csharpier...${NC}"
        dotnet tool install csharpier
        echo -e "${GREEN}✅ csharpier installed successfully!${NC}"
        ;;
    n|N )
        echo -e "${YELLOW}⏭️  Skipping csharpier installation${NC}"
        ;;
    * )
        echo -e "${RED}❌ Invalid choice, exiting setup${NC}"
        exit 0
        ;;
    esac
else
    echo -e "${GREEN}✅ csharpier is already installed${NC}"
fi

# Setup git hooks
echo -e "${BLUE}🔧 Setting up git hooks...${NC}"
GIT_HOOKS_DIR=$(git rev-parse --git-path hooks)

if [ ! -d "$GIT_HOOKS_DIR" ]; then
    GIT_HOOKS_DIR=".git/hooks/"
    echo -e "${YELLOW}📁 Git hooks directory not found, creating: $GIT_HOOKS_DIR${NC}"
    mkdir -p $GIT_HOOKS_DIR
fi

echo -e "${BLUE}📋 Copying pre-commit and pre-push hooks...${NC}"
cp .git_hooks/pre-commit "$GIT_HOOKS_DIR"/pre-commit
cp .git_hooks/pre-push "$GIT_HOOKS_DIR"/pre-push

# Make hooks executable
echo -e "${BLUE}🔐 Making hooks executable...${NC}"
chmod +x "$GIT_HOOKS_DIR/pre-commit" "$GIT_HOOKS_DIR/pre-push"

echo -e "${GREEN}✅ Git hooks configured successfully!${NC}"
echo -e "${GREEN}🎉 Setup complete! Your development environment is ready${NC}"
echo ""
echo -e "${BLUE}📋 What was configured:${NC}"
echo -e "${YELLOW}  • csharpier for code formatting${NC}"
echo -e "${YELLOW}  • Pre-commit hook for automatic code formatting${NC}"
echo -e "${YELLOW}  • Pre-push hook for branch validation and style checks${NC}"
echo ""
echo -e "${BLUE}💡 Next steps:${NC}"
echo -e "${YELLOW}  • Make your changes and commit them${NC}"
echo -e "${YELLOW}  • The pre-commit hook will automatically format your code${NC}"
echo -e "${YELLOW}  • The pre-push hook will validate your branch name and code style${NC}"
