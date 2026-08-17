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

namespace DrawDistance.Settings
{
    public class OptionSetting : Il2CppObjectBase
    {
        public OptionSetting(IntPtr pointer) : base(pointer) { }
        public int GetSettingValueFrom() => throw null;
    }

    public class LanguageSetting : OptionSetting
    {
        public LanguageSetting(IntPtr pointer) : base(pointer) { }
        public int GetDefaultValue() => throw null;
    }
}
