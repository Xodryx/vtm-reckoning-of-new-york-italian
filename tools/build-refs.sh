#!/usr/bin/env bash
# Builds the reference stubs in refs/ and collects them into build/refs.
#
# The plugin normally compiles against the interop assemblies BepInEx generates from
# the game's IL2CPP metadata, which are a derivative of the game and cannot be
# committed. The stubs carry the same assembly names and the same signatures for the
# twelve types the plugin actually touches, so the compiler is satisfied without a
# copy of the game. At runtime the plugin binds to the real assemblies; the stubs are
# never shipped and never loaded.
#
#     bash tools/build-refs.sh              # usa bepinex-core/ nel progetto
#     BEPINEX_CORE=/percorso bash tools/build-refs.sh
#
# What this buys is a compile check on every push. It does NOT prove the plugin works:
# a stub that has drifted from the real API compiles happily and fails at runtime, and
# only starting the game can tell. The shipped DLL is still built locally, by
# release.sh, against the real assemblies.
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT="$PROJECT_DIR/build/refs"
BEPINEX_CORE="${BEPINEX_CORE:-$PROJECT_DIR/bepinex-core}"

if [ ! -f "$BEPINEX_CORE/Il2CppInterop.Runtime.dll" ]; then
    echo "ERRORE: non trovo il core di BepInEx in $BEPINEX_CORE" >&2
    echo "Scaricalo da https://builds.bepinex.dev/projects/bepinex_be" >&2
    exit 1
fi

rm -rf "$OUT"
mkdir -p "$OUT"

for project in "$PROJECT_DIR"/refs/*/; do
    name="$(basename "$project")"
    printf '  %-48s' "$name"
    if ! output="$(dotnet build "$project/$name.csproj" -c Release -v minimal \
        -p:BepInExCore="$BEPINEX_CORE" 2>&1)"; then
        echo "FALLITO"
        echo "$output" >&2
        exit 1
    fi
    cp "$project/bin/Release/net6.0/$name.dll" "$OUT/"
    echo "ok"
done

# The plugin's csproj globs the whole reference folder, so BepInEx's own assemblies
# have to be there too.
cp "$BEPINEX_CORE"/*.dll "$OUT/" 2>/dev/null || true

echo "Riferimenti pronti in $OUT"
