"""Extract the I2 Localization table from Reckoning of New York.

The game is an IL2CPP build, so the `I2Languages` asset ships without a type
tree and UnityPy cannot deserialise the MonoBehaviour for us. This walks Unity's
binary layout by hand instead. The layout is taken from the Il2CppDumper output
for `I2.Loc.LanguageSourceData` and `I2.Loc.TermData`:

    LanguageSourceData          TermData             LanguageData
      3 bools, each padded to 4    string  Term         string Term
      List<TermData> mTerms        int32   TermType     string Code
      bool  CaseInsensitiveTerms   string[] Languages   byte Flags + bool
      enum  OnMissingTranslation   byte[]  Flags          Compressed, padded
      string mTerm_AppName         int32   (trailing)
      List<LanguageData> mLanguages
      bool IgnoreDeviceLanguage
      enum _AllowUnloadingLanguages
      ...Google sync settings, then List<Object> Assets

Strings are int32 length + UTF-8 bytes padded to a 4-byte boundary; lists are an
int32 count followed by the elements.

Usage:
    python extract_i2.py "<game>/VtM Reckoning of New York_Data/data.unity3d" out/
"""
import argparse
import csv
import json
import os
import struct

import UnityPy

ASSET_NAME = "I2Languages"


class Reader:
    """Cursor over Unity's serialised binary layout."""

    def __init__(self, buf):
        self.buf = buf
        self.pos = 0

    def i32(self):
        value = struct.unpack_from("<i", self.buf, self.pos)[0]
        self.pos += 4
        return value

    def f32(self):
        value = struct.unpack_from("<f", self.buf, self.pos)[0]
        self.pos += 4
        return value

    def align(self):
        self.pos = (self.pos + 3) & ~3

    def string(self):
        length = self.i32()
        if length < 0 or self.pos + length > len(self.buf):
            raise ValueError(f"implausible string length {length} at offset {self.pos - 4}")
        raw = self.buf[self.pos:self.pos + length]
        self.pos += length
        self.align()
        return raw.decode("utf-8", "replace")

    def byte_array(self):
        count = self.i32()
        raw = self.buf[self.pos:self.pos + count]
        self.pos += count
        self.align()
        return raw


def read_raw_asset(bundle_path):
    """Return the raw MonoBehaviour bytes of the I2Languages asset."""
    env = UnityPy.load(bundle_path)
    for obj in env.objects:
        if obj.type.name != "MonoBehaviour":
            continue
        if getattr(obj.read(check_read=False), "m_Name", "") == ASSET_NAME:
            return obj.get_raw_data()
    raise SystemExit(f"{ASSET_NAME} not found in {bundle_path}")


def parse(raw):
    """Parse the asset into its terms, languages and Google-sync settings."""
    r = Reader(raw)
    r.pos = 12          # m_GameObject PPtr
    r.i32()             # m_Enabled
    r.i32()
    r.pos += 8          # m_Script PPtr
    asset_name = r.string()
    r.pos += 12         # bools preceding the term list

    terms = []
    for index in range(r.i32()):
        term = r.string()
        term_type = r.i32()
        languages = [r.string() for _ in range(r.i32())]
        flags = list(r.byte_array())
        r.i32()         # trailing int, unused
        terms.append({"Term": term, "TermType": term_type,
                      "Languages": languages, "Flags": flags})

    r.i32()             # CaseInsensitiveTerms / OnMissingTranslation
    r.i32()
    app_name = r.string()

    languages = []
    for _ in range(r.i32()):
        name, code = r.string(), r.string()
        r.i32()         # LanguageData.Flags + Compressed, padded to 4
        languages.append({"Name": name, "Code": code})

    r.i32()             # IgnoreDeviceLanguage
    r.i32()             # _AllowUnloadingLanguages
    google = {
        "WebServiceURL": r.string(),
        "SpreadsheetKey": r.string(),
        "SpreadsheetName": r.string(),
        "LastUpdatedVersion": r.string(),
        "UpdateFrequency": r.i32(),
        "InEditorCheckFrequency": r.i32(),
        "UpdateSynchronization": r.i32(),
        "UpdateDelay": r.f32(),
    }

    if r.pos + 4 < len(raw):
        raise ValueError(f"parse ended at {r.pos}, expected near {len(raw)} — layout mismatch")

    return {"AssetName": asset_name, "AppName": app_name,
            "Languages": languages, "Google": google, "Terms": terms}


UPDATE_FREQUENCY = {0: "Always", 1: "Never", 2: "Daily", 3: "Weekly",
                    4: "Monthly", 5: "OnlyOnce", 6: "EveryOtherDay"}


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("data_unity3d", help="path to the game's data.unity3d")
    ap.add_argument("out_dir", help="directory to write i2_terms.json / i2_terms.csv into")
    args = ap.parse_args()

    os.makedirs(args.out_dir, exist_ok=True)
    source = parse(read_raw_asset(args.data_unity3d))

    codes = [lang["Code"] for lang in source["Languages"]]
    listed = ", ".join("{Name} [{Code}]".format(**lang) for lang in source["Languages"])
    print(f"sorgente : {source['AssetName']}  ({source['AppName']})")
    print(f"lingue   : {listed}")
    print(f"termini  : {len(source['Terms'])}")
    freq = UPDATE_FREQUENCY.get(source['Google']['UpdateFrequency'], source['Google']['UpdateFrequency'])
    print(f"sync Google: {freq}  (foglio: {source['Google']['SpreadsheetName']})")

    json_path = os.path.join(args.out_dir, "i2_terms.json")
    with open(json_path, "w", encoding="utf-8") as fh:
        json.dump(source, fh, ensure_ascii=False, indent=1)

    csv_path = os.path.join(args.out_dir, "i2_terms.csv")
    with open(csv_path, "w", encoding="utf-8", newline="") as fh:
        writer = csv.writer(fh)
        writer.writerow(["Term", "TermType"] + codes)
        for term in source["Terms"]:
            row = list(term["Languages"]) + [""] * (len(codes) - len(term["Languages"]))
            writer.writerow([term["Term"], term["TermType"]] + row)

    english = [t["Languages"][0] for t in source["Terms"] if t["Languages"] and t["Languages"][0].strip()]
    print(f"\nda tradurre: {len(english)} stringhe, {sum(len(s) for s in english):,} caratteri")
    print(f"scritti {json_path} e {csv_path}")


if __name__ == "__main__":
    main()
