using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace RonyItalian
{
    /// <summary>
    /// Finds the text the translation cannot reach.
    ///
    /// Some labels in this game are not localized at all: they show whatever was typed
    /// into the prefab in the editor, and no language ever changes them. The two
    /// character descriptions were the first ones found, by hand, at the cost of
    /// several evenings; "New Glossary entry unlocked!" was the second, spotted by a
    /// player. There is no reason to keep finding them one at a time.
    ///
    /// A label is suspect when all of this holds as it appears on screen:
    ///  - it is showing something,
    ///  - no I2 Localize component owns it or any of its parents, so nothing will
    ///    correct it later,
    ///  - the text is not one the translation supplied, which rules out everything the
    ///    game writes from the term table,
    ///  - it contains at least one letter, which rules out counters and separators.
    ///
    /// Turn on ReportUntranslatedLabels and play: what the log collects is the list of
    /// everything still to deal with.
    /// </summary>
    internal static class UntranslatedReport
    {
        /// <summary>Enough to survey a whole playthrough, far short of filling a disk.</summary>
        private const int Limit = 400;

        private static readonly HashSet<string> Seen = new HashSet<string>(StringComparer.Ordinal);

        private static ConfigEntry<bool> _enabled;
        private static int _reported;

        internal static void Initialize(ConfigFile config)
        {
            _enabled = config.Bind(
                "General", "ReportUntranslatedLabels", false,
                "Logs every label that appears showing text the translation did not "
                + "provide and that no localization component owns — the text baked into "
                + "the scenes, which cannot be translated through the language table. Off "
                + "by default; turn it on to survey the game, then read the log.");
        }

        internal static void Inspect(TMP_Text label)
        {
            if (_enabled == null || !_enabled.Value || _reported >= Limit)
            {
                return;
            }

            try
            {
                var text = label.text;
                if (string.IsNullOrWhiteSpace(text) || !HasLetter(text))
                {
                    return;
                }

                if (Plugin.Translations.Provided(text) || IsLocalized(label))
                {
                    return;
                }

                var path = Path(label.transform);
                if (!Seen.Add($"{path}\n{text}"))
                {
                    return;
                }

                _reported++;
                Plugin.Logger.LogInfo($"not translated: {path} = '{Excerpt(text)}'");

                if (_reported == Limit)
                {
                    Plugin.Logger.LogWarning(
                        $"reached {Limit} untranslated labels; no more will be reported");
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"could not inspect a label: {e.Message}");
            }
        }

        /// <summary>
        /// Whether anything will localize this label. I2 puts its component either on
        /// the label itself or on an object above it.
        /// </summary>
        private static bool IsLocalized(TMP_Text label)
        {
            return label.GetComponentInParent<Localize>(true) != null;
        }

        private static bool HasLetter(string text)
        {
            foreach (var character in text)
            {
                if (char.IsLetter(character))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The full path, because that is what a fix has to match on.</summary>
        private static string Path(Transform node)
        {
            var path = node.name;
            for (var parent = node.parent; parent != null; parent = parent.parent)
            {
                path = $"{parent.name}/{path}";
            }

            return path;
        }

        private static string Excerpt(string text)
        {
            var single = text.Replace("\n", " ").Replace("\r", "");
            return single.Length > 90 ? single.Substring(0, 90) + "..." : single;
        }
    }
}
