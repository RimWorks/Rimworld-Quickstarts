#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

versions=$(grep -oP '(?<=<v)\d+\.\d+(?=>)' loadFolders.xml)
if [ -z "$versions" ]; then
  echo "no <vX.Y> entries in loadFolders.xml" >&2
  exit 1
fi

for version in $versions; do
  echo "==> rimworld $version"
  dotnet build Quickstarts.slnx -c "${CONFIG:-Release}" -p:GameVersion="$version" --nologo
done
