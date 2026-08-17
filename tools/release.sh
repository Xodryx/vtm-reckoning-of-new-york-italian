#!/usr/bin/env bash
# Builds the release zip. Runs locally, not in CI, and cannot be moved there: the
# plugin compiles against the ~152 interop assemblies BepInEx generates from the
# game's own IL2CPP metadata on first run. Those come from a copy of the game, which
# is copyrighted and never versioned, so no hosted runner can produce this DLL.
#
#     bash tools/release.sh
#     bash tools/release.sh --with-bepinex ~/Downloads/BepInEx-Unity.IL2CPP-win-x64.zip
#
# Without --with-bepinex the zip holds only the plugin and the translation, and the
# player installs BepInEx themselves. With it, BepInEx is bundled: that is allowed,
# it is LGPL-2.1, but only on the licence's terms -- the licence text travels with
# the binaries and the source is credited. The script refuses to bundle a BepInEx
# archive that carries no licence file, so a release cannot quietly break that.
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DIST_DIR="$PROJECT_DIR/dist"
BEPINEX_ZIP=""

while [ $# -gt 0 ]; do
    case "$1" in
        --with-bepinex)
            BEPINEX_ZIP="${2:-}"
            if [ -z "$BEPINEX_ZIP" ]; then
                echo "--with-bepinex vuole il percorso dello zip di BepInEx." >&2
                exit 1
            fi
            shift 2
            ;;
        *)
            echo "Opzione sconosciuta: $1" >&2
            exit 1
            ;;
    esac
done

# The plugin announces this version in the log, so taking it from anywhere else would
# let the file name and the running plugin disagree.
VERSION="$(sed -n 's/.*const string Version = "\([^"]*\)".*/\1/p' "$PROJECT_DIR/plugin/Plugin.cs")"
if [ -z "$VERSION" ]; then
    echo "Non trovo la versione in plugin/Plugin.cs" >&2
    exit 1
fi

STAGE="$DIST_DIR/stage"
ARCHIVE="$DIST_DIR/RonyItalian-ita-v$VERSION.zip"

echo "Controllo dei blocchi..."
python "$PROJECT_DIR/tools/apply.py" --check > /dev/null

echo "Compilazione..."
dotnet build "$PROJECT_DIR/plugin/RonyItalian.csproj" -c Release -v minimal | tail -3

rm -rf "$STAGE"
mkdir -p "$STAGE/BepInEx/plugins"

cp "$PROJECT_DIR/plugin/bin/Release/net6.0/RonyItalian.dll" "$STAGE/BepInEx/plugins/"
cp "$PROJECT_DIR/translations/italian.json" "$STAGE/BepInEx/plugins/"

if [ -n "$BEPINEX_ZIP" ]; then
    if [ ! -f "$BEPINEX_ZIP" ]; then
        echo "Non trovo $BEPINEX_ZIP" >&2
        exit 1
    fi
    echo "Includo BepInEx da $(basename "$BEPINEX_ZIP")"
    python -c "import sys, zipfile; zipfile.ZipFile(sys.argv[1]).extractall(sys.argv[2])" \
        "$BEPINEX_ZIP" "$STAGE"

    # LGPL-2.1 lets us ship the binaries only if the licence ships with them.
    if ! find "$STAGE" -iname 'LICENSE*' -o -iname 'COPYING*' | grep -q .; then
        echo "ERRORE: lo zip di BepInEx non contiene il testo della licenza." >&2
        echo "La LGPL-2.1 lo richiede per ridistribuire i binari. Non impacchetto." >&2
        exit 1
    fi

    cat > "$STAGE/BEPINEX.txt" <<'ATTRIBUTION'
Questo pacchetto include BepInEx, che non è opera nostra.

BepInEx è sviluppato dal team BepInEx ed è distribuito con licenza LGPL-2.1.
Codice sorgente e licenza completa: https://github.com/BepInEx/BepInEx

I file di BepInEx qui inclusi non sono stati modificati in alcun modo.
ATTRIBUTION
fi

cat > "$STAGE/LEGGIMI.txt" <<INSTRUCTIONS
Vampire: The Masquerade - Reckoning of New York
Traduzione italiana non ufficiale, versione $VERSION

Traduzione amatoriale, senza alcun rapporto con Draw Distance né con Paradox
Interactive. Il gioco non è incluso: serve una copia regolare.


COME SI INSTALLA

1. Copia il contenuto di questo archivio nella cartella del gioco, quella che
   contiene "VtM Reckoning of New York.exe". Le cartelle si fondono con quelle
   già presenti.
2. Avvia il gioco.
3. Il gioco parte già in italiano.


IL PRIMO AVVIO È LENTO: È NORMALE

La prima volta che parte, BepInEx deve generare gli assembly di interoperabilità
del gioco. Ci mette una trentina di secondi e per tutto quel tempo la finestra
sembra bloccata. Non lo è: aspetta. Dal secondo avvio in poi il gioco parte come
sempre.


SE IL TESTO RESTA IN INGLESE

- Controlla che RonyItalian.dll e italian.json siano in BepInEx/plugins.
- Se non hai installato BepInEx, questo pacchetto da solo non basta: serve
  BepInEx 6 per IL2CPP, da https://github.com/BepInEx/BepInEx
- In BepInEx/LogOutput.log le righe del plugin cominciano per
  "Reckoning of New York - Italian".


COME SI DISINSTALLA

Cancella BepInEx/plugins/RonyItalian.dll e BepInEx/plugins/italian.json.
Il gioco torna in inglese senza altri interventi: la traduzione non tocca i file
del gioco, li lascia esattamente come sono.
INSTRUCTIONS

python - "$STAGE" "$ARCHIVE" <<'PACK'
import os
import sys
import zipfile

stage, archive = sys.argv[1], sys.argv[2]
os.makedirs(os.path.dirname(archive), exist_ok=True)
with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as zf:
    for root, _, files in os.walk(stage):
        for name in sorted(files):
            path = os.path.join(root, name)
            zf.write(path, os.path.relpath(path, stage))
PACK

rm -rf "$STAGE"

echo
echo "Creato $ARCHIVE"
python -c "import sys, zipfile; print('\n'.join('  ' + n for n in zipfile.ZipFile(sys.argv[1]).namelist()[:12]))" "$ARCHIVE"
