# Vampire: The Masquerade — Reckoning of New York, in Italian

An unofficial Italian translation of *Reckoning of New York*, delivered as a BepInEx
plugin. The game ships English and French only; this adds **Italiano** as a third entry
in the language selector and serves the translated text at runtime. **No game file is
modified.**

> ### Translated — 11,141 of 11,141 lines
>
> Every line is translated: the eight nights, the endings, the optional missions, the
> in-game glossary, the journal, the achievements and the interface.
>
> That total includes **747 lines that are not from this game**. The localization table
> still carries an unreleased demo of another Draw Distance title — a different cast, set
> in Podgórze in Kraków, with some lines in Polish and one that reads *"This concludes
> the demo."* No player of *Reckoning of New York* can reach any of it. It is translated
> for completeness, but the figure for this game alone is **10,394**.
>
> Translated is not the same as proofread. See below.

> ### Made with AI — expect mistakes
>
> Both the Italian text and the C# plugin are written by an AI assistant (Claude),
> directed and reviewed by a human. **Nobody has played the game through in Italian to
> proofread the result.** Treat it as a draft that runs, not as a finished localisation.
>
> - **Terminology should be reliable.** Glossary terms are not invented: they come from
>   the official Italian translation of *Coteries of New York*, the same setting and the
>   same vocabulary. `Kindred` is *Fratelli*, `Final Death` is *Morte Ultima*.
> - **Tone and register are where errors will be.** An automated check can verify that a
>   `<link="Sire">` marker survived; it cannot tell that a sarcastic line reads as
>   sincere.
> - **Subtitles may drift from the voice acting.** The game is fully voiced in English
>   and Italian runs longer. Lines are checked against a length budget, but the budget is
>   an estimate, not the game's renderer.
>
> Please [open an issue](../../issues) for anything that reads wrong. Mistakes nobody
> reports simply stay in.

## Compatibility

**Windows PC only**, and that is the only configuration this has been run on. The mod is
a BepInEx plugin, so it needs to inject itself into the process before the game starts.

The game is Unity 2022.3 **IL2CPP**, which means it needs BepInEx 6 (bleeding edge), not
the more common BepInEx 5.

## Installing

Download it from the [latest release](../../releases/latest). Two packages, and the
first one is the one to take:

| | |
|---|---|
| **`RonyItalian-ita-…-con-bepinex.zip`** | Everything included. Unpack it into the game folder and you are done. |
| **`RonyItalian-ita-….zip`** | 459 KB, the translation alone, for anyone who already runs BepInEx 6 for IL2CPP. |

Unpack into the folder that holds `VtM Reckoning of New York.exe`; the folders merge with
what is already there. The game then starts in Italian on its own. **The first launch
takes about half a minute** while BepInEx generates the game's interop assemblies, and
the window looks frozen for all of it.

To uninstall, delete `RonyItalian.dll` and `italian.json` from `BepInEx/plugins`. Nothing
else is touched, so Steam's file verification has nothing to restore.

## Building

Requires the .NET 6 SDK or later, Python 3.10+, and a copy of the game.

1. Copy the game folder somewhere writable. Do not work in the Steam install: Windows
   needs administrator rights to write under `Program Files`, and verifying the game
   files through Steam would delete anything you put there.
2. Install [BepInEx 6 IL2CPP](https://builds.bepinex.dev/projects/bepinex_be)
   (`BepInEx-Unity.IL2CPP-win-x64`) into the copy and launch the game once. The first
   run generates the interop assemblies the plugin builds against, which takes about
   half a minute.
3. Extract the game's localization table, which the tools use as their source of truth:

   ```
   pip install UnityPy
   python tools/extract_i2.py "<game copy>/VtM Reckoning of New York_Data/data.unity3d" dump
   ```

4. Build and install:

   ```
   bash tools/deploy.sh
   ```

   Set `GAME_DIR` if your working copy is not at `~/Documents/RoNY-game-copy`. The
   project file defaults to a standard Steam install; the scripts override it.

To publish a release: `bash tools/release.sh --publish`, optionally with
`--with-bepinex <zip>` to build the all-in-one package too. It refuses to run on
uncommitted changes or on a tag that disagrees with the version in `Plugin.cs`.

### Without a copy of the game

`refs/` holds reference stubs — assembly names and signatures, no behaviour — for the
twelve game types the plugin touches. `bash tools/build-refs.sh` builds them, and the
plugin then compiles against those alone. That is what
[the build workflow](.github/workflows/build.yml) does on every push.

It is a compile check and nothing more. A stub that has drifted from the real API
compiles happily and fails when the game loads it, which no runner can see, so the DLL
that ships is always built locally against the real assemblies.

## Contributing a translation

See [FLUSSO.md](FLUSSO.md) for the working loop, in Italian. In short: pick a
conversation with `tools/next_block.py`, fill in the block it writes, and run
`tools/apply.py`, which refuses to merge anything with a broken marker or an
overlong line.

The English text of the game is **not** in this repository: it is copyrighted, and you
regenerate it from your own copy in step 3 above.

## How it works

[ARCHITETTURA.md](ARCHITETTURA.md) documents the plugin, in Italian, including the
things that cost the most to find out — the game keeps two separate language sources
that both have to be updated, the interface reads text through three different paths,
an untranslated line does not merely render blank but stops the dialogue outright, and a
Harmony postfix that answers through a by-reference parameter never reaches a native
caller, so it reports a translation that arrived nowhere.

A few labels are not localized at all and show whatever was typed into the prefab: both
character descriptions on the selection screen, and one notification. The unmodified
game does that in every language. [STATO.md](STATO.md) records how they were found and
what the plugin does about them.

[RICOGNIZIONE.md](RICOGNIZIONE.md) is the technical survey the project started from.

## Licence

The plugin source is MIT. The translated text is a derivative work of the game's script
and is published for use with a legally owned copy of the game; it is not licensed for
redistribution on its own.

*Vampire: The Masquerade — Reckoning of New York* is by
[Draw Distance](https://drawdistance.net/). This project is not affiliated with, endorsed
by, or connected to Draw Distance or Paradox Interactive.
