#!/bin/bash

# Color definitions
GREEN='\033[1;32m'
YELLOW='\033[1;33m'
BLUE='\033[1;34m'
RED='\033[1;31m'
NC='\033[0m'  # No Color

# Print colored text
print_color() {
    local color=$1
    local message=$2
    printf "${color}${message}${NC}"
}

# Color-specific convenience functions
info() {
    print_color "\n${BLUE}" "$1\n"
}

warning() {
    print_color "\n${YELLOW}" "$1\n"
}

error() {
    print_color "\n${RED}" "$1\n"
}

success() {
    print_color "\n${GREEN}" "$1\n"
}
