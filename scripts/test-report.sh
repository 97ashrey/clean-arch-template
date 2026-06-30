#!/bin/bash
set -e

source "$(dirname "$0")/logging.sh"

noRestore=""
noBuild=""
reportFormat="opencover"
testFilter=""
reportTestMatcher="**"
shouldOpen=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        unit)
            testFilter="--filter \"FullyQualifiedName~UnitTests\""
            reportTestMatcher="**UnitTests"
            shift
            ;;
        open)
            shouldOpen=true
            shift
            ;;
        --no-restore)
            noRestore="--no-restore"
            shift
            ;;
        --no-build)
            noBuild="--no-build"
            shift
            ;;
        --format)
            if [[ -z "$2" || ( "$2" != "opencover" && "$2" != "cobertura" ) ]]; then
                echo "Error: --format requires 'opencover' or 'cobertura'"
                exit 1
            fi
            reportFormat="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [unit] [open] [--no-restore] [--no-build] [--format opencover|cobertura]"
            exit 1
            ;;
    esac
done

info "Configuration: scope=${reportTestMatcher} restore=$([ -z "$noRestore" ] && echo 'yes' || echo 'no') build=$([ -z "$noBuild" ] && echo 'yes' || echo 'no') format=${reportFormat}"

info "Removing all previous TestResults"
find . -type d -name "TestResults" -exec sh -c 'rm -rf "$1"/*' _ {} \;

info "Running tests and collecting coverage"
dotnet test $testFilter --collect "XPlat Code Coverage;Format=opencover,cobertura" --settings coverage.runsettings $noRestore $noBuild

reportTargetDir="$PWD/test-coverage-reports/"

info "Generating report"

dotnet reportgenerator -reports:"./tests/${reportTestMatcher}/TestResults/**/coverage.${reportFormat}.xml" -targetdir:"${reportTargetDir}"

if [[ "$shouldOpen" == true ]]; then
    xdg-open "${reportTargetDir}/index.html"
fi
