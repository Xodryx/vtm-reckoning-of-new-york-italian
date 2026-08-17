"""Merge the translation blocks into the file the plugin reads, checking them first.

Every block in blocks/ is a JSON object mapping a localization key to its Italian
text. This merges them into translations/italian.json and refuses to write anything
if a block contains a mistake that would only show up in game, or not at all.

    python tools/apply.py            # check and write
    python tools/apply.py --check    # check only
    python tools/apply.py --report   # also list the worst length offenders

Markers are checked because breaking one is silent: a mangled <link="Sire"> stops
the glossary from opening, and a lost {[button]} prints a literal placeholder.
Length is checked because the game is fully voiced in English: a subtitle much
longer than its line drifts out of sync with the speech.

The checks need to know the shape of each English line, but the English text is
copyrighted and never versioned, so this keeps a fingerprint of it instead --
tag names, placeholder names, bracket and newline counts, length. That is
structure, not prose: no sentence can be reconstructed from it. With the dump
present the fingerprints are rebuilt from it on every write; without the dump,
the committed copy is used, which is what lets CI run the same check.
"""
import argparse
import glob
import json
import os
import re
import sys

PROJECT_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BLOCKS_DIR = os.path.join(PROJECT_DIR, "blocks")
TERMS_FILE = os.path.join(PROJECT_DIR, "dump", "i2_terms.json")
FINGERPRINTS_FILE = os.path.join(PROJECT_DIR, "reference", "english_fingerprints.json")
OUTPUT_FILE = os.path.join(PROJECT_DIR, "translations", "italian.json")
README_FILE = os.path.join(PROJECT_DIR, "README.md")

# The progress line in the README, kept in step automatically so it cannot drift
# from what is actually translated.
PROGRESS_LINE = re.compile(r"^(> ### Work in progress — )[\d,]+( of )[\d,]+( lines)$",
                           re.MULTILINE)

# Substituted at runtime; must survive untouched.
PLACEHOLDER = re.compile(r"\{\[[^\]]*\]\}")
# TMP rich text. Attributes matter: <link="Sire"> is what opens the glossary entry.
TAG = re.compile(r"<[^>]+>")
# Power names and stage directions. These are meant to be translated, so only the
# count has to match, not the contents.
BRACKET = re.compile(r"\[[^\]]+\]")

# Italian runs longer than English; past this it starts fighting the voice acting.
WARN_RATIO = 1.25
ERROR_RATIO = 1.60
# Short strings swing wildly in ratio, so leave them alone.
MIN_LENGTH_FOR_RATIO = 40


def fingerprint(text):
    """The shape of a string: what must match, and how long it is.

    n length, s whether it holds anything but whitespace, p placeholders,
    t rich text tags, b bracket groups, l newlines. Short names because there is
    one of these per line and there are eleven thousand lines.
    """
    # Strip placeholders before counting brackets: {[X]} is also a bracket group.
    return {
        "n": len(text),
        "s": 1 if text.strip() else 0,
        "p": sorted(PLACEHOLDER.findall(text)),
        "t": sorted(TAG.findall(text)),
        "b": len(BRACKET.findall(PLACEHOLDER.sub("", text))),
        "l": text.count("\n"),
    }


def load_fingerprints():
    """Key -> fingerprint of the English line, and whether it came from the dump.

    Prefers the game's own table. Falls back to the committed fingerprints, which
    is the only thing available on a machine without a copy of the game -- CI, for
    one.
    """
    if os.path.exists(TERMS_FILE):
        with open(TERMS_FILE, encoding="utf-8") as fh:
            source = json.load(fh)
        return {term["Term"]: fingerprint(term["Languages"][0] if term["Languages"] else "")
                for term in source["Terms"]}, True

    if os.path.exists(FINGERPRINTS_FILE):
        with open(FINGERPRINTS_FILE, encoding="utf-8") as fh:
            stored = json.load(fh)
        return stored["keys"], False

    sys.exit(f"manca {TERMS_FILE}\n"
             f"generalo con: python tools/extract_i2.py \"<gioco>/...\\data.unity3d\" dump\n"
             f"oppure committa {FINGERPRINTS_FILE}")


