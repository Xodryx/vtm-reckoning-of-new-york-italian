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
# player installs BepInEx themselves. With it, BepInEx travels along, which every one
# of its licences allows -- on their terms: the texts ship with the binaries, the
# sources are credited, nothing is modified.
#
# "BepInEx" is sixteen projects, not one: LGPL-2.1, LGPL-3.0, MIT and Apache-2.0
# between them, and its archives carry no licence file at all. So the texts live in
# reference/licenses/, each fetched from its own upstream, and the build refuses to
# make a bundle without them.
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
LICENSE_DIR="$PROJECT_DIR/reference/licenses"

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
    # 6.0.0-be.785: 233 files, not one LICENSE or COPYING. The LGPL does not ask them
    # to; it asks whoever redistributes the binaries, which is us. And BepInEx is not
    # one project but sixteen: every licence in reference/licenses/ was fetched from
    # its own upstream repository, so the notice below is checked, not remembered.
    if [ ! -d "$LICENSE_DIR" ] || [ -z "$(ls -A "$LICENSE_DIR" 2>/dev/null)" ]; then
        echo "ERRORE: manca $LICENSE_DIR con i testi delle licenze." >&2
        echo "Senza quelli non si possono ridistribuire i binari di BepInEx." >&2
        exit 1
    fi
    mkdir -p "$STAGE/licenses"
    cp "$LICENSE_DIR"/*.txt "$STAGE/licenses/"

    cat > "$STAGE/LICENZE.txt" <<'ATTRIBUTION'
COMPONENTI DI TERZE PARTI INCLUSI IN QUESTO PACCHETTO

Questo pacchetto include BepInEx e le librerie che BepInEx porta con sé. Non sono
opera nostra e non sono state modificate in alcun modo. Il testo completo di ogni
licenza, con le rispettive note di copyright, è nella cartella licenses/.

  File nel pacchetto                        Progetto                Licenza
  ---------------------------------------------------------------------------
  BepInEx/core/BepInEx.*.dll                BepInEx                 LGPL-2.1
  BepInEx/core/Il2CppInterop.*.dll          Il2CppInterop           LGPL-3.0
  winhttp.dll, doorstop_config.ini,
    .doorstop_version                       UnityDoorstop           LGPL-2.1
  BepInEx/core/0Harmony.dll                 HarmonyX                MIT
  BepInEx/core/MonoMod.*.dll                MonoMod                 MIT
  BepInEx/core/Mono.Cecil*.dll              Mono.Cecil              MIT
  BepInEx/core/AsmResolver*.dll             AsmResolver             MIT
  BepInEx/core/Cpp2IL.Core.dll,
    LibCpp2IL.dll, StableNameDotNet.dll,
    WasmDisassembler.dll                    Cpp2IL                  MIT
  BepInEx/core/Disarm.dll                   Disarm                  MIT
  BepInEx/core/AssetRipper.CIL.dll          AssetRipper.CIL         MIT
  BepInEx/core/AssetRipper.Primitives.dll   AssetRipper.Primitives  MIT
  BepInEx/core/Gee.External.Capstone.dll    Capstone.NET            MIT
  BepInEx/core/Iced.dll                     iced                    MIT
  BepInEx/core/SemanticVersioning.dll       SemanticVersioning      MIT
  BepInEx/core/dobby.dll                    Dobby                   Apache-2.0
  dotnet/                                   Runtime .NET            MIT

DOVE TROVARE I SORGENTI

  BepInEx                 https://github.com/BepInEx/BepInEx
  Il2CppInterop           https://github.com/BepInEx/Il2CppInterop
  HarmonyX                https://github.com/BepInEx/HarmonyX
  UnityDoorstop           https://github.com/NeighTools/UnityDoorstop
  MonoMod                 https://github.com/MonoMod/MonoMod
  Mono.Cecil              https://github.com/jbevain/cecil
  AsmResolver             https://github.com/Washi1337/AsmResolver
  Cpp2IL                  https://github.com/SamboyCoding/Cpp2IL
  Disarm                  https://github.com/SamboyCoding/Disarm
  AssetRipper.CIL         https://github.com/AssetRipper/AssetRipper.CIL
  AssetRipper.Primitives  https://github.com/AssetRipper/AssetRipper.Primitives
  Capstone.NET            https://github.com/9ee1/Capstone.NET
  iced                    https://github.com/icedland/iced
  SemanticVersioning      https://github.com/adamreeve/semver.net
  Dobby                   https://github.com/jmpews/Dobby
  Runtime .NET            https://github.com/dotnet/runtime

La traduzione italiana, cioè BepInEx/plugins/RonyItalian.dll e italian.json, è la
sola parte di questo pacchetto che sia opera nostra, e non fa parte di nessuno dei
progetti qui sopra.
ATTRIBUTION
fi

# Two paragraphs of the readme depend on whether BepInEx is in the package. A single
# wording for both was wrong in one of them: it told players of the bundled package to
# go and install BepInEx, and players of the plain one nothing about removing it.
if [ -n "$BEPINEX_ZIP" ]; then
    BEPINEX_HINT=""
    UNINSTALL_EXTRA="Per togliere anche BepInEx, che era incluso in questo pacchetto: cancella la
cartella BepInEx, la cartella dotnet, winhttp.dll, doorstop_config.ini,
.doorstop_version, changelog.txt, LICENZE.txt e la cartella licenses.

"
else
    # Naming the exact build is not pedantry. BepInEx 5 is what a player finds first and
    # it does not work with an IL2CPP game at all, and BepInEx 6 is a bleeding-edge line
    # whose API moves between builds -- be.785 is the one this was built and tested
    # against.
    BEPINEX_HINT="- Questo pacchetto non include BepInEx, e da solo non basta.
  Serve BepInEx 6 nella versione per IL2CPP, a 64 bit, dalle build di sviluppo:
  https://builds.bepinex.dev/projects/bepinex_be
  Cerca \"BepInEx-Unity.IL2CPP-win-x64\". La traduzione è stata provata con la
  build 6.0.0-be.785.
  ATTENZIONE: BepInEx 5, che è quello che si scarica per primo dal sito
  principale, con questo gioco NON funziona. Deve essere la 6 per IL2CPP.
"
    UNINSTALL_EXTRA=""
fi

cat > "$STAGE/LEGGIMI.txt" <<INSTRUCTIONS
Vampire: The Masquerade - Reckoning of New York
Traduzione italiana non ufficiale, versione $VERSION

Traduzione amatoriale, senza alcun rapporto con Draw Distance né con Paradox
Interactive. Il gioco non è incluso: serve una copia regolare.


NON È ANCORA STATA RILETTA DA UNA PERSONA

La traduzione è completa — tutte le 11.141 battute del gioco — ma non è ancora stata
verificata da una rilettura umana giocandoci. Per questo la versione resta sotto la
1.0: completo e verificato non sono la stessa cosa.

Se trovi un errore, e soprattutto una frase rivolta a Kali con il genere sbagliato
(l'inglese non lo marca, quindi è l'errore più facile da non vedere), segnalalo qui:

  https://github.com/Xodryx/vtm-reckoning-of-new-york-italian

Puoi anche correggerlo da solo: BepInEx/plugins/italian.json è un file di testo, si
apre con un editor qualsiasi e il gioco lo rilegge a ogni avvio.


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
