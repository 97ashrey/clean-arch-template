#!/bin/bash
printf "Removing all previous TestResults\n"
find . -type d -name "TestResults" -exec sh -c 'rm -rf "$1"/*' _ {} \;

if [[ "$1" == "unit" ]]; then
    testFilter="--filter FullyQualifiedName~UnitTests"
    reportTestMatcher="**UnitTests"
else
    testFilter=""
    reportTestMatcher="**"
fi

printf "\nRunning tests and collecting coverage\n"
dotnet test "$testFilter" --collect "XPlat Code Coverage;Format=opencover,cobertura" --settings coverage.runsettings --no-restore --no-build

# currentDirectory=$(basename "$PWD")
reportTargetDir="$PWD/test-coverage-reports/"

printf "\nGenerating report\n"
reportgenerator -reports:"./tests/${reportTestMatcher}/TestResults/**/coverage.cobertura.xml" -targetdir:"${reportTargetDir}"

if [[ "$2" == "open" ]]; then
    xdg-open "${reportTargetDir}/index.html"
fi
