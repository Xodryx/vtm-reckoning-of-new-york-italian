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

namespace UnityEngine
{
    public class Object : Il2CppObjectBase
    {
        public Object(IntPtr pointer) : base(pointer) { }
        public string name { get => throw null; set => throw null; }
    }

    public class Component : Object
    {
        public Component(IntPtr pointer) : base(pointer) { }
        public Transform transform => throw null;
        public GameObject gameObject => throw null;
        public Component GetComponent(Il2CppSystem.Type type) => throw null;
        public Component GetComponentInParent(Il2CppSystem.Type type, bool includeInactive) => throw null;
        public Il2CppReferenceArray<T> GetComponentsInChildren<T>(bool includeInactive)
            where T : Il2CppObjectBase => throw null;
    }

    public class Behaviour : Component
    {
        public Behaviour(IntPtr pointer) : base(pointer) { }
    }

    public class MonoBehaviour : Behaviour
    {
        public MonoBehaviour(IntPtr pointer) : base(pointer) { }
    }

    public class Transform : Component
    {
        public Transform(IntPtr pointer) : base(pointer) { }
        public Transform parent => throw null;
    }

    public class GameObject : Object
    {
        public GameObject(IntPtr pointer) : base(pointer) { }
        public bool activeInHierarchy => throw null;
    }

    public static class Resources
    {
        public static Il2CppReferenceArray<Object> FindObjectsOfTypeAll(Il2CppSystem.Type type) => throw null;
    }

    public static class Time
    {
        public static int frameCount => throw null;
    }
}
