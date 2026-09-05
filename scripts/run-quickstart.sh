#!/usr/bin/env bash
# Run a quickstart headlessly (xvfb) and exit with its CI code. Same script locally and in CI.
# Usage: run-quickstart.sh <QuickstartName> [reportPath]
# Env:   RIMWORLD_DIR, RIMWORLD_CONFIG, QUICKSTART_LOG, QUICKSTART_SEED. Defaults below.

set -uo pipefail

GAME_DIR="${RIMWORLD_DIR:-/mnt/games/SteamLibrary/steamapps/common/RimWorld}"
CONFIG_DIR="${RIMWORLD_CONFIG:-$HOME/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config}"
NAME="${1:?usage: run-quickstart.sh <QuickstartName> [reportPath]}"
REPORT="${2:-/tmp/quickstart-report.json}"
LOG="${QUICKSTART_LOG:-/tmp/rimworld-quickstart.log}"
SEED="${QUICKSTART_SEED:-}"

rm -f "$REPORT"

if ! command -v xvfb-run >/dev/null 2>&1; then
  echo "xvfb-run not found. Install xorg-server-xvfb (Arch) / xvfb (Debian)." >&2
  exit 127
fi

# A quickstart only runs in dev mode, and a fresh Prefs.xml has it off.
PREFS="$CONFIG_DIR/Prefs.xml"
if [[ -f "$PREFS" ]]; then
  if grep -q '<devMode>False</devMode>' "$PREFS"; then
    sed -i 's|<devMode>False</devMode>|<devMode>True</devMode>|' "$PREFS"
    echo "enabled dev mode in $PREFS"
  fi
else
  echo "warning: no Prefs.xml at $PREFS; if dev mode is off the quickstart will not run." >&2
fi

cd "$GAME_DIR" || exit 1

ARGS=(-quickstart="$NAME" -quickstartreport="$REPORT" -logfile "$LOG")
if [[ -n "$SEED" ]]; then
  ARGS+=(-quickstartseed="$SEED")
fi

xvfb-run -a --server-args="-screen 0 1920x1080x24" ./RimWorldLinux "${ARGS[@]}"

CODE=$?

echo "----------------------------------------"
echo "quickstart '$NAME' exited with code $CODE"
grep -F 'picked starting tile' "$LOG" | tail -1
if [[ -f "$REPORT" ]]; then
  echo "report: $REPORT"
  cat "$REPORT"
else
  echo "no report written (boot may have failed before the quickstart ran; see $LOG)"
fi

exit "$CODE"
