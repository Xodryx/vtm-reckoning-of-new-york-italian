# Vampire: The Masquerade — Reckoning of New York, in Italian

An unofficial Italian translation of *Reckoning of New York*, delivered as a BepInEx
plugin. The game ships English and French only; this adds **Italiano** as a third entry
in the language selector and serves the translated text at runtime. **No game file is
modified.**

> ### Work in progress — 7,430 of 11,141 lines
>
> The plugin works end to end: Italian appears in the language selector, the choice
> survives a restart, and translated lines show up in menus and dialogue. What is
> missing is the translation itself. Anything not yet translated stays in English, so
> the game remains fully playable at every point.

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

Not yet — there is no release worth installing. When there is, it will be
[here](../../releases/latest).

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

   Set `GAME_DIR` if your working copy is not at `~/Documents/RoNY-game-copy`.

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
that both have to be updated, the interface reads text through two different paths, and
an untranslated line does not merely render blank but stops the dialogue outright.

[RICOGNIZIONE.md](RICOGNIZIONE.md) is the technical survey the project started from.

## Licence

The plugin source is MIT. The translated text is a derivative work of the game's script
and is published for use with a legally owned copy of the game; it is not licensed for
redistribution on its own.

*Vampire: The Masquerade — Reckoning of New York* is by
[Draw Distance](https://drawdistance.net/). This project is not affiliated with, endorsed
by, or connected to Draw Distance or Paradox Interactive.
