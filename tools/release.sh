#!/usr/bin/env bash
# Builds the release zip. Runs locally, not in CI, and cannot be moved there: the
# plugin compiles against the ~152 interop assemblies BepInEx generates from the
# game's own IL2CPP metadata on first run. Those come from a copy of the game, which
# is copyrighted and never versioned, so no hosted runner can produce this DLL.
#
#     bash tools/release.sh
#     bash tools/release.sh --with-bepinex ~/Downloads/BepInEx-Unity.IL2CPP-win-x64.zip
#     bash tools/release.sh --publish            # tag, release e allegati, in un colpo
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
REPO="Xodryx/vtm-reckoning-of-new-york-italian"
BEPINEX_ZIP=""
PUBLISH=0

while [ $# -gt 0 ]; do
    case "$1" in
        --publish)
            PUBLISH=1
            shift
            ;;
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
    ARCHIVE="$DIST_DIR/RonyItalian-ita-v$VERSION-with-bepinex.zip"
else
    ARCHIVE="$DIST_DIR/RonyItalian-ita-v$VERSION.zip"
fi

# Publishing checks come before the build, so a bad state costs a second, not a package.
if [ "$PUBLISH" = "1" ]; then
    if [ -n "$(git -C "$PROJECT_DIR" status --porcelain)" ]; then
        echo "ERRORE: ci sono modifiche non committate. Non pubblico." >&2
        exit 1
    fi
    # The tag and the version compiled into the plugin must be the same thing, or the
    # log says one number and the file name another.
    if git -C "$PROJECT_DIR" rev-parse -q --verify "refs/tags/v$VERSION" > /dev/null; then
        if [ "$(git -C "$PROJECT_DIR" rev-list -n1 "v$VERSION")" != "$(git -C "$PROJECT_DIR" rev-parse HEAD)" ]; then
            echo "ERRORE: il tag v$VERSION esiste ma non punta a HEAD." >&2
            echo "O sposti il tag, o alzi Version in plugin/Plugin.cs." >&2
            exit 1
        fi
    fi
fi

echo "Controllo dei blocchi..."
python "$PROJECT_DIR/tools/apply.py" --check > /dev/null

echo "Compilazione..."
# Piping straight into tail -3 hides the compiler errors behind a silent exit that looks
# exactly like a build which succeeded. Same trap that was sitting in deploy.sh.
GAME_DIR="${GAME_DIR:-$HOME/Documents/RoNY-game-copy}"
if ! build_output="$(dotnet build "$PROJECT_DIR/plugin/RonyItalian.csproj" -c Release -v minimal \
    -p:GameDir="$(cygpath -w "$GAME_DIR" 2>/dev/null || echo "$GAME_DIR")" 2>&1)"; then
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

    # BepInEx opens a console window on every launch by default, and the archives carry
    # no configuration, so a player installing this would get one for good. The same
    # lines go to BepInEx/LogOutput.log regardless: the console is for whoever is
    # working on the mod, not for whoever is playing it.
    #
    # Only the one key we mean is written. BepInEx fills the rest of the file in with
    # its own defaults on first run, so this cannot go stale when BepInEx changes.
    mkdir -p "$STAGE/BepInEx/config"
    cat > "$STAGE/BepInEx/config/BepInEx.cfg" <<'BEPINEXCFG'
[Logging.Console]

## Enables showing a console for log output.
Enabled = false
BEPINEXCFG

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
    BEPINEX_HINT="- Se avevi già BepInEx installato, questo pacchetto ne sovrascrive la
  configurazione (BepInEx/config/BepInEx.cfg) per spegnere la finestra nera
  della console, che a chi gioca non serve. Il log resta in
  BepInEx/LogOutput.log. Se ti servivano impostazioni tue, salvale prima.
"
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

if [ "$PUBLISH" = "1" ]; then
    # Every archive built for this version, whichever runs produced them: forgetting an
    # attachment is the mistake this whole flag exists to prevent.
    ASSETS=("$DIST_DIR"/RonyItalian-ita-v"$VERSION"*.zip)
    if [ ! -e "${ASSETS[0]}" ]; then
        echo "ERRORE: in dist/ non c'è nessun pacchetto per la versione $VERSION." >&2
        exit 1
    fi

    # Read from the git credential helper and never echo it.
    TOKEN="$(printf 'protocol=https
host=github.com

' | git credential fill 2>/dev/null         | sed -n 's/^password=//p')"
    if [ -z "$TOKEN" ]; then
        echo "ERRORE: nessuna credenziale GitHub disponibile. Fai un push a mano una volta." >&2
        exit 1
    fi

    if ! git -C "$PROJECT_DIR" rev-parse -q --verify "refs/tags/v$VERSION" > /dev/null; then
        git -C "$PROJECT_DIR" tag -a "v$VERSION" -m "$VERSION"
    fi
    git -C "$PROJECT_DIR" push origin "v$VERSION" > /dev/null 2>&1 || true

    NOTES_FILE="$PROJECT_DIR/tools/release-notes.md"
    if [ ! -f "$NOTES_FILE" ]; then
        echo "ERRORE: manca $NOTES_FILE." >&2
        exit 1
    fi

    RELEASE_ID="$(python "$PROJECT_DIR/tools/publish.py" release         "$REPO" "$VERSION" "$NOTES_FILE" "$TOKEN")"
    echo "Release v$VERSION: id $RELEASE_ID"

    for asset in "${ASSETS[@]}"; do
        echo -n "  $(basename "$asset") "
        python "$PROJECT_DIR/tools/publish.py" asset "$REPO" "$RELEASE_ID" "$asset" "$TOKEN"
    done

    echo "https://github.com/$REPO/releases/tag/v$VERSION"
fi
