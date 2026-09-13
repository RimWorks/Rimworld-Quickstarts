#!/usr/bin/env bash
#   stage-quickstart-mods.sh <harmony|concord> <mods-dir> <config-dir>
set -euo pipefail

BACKEND="${1:?usage: stage-quickstart-mods.sh <harmony|concord> <mods-dir> <config-dir>}"
MODS_DIR="${2:?mods dir}"
CONFIG_DIR="${3:?config dir}"

HARMONY_REPO="${HARMONY_REPO:-pardeike/HarmonyRimWorld}"
CONCORD_REPO="${CONCORD_REPO:-ConcordLib/RimWorld}"
RIMLOGGING_REPO="${RIMLOGGING_REPO:-RimWorks/rimworld-logging-framework}"

mkdir -p "$MODS_DIR" "$CONFIG_DIR"

https_only=(--proto '=https' --proto-redir '=https')

gh_api() {
  local url="$1"

  if [[ -n "${GITHUB_TOKEN:-}" ]]; then
    curl -sSfL "${https_only[@]}" \
      -H "Authorization: Bearer ${GITHUB_TOKEN}" \
      -H "X-GitHub-Api-Version: 2022-11-28" "$url"
  else
    curl -sSfL "${https_only[@]}" "$url"
  fi
}

stage_release_zip() {
  local repo="$1" prefix="$2" dest="$3" tmp json url inner bad
  tmp="$(mktemp -d)"

  json="$(gh_api "https://api.github.com/repos/${repo}/releases/latest")" || {
    echo "error: could not read the latest ${repo} release." \
         "GitHub allows 60 anonymous calls an hour; set GITHUB_TOKEN to raise it." >&2
    exit 1
  }

  url="$(printf '%s' "$json" | ASSET_PREFIX="$prefix" python3 -c 'import json, os, sys
prefix = os.environ["ASSET_PREFIX"]
assets = json.load(sys.stdin).get("assets", [])
match = [a for a in assets if a["name"].startswith(prefix) and a["name"].endswith(".zip")]
if not match:
    sys.exit(1)
print(match[0]["browser_download_url"])')" || {
    echo "error: the latest ${repo} release has no ${prefix}*.zip asset" >&2
    exit 1
  }

  curl -sSfL "${https_only[@]}" "$url" -o "$tmp/mod.zip"

  bad="$(unzip -Z1 "$tmp/mod.zip" | grep -E '(^|/)\.\./|^/' || true)"
  if [[ -n "$bad" ]]; then
    echo "error: the ${repo} zip contains a path traversal or absolute entry, refusing to extract" >&2
    exit 1
  fi

  unzip -qo "$tmp/mod.zip" -d "$tmp/x"

  inner="$(dirname "$(dirname "$(find "$tmp/x" -mindepth 2 -maxdepth 3 -path '*/About/About.xml' -print -quit)")")"
  [[ -d "$inner" && "$inner" != "." ]] || {
    echo "error: no About/About.xml inside the ${repo} zip, so its mod folder cannot be found" >&2
    exit 1
  }

  rm -rf "$dest"
  mv "$inner" "$dest"
  rm -rf "$tmp"
}

ACTIVE=""

if [[ "$BACKEND" == "concord" ]]; then
  stage_release_zip "$CONCORD_REPO" "Concord-" "$MODS_DIR/Concord"
  ACTIVE="${ACTIVE}concordlib.concord
"
else
  stage_release_zip "$HARMONY_REPO" "HarmonyMod" "$MODS_DIR/Harmony"
  ACTIVE="${ACTIVE}brrainz.harmony
"
fi

stage_release_zip "$RIMLOGGING_REPO" "RimLogging-" "$MODS_DIR/RimLogging"

ACTIVE="${ACTIVE}ludeon.rimworld
ludeon.rimworld.royalty
ludeon.rimworld.ideology
ludeon.rimworld.biotech
ludeon.rimworld.anomaly
ludeon.rimworld.odyssey
rimworks.rimlogging
rimworks.quickstarts"

cat > "$CONFIG_DIR/ModsConfig.xml" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData>
  <version>1.6</version>
  <activeMods>
$(printf '    <li>%s</li>\n' $ACTIVE)
  </activeMods>
  <knownExpansions>
    <li>ludeon.rimworld.royalty</li>
    <li>ludeon.rimworld.ideology</li>
    <li>ludeon.rimworld.biotech</li>
    <li>ludeon.rimworld.anomaly</li>
    <li>ludeon.rimworld.odyssey</li>
  </knownExpansions>
</ModsConfigData>
EOF

cat > "$CONFIG_DIR/Prefs.xml" <<'EOF'
<?xml version="1.0" encoding="utf-8"?>
<PrefsData>
  <screenWidth>1920</screenWidth>
  <screenHeight>1080</screenHeight>
  <fullscreen>False</fullscreen>
  <volumeGame>0</volumeGame>
  <volumeMusic>0</volumeMusic>
  <volumeAmbient>0</volumeAmbient>
  <devMode>True</devMode>
  <runInBackground>True</runInBackground>
  <resetModsConfigOnCrash>False</resetModsConfigOnCrash>
</PrefsData>
EOF

chmod -R 777 "$CONFIG_DIR"

echo "staged '$BACKEND':"
find "$MODS_DIR" -name 'About.xml' | sort
