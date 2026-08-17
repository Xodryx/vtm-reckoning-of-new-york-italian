using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace RonyItalian
{
    /// <summary>
    /// Italian translation for VtM: Reckoning of New York.
    ///
    /// The plugin adds Italian to the language selector and serves the translated text
    /// as the game asks for it. It never rewrites the game's own language data, and it
    /// never touches a file in the game folder.
    /// </summary>
    [BepInPlugin(Guid, "Reckoning of New York - Italian", Version)]
    public class Plugin : BasePlugin
    {
        public const string Guid = "dev.xodryx.rony.italian";
        // Stays below 1.0 until a person has played through and checked the text: the
        // translation is complete, which is not the same as verified, and this number is
        // the only thing a player sees before downloading.
        public const string Version = "0.9.1";

        internal static ManualLogSource Logger;
        internal static TranslationStore Translations = TranslationStore.Empty();

        /// <summary>
        /// Runs the game untouched while keeping the plugin's diagnostics, which is the
        /// fastest way to tell our own faults apart from the game's.
        /// </summary>
        internal static ConfigEntry<bool> Enabled;

        public override void Load()
        {
            Logger = Log;

            Enabled = Config.Bind(
                "General", "Enabled", true,
                "Set to false to leave the game completely untranslated while keeping the "
                + "plugin's logging, for comparing against the unmodified game.");

            LanguageMemory.Initialize(Config, Log);
            CharacterPanelText.Initialize(Config);
            Diagnostics.Initialize(Config);
            UntranslatedReport.Initialize(Config);
            BakedText.Initialize(Config);

            if (!Enabled.Value)
            {
                Log.LogWarning("disabled by config - the game runs untouched");
                return;
            }

            Translations = TranslationStore.Load(TranslationPath(), Log);

            foreach (var type in PatchClasses())
            {
                try
                {
                    Harmony.CreateAndPatchAll(type);
                    Log.LogInfo($"patched: {type.Name}");
                }
                catch (Exception e)
                {
                    Log.LogError($"patch failed for {type.Name}: {e.Message}");
                }
            }

            Log.LogInfo($"v{Version} ready");
        }

        /// <summary>
        /// Every patch class in this assembly, found by its attribute.
        ///
        /// This used to be a hand-written list, and a patch class left out of it was
        /// simply never applied: no error, no warning, just a plugin that quietly did
        /// less than the source said it did. That cost an afternoon.
        /// </summary>
        private static Type[] PatchClasses()
        {
            var found = new List<Type>();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.GetCustomAttribute<HarmonyPatch>() != null)
                {
                    found.Add(type);
                }
            }

            found.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return found.ToArray();
        }

        /// <summary>The translation file lives next to the plugin assembly.</summary>
        private static string TranslationPath()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(directory ?? ".", TranslationStore.FileName);
        }
    }
}
