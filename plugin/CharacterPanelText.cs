using System;
using BepInEx.Configuration;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace RonyItalian
{
    /// <summary>
    /// Writes the two character descriptions that the game leaves unwritten.
    ///
    /// On the character selection screen neither Description label carries an I2
    /// Localize component, and no code ever assigns to them, so each still shows
    /// whatever was typed into the prefab: English under Kali, and the bare term key
    /// "UI/MainMenu/Rony/PadraicDescription" under Pádraic. The unmodified game does
    /// the same in every language — an English player sees that key too. It is not
    /// something the translation introduced, but on screen it is the translation that
    /// gets the blame.
    ///
    /// The right text does not have to be guessed. The game asks for it as the panel
    /// opens — her description for Kali, and for Pádraic either his description or the
    /// unlock requirement, depending on whether he is unlocked — and then fails to put
    /// it anywhere. All that is missing is the delivery.
    ///
    /// This is the only place where the plugin writes into the scene rather than
    /// answering a lookup, so it has its own switch: set FixCharacterPanel to false to
    /// leave the screen exactly as the game draws it.
    /// </summary>
    internal static class CharacterPanelText
    {
        private const string PanelName = "P_CharacterSelectionUiPanel";
        private const string TitleTerm = "UI/MainMenu/Rony/CharacterSelect";
        private const string LabelName = "Description";

        private const string KaliTerm = "UI/MainMenu/Rony/KaliDescription";
        private const string PadraicTerm = "UI/MainMenu/Rony/PadraicDescription";
        private const string PadraicLockedTerm = "UI/MainMenu/Rony/PadraicUnlockRequirements";

        private static ConfigEntry<bool> _enabled;

        /// <summary>
        /// Which of the two texts belongs on Pádraic's side. The game asks for the one it
        /// means every time the panel opens, so following it is steadier than working out
        /// the unlock rule ourselves — and it keeps working once he is unlocked.
        /// </summary>
        private static string _padraicTerm = PadraicLockedTerm;

        internal static void Initialize(ConfigFile config)
        {
            _enabled = config.Bind(
                "General", "FixCharacterPanel", true,
                "Fills in the two character descriptions on the selection screen, which "
                + "the game itself never writes: without this, Kali's stays in English and "
                + "Pádraic's shows a raw term key. Set to false to leave that screen alone.");
        }

        internal static void NoteRequestedTerm(string term)
        {
            if (term == PadraicTerm || term == PadraicLockedTerm)
            {
                _padraicTerm = term;
            }
        }

        /// <summary>
        /// Runs when the panel's own title is localized, which happens every time the
        /// panel is shown and again on a language change.
        /// </summary>
        internal static void OnLocalized(Localize localize)
        {
            if (_enabled == null || !_enabled.Value || !CurrentLanguage.IsItalian)
            {
                return;
            }

            try
            {
                var term = string.IsNullOrEmpty(localize.FinalTerm)
                    ? localize.Term
                    : localize.FinalTerm;
                if (term != TitleTerm)
                {
                    return;
                }

                var panel = Ancestor(localize.transform, PanelName);
                if (panel == null)
                {
                    return;
                }

                foreach (var label in panel.GetComponentsInChildren<TMP_Text>(true))
                {
                    Fill(label);
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"could not fill the character panel: {e.Message}");
            }
        }

        private static void Fill(TMP_Text label)
        {
            if (label.name != LabelName)
            {
                return;
            }

            var term = TermFor(label.transform);
            if (term == null || !Plugin.Translations.TryGet(term, out var italian))
            {
                return;
            }

            if (label.text == italian)
            {
                return;
            }

            label.text = italian;
            Plugin.Logger.LogInfo($"filled {label.name} with {term}");
        }

        /// <summary>Which character a label belongs to, read off the button above it.</summary>
        private static string TermFor(Transform label)
        {
            for (var node = label; node != null; node = node.parent)
            {
                if (node.name.Contains("Kali"))
                {
                    return KaliTerm;
                }

                if (node.name.Contains("Padraic"))
                {
                    return _padraicTerm;
                }
            }

            return null;
        }

        private static Transform Ancestor(Transform node, string name)
        {
            while (node != null && !node.name.Contains(name))
            {
                node = node.parent;
            }

            return node;
        }
    }
}
