#!/usr/bin/env bash
# Run a quickstart headlessly (xvfb) and exit with its CI code.
# Used both locally and in CI so there is no environment drift.
#
# Usage:
#   run-quickstart.sh <QuickstartName> [reportPath]
# Example:
#   run-quickstart.sh ScenarioTestQuickstart /tmp/qs-report.json
#
# Env:
#   RIMWORLD_DIR     game folder            (default: the Steam library path below)
#   RIMWORLD_CONFIG  Config folder holding Prefs.xml
#   QUICKSTART_LOG   where the Player.log goes

set -uo pipefail

GAME_DIR="${RIMWORLD_DIR:-/mnt/games/SteamLibrary/steamapps/common/RimWorld}"
CONFIG_DIR="${RIMWORLD_CONFIG:-$HOME/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config}"
NAME="${1:?usage: run-quickstart.sh <QuickstartName> [reportPath]}"
REPORT="${2:-/tmp/quickstart-report.json}"
LOG="${QUICKSTART_LOG:-/tmp/rimworld-quickstart.log}"

rm -f "$REPORT"

if ! command -v xvfb-run >/dev/null 2>&1; then
  echo "xvfb-run not found. Install xorg-server-xvfb (Arch) / xvfb (Debian)." >&2
  exit 127
fi

# A quickstart only runs in dev mode, and a fresh Prefs.xml has it off. Without this
# the game boots to the main menu and the report is never written.
PREFS="$CONFIG_DIR/Prefs.xml"
if [ -f "$PREFS" ]; then
  if grep -q '<devMode>False</devMode>' "$PREFS"; then
    sed -i 's|<devMode>False</devMode>|<devMode>True</devMode>|' "$PREFS"
    echo "enabled dev mode in $PREFS"
  fi
else
  echo "warning: no Prefs.xml at $PREFS; if dev mode is off the quickstart will not run." >&2
fi

cd "$GAME_DIR" || exit 1

xvfb-run -a --server-args="-screen 0 1920x1080x24" \
  ./RimWorldLinux -quickstart="$NAME" -quickstartreport="$REPORT" -logfile "$LOG"

CODE=$?

echo "----------------------------------------"
echo "quickstart '$NAME' exited with code $CODE"
if [ -f "$REPORT" ]; then
  echo "report: $REPORT"
  cat "$REPORT"
else
  echo "no report written (boot may have failed before the quickstart ran; see $LOG)"
fi

exit "$CODE"
