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
# the binaries and the source is credited. The official BepInEx archives ship no
# licence file of their own, so this script adds one and refuses to build a bundle
# without it, which is what keeps a release from quietly breaking that.
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
LICENSE_TEXT="$PROJECT_DIR/reference/bepinex-license.txt"

# The two variants used to share a file name, so building one silently replaced the
# other and there was no telling them apart afterwards either.
if [ -n "$BEPINEX_ZIP" ]; then
    ARCHIVE="$DIST_DIR/RonyItalian-ita-v$VERSION-con-bepinex.zip"
else
    ARCHIVE="$DIST_DIR/RonyItalian-ita-v$VERSION.zip"
fi

echo "Controllo dei blocchi..."
python "$PROJECT_DIR/tools/apply.py" --check > /dev/null

echo "Compilazione..."
# Piping straight into tail -3 hides the compiler errors behind a silent exit that looks
# exactly like a build which succeeded. Same trap that was sitting in deploy.sh.
if ! build_output="$(dotnet build "$PROJECT_DIR/plugin/RonyItalian.csproj" -c Release -v minimal 2>&1)"; then
    echo "$build_output" >&2
    echo "ERRORE: compilazione fallita, nessun pacchetto è stato creato." >&2
    exit 1
fi
echo "$build_output" | tail -3

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

    # The official BepInEx archives carry no licence file at all -- checked against
    # 6.0.0-be.785, 233 files, not one LICENSE or COPYING. The LGPL does not ask them
    # to; it asks whoever redistributes the binaries. That is us, so we ship the text.
    if [ ! -f "$LICENSE_TEXT" ]; then
        echo "ERRORE: manca $LICENSE_TEXT, il testo della LGPL-2.1." >&2
        echo "Senza quello non si possono ridistribuire i binari di BepInEx." >&2
        exit 1
    fi
    cp "$LICENSE_TEXT" "$STAGE/BepInEx-LICENSE.txt"

    # Belt and braces: whatever happened above, the package does not leave without it.
    if ! find "$STAGE" -iname 'LICENSE*' -o -iname 'COPYING*' -o -iname '*-LICENSE.txt' \
        | grep -q .; then
        echo "ERRORE: nel pacchetto non è finito nessun testo di licenza. Non impacchetto." >&2
        exit 1
    fi

    cat > "$STAGE/BEPINEX.txt" <<'ATTRIBUTION'
Questo pacchetto include BepInEx, che non è opera nostra.

BepInEx è sviluppato dal team BepInEx ed è distribuito con licenza LGPL-2.1.
Codice sorgente: https://github.com/BepInEx/BepInEx

I file di BepInEx qui inclusi non sono stati modificati in alcun modo.

Il testo completo della licenza è in BepInEx-LICENSE.txt. Gli archivi ufficiali di
BepInEx non lo contengono, quindi è la copia canonica della GNU LGPL versione 2.1
presa da https://www.gnu.org/licenses/old-licenses/lgpl-2.1.txt, riprodotta integra.
ATTRIBUTION
fi

# Two paragraphs of the readme depend on whether BepInEx is in the package. A single
# wording for both was wrong in one of them: it told players of the bundled package to
# go and install BepInEx, and players of the plain one nothing about removing it.
if [ -n "$BEPINEX_ZIP" ]; then
    BEPINEX_HINT=""
    UNINSTALL_EXTRA="Per togliere anche BepInEx, che era incluso in questo pacchetto: cancella la
cartella BepInEx, la cartella dotnet, winhttp.dll, doorstop_config.ini,
.doorstop_version, changelog.txt, BEPINEX.txt e BepInEx-LICENSE.txt.

"
else
    BEPINEX_HINT="- Questo pacchetto non include BepInEx: senza, da solo non basta. Serve
  BepInEx 6 per IL2CPP, da https://github.com/BepInEx/BepInEx
"
    UNINSTALL_EXTRA=""
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
${BEPINEX_HINT}- In BepInEx/LogOutput.log le righe del plugin cominciano per
  "Reckoning of New York - Italian".


COME SI DISINSTALLA

Cancella BepInEx/plugins/RonyItalian.dll e BepInEx/plugins/italian.json.
${UNINSTALL_EXTRA}Il gioco torna in inglese senza altri interventi: la traduzione non tocca i file
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
