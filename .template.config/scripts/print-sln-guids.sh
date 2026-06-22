#!/bin/bash
set -e

grep -oE '[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}' *.sln \
# 2150E333-8FDC-42A3-9474-1A3956D46DE8 is an id reserved for solution folders
# FAE04EC0-301F-11D3-BF4B-00C04F79EFBC is an id reserved for .net projects
# these should never be replaced
| awk '
BEGIN {
  ignore["2150E333-8FDC-42A3-9474-1A3956D46DE8"]=1
  ignore["FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"]=1
}
!ignore[toupper($0)]
' \
| sort -u \
| awk '{print "  \"" $0 "\","}' \
| sed '$ s/,$//'