def write_fingerprints(fingerprints):
    """Keeps the committed fingerprints in step with the game's table."""
    payload = {
        "_comment": [
            "Marker fingerprint of every English line, so the checks in tools/apply.py",
            "can run where dump/ is not available -- CI, or a fresh clone. The English",
            "text itself is copyrighted and stays out of the repository; this is only",
            "its shape, and no sentence can be reconstructed from it.",
            "Rebuilt automatically by tools/apply.py whenever dump/ is present.",
            "n length, s non-blank, p placeholders, t tags, b bracket groups, l newlines.",
        ],
        "keys": fingerprints,
    }
    os.makedirs(os.path.dirname(FINGERPRINTS_FILE), exist_ok=True)
    with open(FINGERPRINTS_FILE, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(payload, fh, ensure_ascii=False, indent=1, sort_keys=True)
        fh.write("\n")


def load_blocks():
    """Every block, in filename order. Returns (entries, origin, errors)."""
    entries = {}
    origin = {}
    errors = []

    paths = sorted(glob.glob(os.path.join(BLOCKS_DIR, "*.json")))
    for path in paths:
        name = os.path.basename(path)
        try:
            with open(path, encoding="utf-8") as fh:
                block = json.load(fh)
        except json.JSONDecodeError as exc:
            errors.append(f"{name}: JSON non valido - {exc}")
            continue

        if not isinstance(block, dict):
            errors.append(f"{name}: deve essere un oggetto JSON chiave/testo")
            continue

        for key, text in block.items():
            if not isinstance(text, str):
                errors.append(f"{name}: '{key}' non e' testo")
                continue
            if key in entries and entries[key] != text:
                errors.append(f"{name}: '{key}' gia' tradotta in {origin[key]}, "
                              f"con testo diverso")
                continue
            entries[key] = text
            origin[key] = name

    return entries, origin, errors, paths


def check_entry(key, italian, english, origin):
    """Returns (errors, warnings) for one translated line.

    english is the fingerprint of the English line, not the line itself.
    """
    errors, warnings = [], []
    where = f"{origin} :: {key}"
    mine = fingerprint(italian)

    if english["p"] != mine["p"]:
        errors.append(f"{where}: segnaposto alterati\n"
                      f"    inglese:  {english['p']}\n"
                      f"    italiano: {mine['p']}")

    if english["t"] != mine["t"]:
        errors.append(f"{where}: tag alterati\n"
                      f"    inglese:  {english['t']}\n"
                      f"    italiano: {mine['t']}")

    if english["b"] != mine["b"]:
        errors.append(f"{where}: {english['b']} gruppi fra parentesi quadre in inglese, "
                      f"{mine['b']} in italiano")

    if english["l"] != mine["l"]:
        warnings.append(f"{where}: {english['l']} a capo in inglese, "
                        f"{mine['l']} in italiano")

    if english["n"] >= MIN_LENGTH_FOR_RATIO:
        ratio = mine["n"] / english["n"]
        if ratio > ERROR_RATIO:
            errors.append(f"{where}: troppo lunga, {ratio:.2f}x l'inglese "
                          f"({english['n']} -> {mine['n']} caratteri)")
        elif ratio > WARN_RATIO:
            warnings.append(f"{where}: {ratio:.2f}x l'inglese "
                            f"({english['n']} -> {mine['n']} caratteri)")

    return errors, warnings


def update_readme(done, total):
    """Rewrites the progress line in the README. Silent if it is not there."""
    if not os.path.exists(README_FILE):
        return

    with open(README_FILE, encoding="utf-8") as fh:
        text = fh.read()

    updated, count = PROGRESS_LINE.subn(
        lambda m: f"{m.group(1)}{done:,}{m.group(2)}{total:,}{m.group(3)}", text)

    if count and updated != text:
        with open(README_FILE, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(updated)
        print(f"aggiornato README.md: {done:,} su {total:,}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true", help="controlla senza scrivere")
    parser.add_argument("--report", action="store_true", help="elenca le battute piu' lunghe")
    args = parser.parse_args()

    english_by_key, from_dump = load_fingerprints()
    entries, origin, errors, paths = load_blocks()

    if not paths:
        sys.exit(f"nessun blocco in {BLOCKS_DIR}")

    print(f"{len(paths)} blocco/i, {len(entries)} voce/i")
    if not from_dump:
        print(f"dump/ assente, uso le impronte in {os.path.basename(FINGERPRINTS_FILE)}")

    warnings = []
    translated = {}
    for key, italian in sorted(entries.items()):
        if key not in english_by_key:
            errors.append(f"{origin[key]} :: {key}: chiave inesistente nel gioco")
            continue

        if not italian.strip():
            # Deliberately left for later; the plugin falls back to English.
            continue

        entry_errors, entry_warnings = check_entry(key, italian, english_by_key[key],
                                                   origin[key])
        errors.extend(entry_errors)
        warnings.extend(entry_warnings)
        translated[key] = italian

    for warning in warnings:
        print(f"  avviso: {warning}")

    if errors:
        print(f"\n{len(errors)} errore/i:")
        for error in errors:
            print(f"  {error}")
        sys.exit("\nnon scrivo niente finche' ci sono errori")

    total = sum(1 for shape in english_by_key.values() if shape["s"])
    done_chars = sum(english_by_key[key]["n"] for key in translated)
    total_chars = sum(shape["n"] for shape in english_by_key.values() if shape["s"])

    print(f"\ntradotte {len(translated)} su {total} ({len(translated) / total:.1%})")
    print(f"caratteri {done_chars:,} su {total_chars:,} ({done_chars / total_chars:.1%})")

    if args.report:
        # Only lines long enough to be spoken subtitles: a short menu label at 2x is
        # "Credits" -> "Titoli di coda", which is fine and would just bury the signal.
        ranked = sorted(
            ((key, italian) for key, italian in translated.items()
             if english_by_key[key]["n"] >= MIN_LENGTH_FOR_RATIO),
            key=lambda kv: len(kv[1]) / english_by_key[kv[0]]["n"],
            reverse=True)

        print(f"\nbattute lunghe (da {MIN_LENGTH_FOR_RATIO} caratteri) col rapporto piu' alto:")
        for key, italian in ranked[:10]:
            ratio = len(italian) / english_by_key[key]["n"]
            print(f"  {ratio:.2f}x  {key}")
        if not ranked:
            print("  nessuna")

    if args.check:
        print("\nsolo controllo, non ho scritto niente")
        return

    os.makedirs(os.path.dirname(OUTPUT_FILE), exist_ok=True)
    # newline="\n" to match .gitattributes: writing CRLF here on Windows makes git
    # rewrite the file on every commit, and makes the CI diff check platform-dependent.
    with open(OUTPUT_FILE, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(translated, fh, ensure_ascii=False, indent=1, sort_keys=True)
        fh.write("\n")

    update_readme(len(translated), total)

    if from_dump:
        write_fingerprints(english_by_key)

    print(f"\nscritto {OUTPUT_FILE}")
    print("installa con: bash tools/deploy.sh")


if __name__ == "__main__":
    main()
