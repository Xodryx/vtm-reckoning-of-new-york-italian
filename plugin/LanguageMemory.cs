using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using DrawDistance.Localization;

namespace RonyItalian
{
    /// <summary>
    /// Persists the chosen language on the plugin's behalf.
    ///
    /// The game cannot do it: OptionSetting.GetSaveData() throws an
    /// ArgumentOutOfRangeException that takes the whole SettingsSystem.Save() down with
    /// it, so no setting at all reaches disk. That happens on the unmodified game too,
    /// changing any option — it is not something we introduced, and not something we
    /// can fix from here without touching unrelated systems.
    ///
    /// So the language is stored in the plugin's own config and reapplied at startup.
    /// </summary>
    internal static class LanguageMemory
    {
        private static ConfigEntry<string> _lastLanguage;
        private static ManualLogSource _log;
        private static bool _restored;

        internal static void Initialize(ConfigFile config, ManualLogSource log)
        {
            _log = log;
            _lastLanguage = config.Bind(
                "General", "LastLanguage", "",
                "Language to restore at startup, remembered by the plugin because the game's "
                + "own settings save is broken. Empty means: leave the game's default alone.");
        }

        internal static void Remember(Language language)
        {
            if (_lastLanguage == null || _lastLanguage.Value == language.ToString())
            {
                return;
            }

            _lastLanguage.Value = language.ToString();
            _log?.LogInfo($"remembered language: {language}");
        }

        /// <summary>
        /// Index of the stored language in the settings dropdown, or -1 if there is
        /// nothing to restore.
        ///
        /// This is how the language actually gets restored. Setting it during
        /// LocalizationSystem.Initialize does not survive: the game applies its own
        /// default a moment later, which both reverts the language and — because that
        /// goes through SetCurrentLanguage — overwrites what we had remembered. Feeding
        /// the preference in as the setting's default instead means the game picks
        /// Italian itself, so there is nothing to fight.
        /// </summary>
        internal static int PreferredOptionIndex()
        {
            if (_lastLanguage == null || string.IsNullOrEmpty(_lastLanguage.Value))
            {
                return -1;
            }

            if (!Enum.TryParse<Language>(_lastLanguage.Value, out var stored))
            {
                return -1;
            }

            try
            {
                var languages = LocalizationSystem.Instance?.GetLanguagesData();
                if (languages == null)
                {
                    return -1;
                }

                for (int i = 0; i < languages.Count; i++)
                {
                    if (languages[i].Language == stored)
                    {
                        _log?.LogInfo($"default language set to {stored} (option {i})");
                        return i;
                    }
                }
            }
            catch (Exception e)
            {
                _log?.LogWarning($"could not resolve stored language: {e.Message}");
            }

            return -1;
        }

        /// <summary>Reapplies the stored language, once per session.</summary>
        internal static void Restore(LocalizationSystem system)
        {
            if (_restored || _lastLanguage == null || string.IsNullOrEmpty(_lastLanguage.Value))
            {
                return;
            }

            _restored = true;

            if (!Enum.TryParse<Language>(_lastLanguage.Value, out var stored))
            {
                _log?.LogWarning($"stored language '{_lastLanguage.Value}' is not a known language");
                return;
            }

            if (stored == Language.it && !LanguageRegistration.IsRegistered)
            {
                _log?.LogWarning("Italian is not registered yet, not restoring it");
                return;
            }

            try
            {
                if (system.CurrentLanguage == stored)
                {
                    _log?.LogInfo($"language already {stored}, nothing to restore");
                    return;
                }

                system.SetCurrentLanguage(stored);
                _log?.LogInfo($"restored language: {stored}");
            }
            catch (Exception e)
            {
                _log?.LogError($"could not restore language {stored}: {e.Message}");
            }
        }
    }
}
