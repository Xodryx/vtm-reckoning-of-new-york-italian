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
using I2.Loc;

namespace DrawDistance.Localization
{
    public class I2LocalizationDatabase : ILocalizationDatabase
    {
        public I2LocalizationDatabase(IntPtr pointer) : base(pointer) { }
        public LanguageSourceAsset _databaseAsset => throw null;
        public string GetValue(string key, Language language) => throw null;

        // A separate method, not a wrapper around the one above: patching only the
        // plain overload left every lookup that passes parameters unanswered.
        public string GetValue(string key, Language language,
                               ILocalizationDatabase.ParameterGetter parameterGetter) => throw null;

        public Il2CppSystem.Collections.Generic.List<LanguageData> CreateLanguagesData() => throw null;
    }
}
