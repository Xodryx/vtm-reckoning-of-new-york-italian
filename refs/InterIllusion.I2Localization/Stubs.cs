// Reference stub. Signatures only, no behaviour: this exists so the plugin can be
// compiled without a copy of the game.
//
// The real type lives in an interop assembly that BepInEx generates from the game's
// IL2CPP metadata on first run. That assembly is a derivative of the game and can
// never be committed, which used to mean the plugin could only be built on a machine
// that owns the game. What the compiler actually needs is much smaller: the names and
// the shapes, twelve types in all.
//
// The assembly name here must match the real one exactly. At runtime the plugin binds
// to the real assembly and never to this one, and these are never shipped.

using System;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace I2.Loc
{
    public class TermData : Il2CppObjectBase
    {
        public TermData(IntPtr pointer) : base(pointer) { }
        public string Term => throw null;
        public Il2CppStringArray Languages => throw null;
        public string GetTranslation(int idx, string specialization, bool editMode) => throw null;
    }

    public class LanguageSourceData : Il2CppObjectBase
    {
        public LanguageSourceData(IntPtr pointer) : base(pointer) { }

        public enum MissingTranslationAction { Empty, Fallback, ShowWarning, ShowTerm }

        public MissingTranslationAction OnMissingTranslation { get => throw null; set => throw null; }
        public Il2CppSystem.Collections.Generic.List<TermData> mTerms => throw null;
        public int GetLanguageIndex(string language, bool AllowDiscartingRegion, bool SkipDisabled) => throw null;
        public void AddLanguage(string LanguageName, string LanguageCode) => throw null;
        public void UpdateDictionary(bool force) => throw null;
        public bool TryGetTranslation(string term, out string Translation, string overrideLanguage,
                                      string overrideSpecialization, bool skipDisabled,
                                      bool allowCategoryMistmatch) => throw null;
    }

    public class LanguageSourceAsset : Il2CppObjectBase
    {
        public LanguageSourceAsset(IntPtr pointer) : base(pointer) { }
        public LanguageSourceData mSource => throw null;
    }

    public static class LocalizationManager
    {
        public static string CurrentLanguage => throw null;
        public static string CurrentLanguageCode => throw null;
        public static Il2CppSystem.Collections.Generic.List<LanguageSourceData> Sources => throw null;
        public static void LocalizeAll(bool Force) => throw null;
        public static string GetTranslation(string Term, bool FixForRTL, int MaxLineLengthForRTL,
                                            bool IgnoreRTLnumbers, bool applyParameters,
                                            GameObject localParametersRoot, string overrideLanguage,
                                            bool allowLocalizedParameters) => throw null;
        public static string GetTermTranslation(string Term, bool FixForRTL, int MaxLineLengthForRTL,
                                                bool IgnoreRTLnumbers, bool applyParameters,
                                                GameObject localParametersRoot, string overrideLanguage,
                                                bool allowLocalizedParameters) => throw null;
    }

    public class Localize : MonoBehaviour
    {
        public Localize(IntPtr pointer) : base(pointer) { }
        public static string MainTranslation => throw null;
        public string Term => throw null;
        public string FinalTerm => throw null;
        public string GetMainTargetsText() => throw null;
        public void OnLocalize(bool Force) => throw null;
    }
}
