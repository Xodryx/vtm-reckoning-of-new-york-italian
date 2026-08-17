using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using DrawDistance.Localization;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace RonyItalian
{
    /// <summary>
    /// The few log lines that are worth their noise.
    ///
    /// The screens built out of I2 Localize components were the hardest thing to
    /// diagnose in this project, because they fail quietly: the text is present, the
    /// term is right, and the wrong language comes out. These two reports say which of
    /// I2's ideas of the current language is in force, and whether that path is being
    /// served at all.
    /// </summary>
    internal static class Diagnostics
    {
        /// <summary>Terms already reported, so a screen is described once and not per frame.</summary>
        private static readonly HashSet<string> Reported = new HashSet<string>();

        /// <summary>Terms whose first service through the scene path is worth a line.</summary>
        private const string InterestingPrefix = "UI/MainMenu/Rony/";

        private static ConfigEntry<bool> _verbose;

        internal static void Initialize(ConfigFile config)
        {
            _verbose = config.Bind(
                "General", "LogLocalizationDetail", false,
                "Reports which read path served each menu term, and lists the labels of "
                + "the character selection panel with their contents. Off by default: it "
                + "is the instrument that found the panel bug, not something a player needs.");
        }

        private static bool Verbose => _verbose != null && _verbose.Value;

        internal static void LanguageChanged(Language language)
        {
            try
            {
                Plugin.Logger.LogInfo(
                    $"language is now {language}; I2 reports "
                    + $"'{LocalizationManager.CurrentLanguage}' ({LocalizationManager.CurrentLanguageCode})");
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"could not read I2's current language: {e.Message}");
            }
        }

        internal static void SceneTermServed(string term, string via)
        {
            if (!Verbose || !term.StartsWith(InterestingPrefix, StringComparison.Ordinal))
            {
                return;
            }

            lock (Reported)
            {
                if (!Reported.Add($"{via}\n{term}"))
                {
                    return;
                }
            }

            Plugin.Logger.LogInfo($"served via {via}: {term}");
        }

        /// <summary>
        /// What a Localize component owns and what it wrote, once per term.
        ///
        /// If nothing is reported for a term the log shows being served, then no
        /// Localize component owns it, and whatever is on screen was put there by
        /// something that never asked us.
        /// </summary>
        internal static void LocalizeRan(Localize localize)
        {
            if (!Verbose)
            {
                return;
            }

            try
            {
                // The term a component carries is not always the one it looks up: I2 lets
                // a prefix and a suffix be glued on. Filtering on the raw one would miss
                // exactly the components that are hardest to find by hand.
                var term = localize.Term;
                var final = localize.FinalTerm;
                var looked = string.IsNullOrEmpty(final) ? term : final;

                if (looked == null || !looked.StartsWith(InterestingPrefix, StringComparison.Ordinal))
                {
                    return;
                }

                lock (Reported)
                {
                    if (!Reported.Add($"localize\n{looked}"))
                    {
                        return;
                    }
                }

                Plugin.Logger.LogInfo(
                    $"Localize on {ObjectPath(localize)}: term={term} final={final} "
                    + $"holds='{Excerpt(Localize.MainTranslation)}' "
                    + $"wrote='{Excerpt(TargetText(localize))}'");

                if (looked == "UI/MainMenu/Rony/CharacterSelect")
                {
                    DumpLabels(localize, "P_CharacterSelectionUiPanel");
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"Localize probe failed: {e.Message}");
            }
        }

        /// <summary>
        /// Every text label under the named panel, with what it currently says.
        ///
        /// This is how a label that nobody localizes gets identified: it is the one
        /// holding English, or holding its own term key, while its neighbours hold
        /// Italian.
        /// </summary>
        private static void DumpLabels(Localize localize, string panelName)
        {
            try
            {
                var panel = localize.transform;
                while (panel != null && !panel.name.Contains(panelName))
                {
                    panel = panel.parent;
                }

                if (panel == null)
                {
                    Plugin.Logger.LogInfo($"no ancestor named {panelName} above {localize.name}");
                    return;
                }

                var labels = panel.GetComponentsInChildren<TMP_Text>(true);
                Plugin.Logger.LogInfo($"{panelName} holds {labels.Length} label(s):");
                foreach (var label in labels)
                {
                    Plugin.Logger.LogInfo(
                        $"  {ObjectPath(label)} = '{Excerpt(label.text)}'");
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"could not list the labels of {panelName}: {e.Message}");
            }
        }

        private static string TargetText(Localize localize)
        {
            try
            {
                return localize.GetMainTargetsText();
            }
            catch (Exception e)
            {
                return $"<unreadable: {e.Message}>";
            }
        }

        /// <summary>Where the object sits in the hierarchy, which is how it gets recognised.</summary>
        private static string ObjectPath(Component component)
        {
            try
            {
                var path = component.name;
                var parent = component.transform?.parent;
                for (int depth = 0; parent != null && depth < 4; depth++)
                {
                    path = $"{parent.name}/{path}";
                    parent = parent.parent;
                }

                return path;
            }
            catch
            {
                return "<unknown>";
            }
        }

        private static string Excerpt(string text)
        {
            if (text == null)
            {
                return "<null>";
            }

            var single = text.Replace("\n", " ").Replace("\r", "");
            return single.Length > 70 ? single.Substring(0, 70) + "..." : single;
        }
    }
}
