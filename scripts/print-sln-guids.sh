#!/bin/bash
set -e

grep -oE 'Project\("\{[A-Fa-f0-9-]+\}"\) = "[^"]+", "[^"]+", "\{([A-Fa-f0-9-]+)\}"' *.sln | grep -oE '[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}' | sort -u | awk '{print "  \"" $0 "\","}' | sed '$ s/,$//'