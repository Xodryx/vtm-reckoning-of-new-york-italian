"""Pick the next lines to translate and write an empty block ready to fill in.

    python tools/next_block.py                 # what is left, by conversation
    python tools/next_block.py R_NIGHT_1       # the next lines of that conversation
    python tools/next_block.py R_NIGHT_1 -n 40

Lines come out in the order the game stores them, which for this game is also the
order they are written in: the keys run LINE-1, LINE-2, LINE-5, LINE-5_2, LINE-6,
LINE-6a... with branches as suffixes. That is a good deal friendlier than
Shadows of New York, where the numbering followed the order nodes were created in
the editor and had to be untangled by walking the dialogue graph.
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

# "M_NIGHT1_MSQ2_LIAISONS-7" and "R_NIGHT4_MSQ_FULLBCODA-:12b" -> conversation name.
LINE_SUFFIX = re.compile(r"-:?\d")


def load_terms():
    """[(key, english)] in the game's own order."""
    if not os.path.exists(TERMS_FILE):
        sys.exit(f"manca {TERMS_FILE} - generalo con tools/extract_i2.py")

    with open(TERMS_FILE, encoding="utf-8") as fh:
        source = json.load(fh)

    return [(term["Term"], term["Languages"][0] if term["Languages"] else "")
            for term in source["Terms"]]


def load_done():
    """Keys already translated in some block."""
    done = set()
    for path in glob.glob(os.path.join(BLOCKS_DIR, "*.json")):
        try:
            with open(path, encoding="utf-8") as fh:
                block = json.load(fh)
        except json.JSONDecodeError:
            continue
        done.update(key for key, text in block.items()
                    if isinstance(text, str) and text.strip())
    return done


def group_of(key):
    """The conversation or interface area a key belongs to.

    Keys come in two shapes: "Dialogue/R_NIGHT_1/LINE-7" and, for a good third of
    them, "Dialogue/M_NIGHT1_MSQ2_LIAISONS-7" with the line number welded onto the
    conversation name. Splitting on "/" alone lumps the second kind into one
    useless bucket of two thousand lines from sixteen different conversations.
    """
    parts = key.split("/")
    if len(parts) > 2:
        return "/".join(parts[:2])
    if len(parts) == 2:
        return parts[0] + "/" + LINE_SUFFIX.split(parts[1])[0]
    return parts[0]


def next_block_number():
    existing = glob.glob(os.path.join(BLOCKS_DIR, "block_*.json"))
    numbers = []
    for path in existing:
        stem = os.path.splitext(os.path.basename(path))[0]
        suffix = stem.rsplit("_", 1)[-1]
        if suffix.isdigit():
            numbers.append(int(suffix))
    return max(numbers, default=0) + 1


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("group", nargs="?", help="conversazione o area, anche parziale")
    parser.add_argument("-n", "--count", type=int, default=40, help="quante battute (default 40)")
    parser.add_argument("--write", action="store_true", help="scrivi il blocco vuoto")
    args = parser.parse_args()

    terms = load_terms()
    done = load_done()
    pending = [(key, english) for key, english in terms
               if english.strip() and key not in done]

    if not args.group:
        counts = collections.Counter()
        chars = collections.Counter()
        for key, english in pending:
            counts[group_of(key)] += 1
            chars[group_of(key)] += len(english)

        print(f"{len(pending)} battute da tradurre, in {len(counts)} gruppi\n")
        print(f"{'gruppo':<45} {'battute':>8} {'caratteri':>11}")
        for group, count in counts.most_common(30):
            print(f"{group:<45} {count:>8} {chars[group]:>11,}")
        print("\nscegline uno:  python tools/next_block.py <gruppo>")
        return

    needle = args.group.lower()
    selected = [(key, english) for key, english in pending if needle in key.lower()]
    if not selected:
        sys.exit(f"nessuna battuta da tradurre per '{args.group}'")

    chunk = selected[:args.count]
    print(f"{len(selected)} da tradurre in '{args.group}', ne mostro {len(chunk)}\n")
    for key, english in chunk:
        print(f"--- {key}")
        print(english)
        print()

    if args.write:
        os.makedirs(BLOCKS_DIR, exist_ok=True)
        path = os.path.join(BLOCKS_DIR, f"block_{next_block_number():03d}.json")
        with open(path, "w", encoding="utf-8") as fh:
            json.dump({key: "" for key, _ in chunk}, fh,
                      ensure_ascii=False, indent=1)
            fh.write("\n")
        print(f"scritto {path} - riempi i valori, poi: python tools/apply.py")


if __name__ == "__main__":
    main()
