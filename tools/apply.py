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
"""
import argparse
import collections
import glob
import json
import os
import re
import sys

PROJECT_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BLOCKS_DIR = os.path.join(PROJECT_DIR, "blocks")
TERMS_FILE = os.path.join(PROJECT_DIR, "dump", "i2_terms.json")
OUTPUT_FILE = os.path.join(PROJECT_DIR, "translations", "italian.json")

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


def load_english():
    """Key -> English text, straight from the game's own table."""
    if not os.path.exists(TERMS_FILE):
        sys.exit(f"manca {TERMS_FILE}\n"
                 f"generalo con: python tools/extract_i2.py \"<gioco>/...\\data.unity3d\" dump")

    with open(TERMS_FILE, encoding="utf-8") as fh:
        source = json.load(fh)

    return {term["Term"]: (term["Languages"][0] if term["Languages"] else "")
            for term in source["Terms"]}


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


def markers(text):
    """The marker fingerprint of a string, as (placeholders, tags, bracket count)."""
    placeholders = collections.Counter(PLACEHOLDER.findall(text))
    tags = collections.Counter(TAG.findall(text))
    # Strip placeholders first: {[X]} would otherwise also count as a bracket.
    brackets = len(BRACKET.findall(PLACEHOLDER.sub("", text)))
    return placeholders, tags, brackets


def check_entry(key, italian, english, origin):
    """Returns (errors, warnings) for one translated line."""
    errors, warnings = [], []
    where = f"{origin} :: {key}"

    en_placeholders, en_tags, en_brackets = markers(english)
    it_placeholders, it_tags, it_brackets = markers(italian)

    if en_placeholders != it_placeholders:
        errors.append(f"{where}: segnaposto alterati\n"
                      f"    inglese:  {sorted(en_placeholders.elements())}\n"
                      f"    italiano: {sorted(it_placeholders.elements())}")

    if en_tags != it_tags:
        errors.append(f"{where}: tag alterati\n"
                      f"    inglese:  {sorted(en_tags.elements())}\n"
                      f"    italiano: {sorted(it_tags.elements())}")

    if en_brackets != it_brackets:
        errors.append(f"{where}: {en_brackets} gruppi fra parentesi quadre in inglese, "
                      f"{it_brackets} in italiano")

    if english.count("\n") != italian.count("\n"):
        warnings.append(f"{where}: {english.count(chr(10))} a capo in inglese, "
                        f"{italian.count(chr(10))} in italiano")

    if len(english) >= MIN_LENGTH_FOR_RATIO:
        ratio = len(italian) / len(english)
        if ratio > ERROR_RATIO:
            errors.append(f"{where}: troppo lunga, {ratio:.2f}x l'inglese "
                          f"({len(english)} -> {len(italian)} caratteri)")
        elif ratio > WARN_RATIO:
            warnings.append(f"{where}: {ratio:.2f}x l'inglese "
                            f"({len(english)} -> {len(italian)} caratteri)")

    return errors, warnings


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true", help="controlla senza scrivere")
    parser.add_argument("--report", action="store_true", help="elenca le battute piu' lunghe")
    args = parser.parse_args()

    english_by_key = load_english()
    entries, origin, errors, paths = load_blocks()

    if not paths:
        sys.exit(f"nessun blocco in {BLOCKS_DIR}")

    print(f"{len(paths)} blocco/i, {len(entries)} voce/i")

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

    total = sum(1 for text in english_by_key.values() if text.strip())
    done_chars = sum(len(english_by_key[key]) for key in translated)
    total_chars = sum(len(text) for text in english_by_key.values() if text.strip())

    print(f"\ntradotte {len(translated)} su {total} ({len(translated) / total:.1%})")
    print(f"caratteri {done_chars:,} su {total_chars:,} ({done_chars / total_chars:.1%})")

    if args.report:
        # Only lines long enough to be spoken subtitles: a short menu label at 2x is
        # "Credits" -> "Titoli di coda", which is fine and would just bury the signal.
        ranked = sorted(
            ((key, italian) for key, italian in translated.items()
             if len(english_by_key[key]) >= MIN_LENGTH_FOR_RATIO),
            key=lambda kv: len(kv[1]) / len(english_by_key[kv[0]]),
            reverse=True)

        print(f"\nbattute lunghe (da {MIN_LENGTH_FOR_RATIO} caratteri) col rapporto piu' alto:")
        for key, italian in ranked[:10]:
            ratio = len(italian) / len(english_by_key[key])
            print(f"  {ratio:.2f}x  {key}")
        if not ranked:
            print("  nessuna")

    if args.check:
        print("\nsolo controllo, non ho scritto niente")
        return

    os.makedirs(os.path.dirname(OUTPUT_FILE), exist_ok=True)
    with open(OUTPUT_FILE, "w", encoding="utf-8") as fh:
        json.dump(translated, fh, ensure_ascii=False, indent=1, sort_keys=True)
        fh.write("\n")

    print(f"\nscritto {OUTPUT_FILE}")
    print("installa con: bash tools/deploy.sh")


if __name__ == "__main__":
    main()
