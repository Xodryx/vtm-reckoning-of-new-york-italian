#!/usr/bin/env bash
# Builds the plugin and installs it into the working copy of the game, verifying that
# what ends up in BepInEx/plugins is byte-for-byte what was just built.
#
# A stale DLL left behind by a failed copy costs hours: the game runs a different plugin
# than the one you wrote, and the logs seem to disprove a fix that was never loaded.
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GAME_DIR="${GAME_DIR:-$HOME/Documents/RoNY-game-copy}"
PLUGIN_DIR="$GAME_DIR/BepInEx/plugins"

BUILT_DLL="$PROJECT_DIR/plugin/bin/Release/net6.0/RonyItalian.dll"
TRANSLATIONS="$PROJECT_DIR/translations/italian.json"

if [ ! -d "$PLUGIN_DIR" ]; then
    echo "Cartella dei plugin non trovata: $PLUGIN_DIR" >&2
    echo "Imposta GAME_DIR se la copia di lavoro sta altrove." >&2
    exit 1
fi

if pgrep -f "VtM Reckoning of New York.exe" > /dev/null 2>&1; then
    echo "Il gioco è in esecuzione: chiudilo prima di installare." >&2
    exit 1
fi

echo "Compilazione..."
dotnet build "$PROJECT_DIR/plugin/RonyItalian.csproj" -c Release -v minimal | tail -3

install_verified() {
    local source="$1" destination="$2" label="$3"
    cp "$source" "$destination"
    local a b
    a="$(md5sum < "$source" | cut -d' ' -f1)"
    b="$(md5sum < "$destination" | cut -d' ' -f1)"
    if [ "$a" != "$b" ]; then
        echo "ERRORE: $label installato non coincide con l'originale ($a != $b)" >&2
        exit 1
    fi
    echo "  $label  $a"
}

echo "Installazione in $PLUGIN_DIR"
install_verified "$BUILT_DLL" "$PLUGIN_DIR/RonyItalian.dll" "RonyItalian.dll  "
install_verified "$TRANSLATIONS" "$PLUGIN_DIR/italian.json" "italian.json     "

echo "Fatto. Avvia il gioco da $GAME_DIR"
