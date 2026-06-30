#!/bin/bash
set -e

source "$(dirname "$0")/logging.sh"

info "Removing all previous TestResults"
find . -type d -name "TestResults" -exec sh -c 'rm -rf "$1"/*' _ {} \;

if [[ "$1" == "unit" ]]; then
    testFilter="--filter \"FullyQualifiedName~UnitTests\""
    reportTestMatcher="**UnitTests"
else
    testFilter=""
    reportTestMatcher="**"
fi

info "Running tests and collecting coverage"
dotnet test $testFilter --collect "XPlat Code Coverage;Format=opencover,cobertura" --settings coverage.runsettings --no-restore --no-build

reportTargetDir="$PWD/test-coverage-reports/"

info "Generating report"

dotnet reportgenerator -reports:"./tests/${reportTestMatcher}/TestResults/**/coverage.opencover.xml" -targetdir:"${reportTargetDir}"

if [[ "$2" == "open" ]]; then
    xdg-open "${reportTargetDir}/index.html"
fi
