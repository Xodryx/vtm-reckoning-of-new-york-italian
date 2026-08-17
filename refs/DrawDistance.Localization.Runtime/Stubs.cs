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

namespace DrawDistance.Localization
{
    public enum Language { de, en, es, fr, it, ja, ko, pl, pt_BR, ru, zh_CN, zh_TW }

    public class LanguageData : Il2CppObjectBase
    {
        public LanguageData(IntPtr pointer) : base(pointer) { }
        public Language Language => throw null;
    }

    public class ILocalizationDatabase : Il2CppObjectBase
    {
        public ILocalizationDatabase(IntPtr pointer) : base(pointer) { }
    }

    public class LocalizationSystem : Il2CppObjectBase
    {
        public LocalizationSystem(IntPtr pointer) : base(pointer) { }
        public static LocalizationSystem Instance => throw null;
        public Language CurrentLanguage => throw null;
        public ILocalizationDatabase LocalizationDatabase => throw null;
        public void SetCurrentLanguage(Language language) => throw null;
        public Il2CppSystem.Collections.Generic.List<LanguageData> GetLanguagesData() => throw null;
        public void Initialize() => throw null;
    }
}
