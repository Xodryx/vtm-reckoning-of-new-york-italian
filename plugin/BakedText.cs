using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using I2.Loc;
using Il2CppInterop.Runtime;
using TMPro;

namespace RonyItalian
{
    /// <summary>
    /// The handful of strings that exist nowhere but in a prefab.
    ///
    /// Most English text the survey finds is a placeholder that the game overwrites a
    /// moment later — the quest subtitle reads "Main Quest" and then "Missione
    /// principale", and only an instrument ever sees the first one. These are the ones
    /// that stay: the game shows the label without ever writing to it, so the editor's
    /// placeholder is what the player reads.
    ///
    /// There is nothing to look up for them. They are not in the game's term table at
    /// all, so the English has to be matched literally and the Italian written here.
    /// Kept deliberately short: every entry is a small bug in the published game, not a
    /// translation, and each one should be justified in a comment.
    /// </summary>
    internal static class BakedText
    {
        private static readonly Dictionary<string, string> Replacements =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                // The one notification label is shared between the journal and the
                // glossary, and its placeholder shows whenever a code path forgets to
                // fill it. Rendered in the same voice as its siblings
                // (UI/Notifications/NewGlossaryTerm), minus the style tag: nothing
                // substitutes parameters on this path.
                ["New Glossary entry unlocked!"] = "Nuova voce del Glossario sbloccata!",
            };

        private static ConfigEntry<bool> _enabled;

        internal static void Initialize(ConfigFile config)
        {
            _enabled = config.Bind(
                "General", "FixBakedText", true,
                "Replaces the few English strings that exist only inside a prefab and "
                + "that the game never writes over, so no translation can reach them. "
                + "Set to false to leave them as the game shows them.");
        }

        /// <summary>
        /// Runs after I2 has finished with a component, which is the last moment before
        /// the label is read by the player.
        /// </summary>
        internal static void Apply(Localize localize)
        {
            if (_enabled == null || !_enabled.Value || !CurrentLanguage.IsItalian)
            {
                return;
            }

            try
            {
                var label = localize.GetComponent(Il2CppType.Of<TextMeshProUGUI>())
                    ?.TryCast<TextMeshProUGUI>();
                if (label == null)
                {
                    return;
                }

                if (Replacements.TryGetValue(label.text, out var italian))
                {
                    label.text = italian;
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"could not replace a baked string: {e.Message}");
            }
        }
    }
}
