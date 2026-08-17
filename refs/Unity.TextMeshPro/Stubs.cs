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
using UnityEngine;

namespace TMPro
{
    public class TMP_Text : MonoBehaviour
    {
        public TMP_Text(IntPtr pointer) : base(pointer) { }
        public string text { get => throw null; set => throw null; }
    }

    public class TextMeshProUGUI : TMP_Text
    {
        public TextMeshProUGUI(IntPtr pointer) : base(pointer) { }
    }
}
