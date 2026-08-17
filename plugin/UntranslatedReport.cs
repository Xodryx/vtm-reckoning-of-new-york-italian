using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using I2.Loc;
using Il2CppInterop.Runtime;
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

        /// <summary>Frames between full sweeps: often enough to catch a banner, rare
        /// enough that walking every loaded label costs nothing noticeable.</summary>
        private const int SweepEvery = 300;

        private static ConfigEntry<bool> _enabled;
        private static int _reported;
        private static bool _broken;
        private static int _sweptOnFrame = -SweepEvery;

        internal static void Initialize(ConfigFile config)
        {
            _enabled = config.Bind(
                "General", "ReportUntranslatedLabels", false,
                "Logs every label that appears showing text the translation did not "
                + "provide and that no localization component owns — the text baked into "
                + "the scenes, which cannot be translated through the language table. Off "
                + "by default; turn it on to survey the game, then read the log.");
        }

        /// <summary>
        /// Every label loaded, active or not, at most once every few seconds.
        ///
        /// Watching labels switch on has a blind spot exactly where it matters: a
        /// notification banner is usually built once and reused, so it never switches on
        /// again and is never seen. Asking Unity for everything loaded closes that, and
        /// an inactive object still carries the text baked into its prefab — which is the
        /// text this survey is looking for.
        /// </summary>
        internal static void SweepAll()
        {
            if (_broken || _enabled == null || !_enabled.Value || _reported >= Limit)
            {
                return;
            }

            if (Time.frameCount - _sweptOnFrame < SweepEvery)
            {
                return;
            }

            _sweptOnFrame = Time.frameCount;

            try
            {
                var loaded = Resources.FindObjectsOfTypeAll(Il2CppType.Of<TextMeshProUGUI>());
                foreach (var found in loaded)
                {
                    var label = found.TryCast<TextMeshProUGUI>();
                    if (label != null)
                    {
                        Inspect(label);
                    }
                }
            }
            catch (Exception e)
            {
                _broken = true;
                Plugin.Logger.LogError($"the sweep for untranslated labels failed: {e}");
            }
        }

        /// <summary>
        /// A localization component holding something that is neither a term we can
        /// translate nor text we produced.
        ///
        /// This game assigns the finished text to a Localize component instead of a
        /// term key, so what a component carries is usually a whole sentence. When that
        /// sentence is one of ours, all is well. When it is not, either the game wrote
        /// English there, or nobody wrote anything and the prefab's own placeholder is
        /// showing. The sweep over labels cannot see any of this, because it skips
        /// exactly the labels something is supposed to be looking after.
        ///
        /// Most of what this reports is harmless: a placeholder replaced a frame later,
        /// visible to an instrument and never to a player. Worth checking one by one
        /// against the term table before believing any of it.
        /// </summary>
        internal static void InspectLocalize(Localize localize)
        {
            if (_broken || _enabled == null || !_enabled.Value || _reported >= Limit)
            {
                return;
            }

            try
            {
                var term = localize.FinalTerm;
                if (string.IsNullOrEmpty(term))
                {
                    term = localize.Term;
                }

                if (string.IsNullOrEmpty(term)
                    || Plugin.Translations.TryGet(term, out _)
                    || Plugin.Translations.Provided(term))
                {
                    return;
                }

                if (!Seen.Add($"term\n{term}"))
                {
                    return;
                }

                _reported++;
                Plugin.Logger.LogInfo(
                    $"not ours: '{Excerpt(term)}' on {Path(localize.transform)}");
            }
            catch (Exception e)
            {
                _broken = true;
                Plugin.Logger.LogError($"the check on localized terms failed: {e}");
            }
        }

        internal static void Inspect(TMP_Text label)
        {
            if (_broken || _enabled == null || !_enabled.Value || _reported >= Limit)
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
                var state = label.gameObject.activeInHierarchy ? "" : " [spenta]";
                Plugin.Logger.LogInfo($"not translated{state}: {path} = '{Excerpt(text)}'");

                if (_reported == Limit)
                {
                    Plugin.Logger.LogWarning(
                        $"reached {Limit} untranslated labels; no more will be reported");
                }
            }
            catch (Exception e)
            {
                // A survey that fails on every label while quietly logging warnings reads
                // as a survey that found nothing. Stop, and say so where it cannot be
                // mistaken for a clean result.
                _broken = true;
                Plugin.Logger.LogError(
                    $"the untranslated-label survey failed and has been switched off for "
                    + $"this session; its findings so far are NOT complete: {e}");
            }
        }

        /// <summary>
        /// Whether anything will localize this label. I2 puts its component either on
        /// the label itself or on an object above it.
        ///
        /// Deliberately not the generic GetComponentInParent&lt;Localize&gt;: under
        /// Il2CppInterop that one throws a constraint violation on every call, and since
        /// the survey caught its own exceptions it collected nothing at all while looking
        /// like it was working. The overload taking a type does the same job quietly.
        /// </summary>
        private static bool IsLocalized(TMP_Text label)
        {
            return label.GetComponentInParent(Il2CppType.Of<Localize>(), true) != null;
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
