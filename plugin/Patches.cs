using System;
using DrawDistance.Localization;
using DrawDistance.Settings;
using HarmonyLib;
using I2.Loc;

namespace RonyItalian
{
    /// <summary>
    /// Registers Italian before the language list is built and cached.
    /// </summary>
    [HarmonyPatch(typeof(I2LocalizationDatabase), "CreateLanguagesData")]
    internal static class CreateLanguagesDataPatch
    {
        private static void Prefix(I2LocalizationDatabase __instance)
        {
            LanguageRegistration.Register(__instance, Plugin.Logger);
        }
    }

    /// <summary>
    /// Earliest point at which the localization system is usable. InitializeLanguages,
    /// despite the name, is never called at startup.
    /// </summary>
    [HarmonyPatch(typeof(LocalizationSystem), "Initialize")]
    internal static class LocalizationSystemInitializePatch
    {
        private static void Postfix(LocalizationSystem __instance)
        {
            var database = __instance.LocalizationDatabase?.TryCast<I2LocalizationDatabase>();
            if (database == null)
            {
                Plugin.Logger.LogError("LocalizationDatabase is not an I2LocalizationDatabase");
                return;
            }

            LanguageRegistration.Register(database, Plugin.Logger);
            LanguageMemory.Restore(__instance);
        }
    }

    /// <summary>
    /// Where the Italian text is actually served.
    ///
    /// The plugin holds the translations in its own dictionary and answers here, so the
    /// game's data is never rewritten. Anything untranslated falls back to English: I2
    /// widens each term's array with <c>null</c> when a language is added, and the game
    /// returns that null unchanged, which makes AutoSkipController.ResetTime() throw
    /// while measuring the line and stops the dialogue dead.
    /// </summary>
    [HarmonyPatch(typeof(I2LocalizationDatabase), "GetValue", new[] { typeof(string), typeof(Language) })]
    internal static class GetValuePatch
    {
        private static void Postfix(I2LocalizationDatabase __instance, string key,
                                    Language language, ref string __result)
        {
            if (language != Language.it)
            {
                return;
            }

            if (key != null && Plugin.Translations.TryGet(key, out var italian))
            {
                __result = italian;
                return;
            }

            if (string.IsNullOrEmpty(__result))
            {
                // language is it, so this recursive call returns immediately from the guard.
                __result = __instance.GetValue(key, Language.en) ?? string.Empty;
            }
        }
    }

    /// <summary>
    /// The other read path, and it is not optional.
    ///
    /// Part of the interface goes through the game's own database wrapper, and part
    /// reads I2 directly — the settings screen shows both at once. On this path a
    /// missing Italian cell surfaces as the raw term key on screen
    /// ("UI/Settings/Video/Title"), because the source's OnMissingTranslation is
    /// ShowTerm, and our translations would never arrive at all.
    ///
    /// TermData carries its own key, so the same lookup works here.
    /// </summary>
    [HarmonyPatch(typeof(TermData), nameof(TermData.GetTranslation))]
    internal static class TermGetTranslationPatch
    {
        private static void Postfix(TermData __instance, int idx, ref string __result)
        {
            if (idx != LanguageRegistration.ItalianIndex)
            {
                return;
            }

            var key = __instance.Term;
            if (key != null && Plugin.Translations.TryGet(key, out var italian))
            {
                __result = italian;
                return;
            }

            if (!string.IsNullOrEmpty(__result))
            {
                return;
            }

            var english = LanguageRegistration.EnglishIndex;
            var languages = __instance.Languages;
            if (english >= 0 && languages != null && english < languages.Length)
            {
                __result = languages[english];
            }
        }
    }

    /// <summary>
    /// Remembers the chosen language, because the game cannot: OptionSetting.GetSaveData()
    /// throws and takes the whole SettingsSystem.Save() down with it, so nothing the
    /// player picks is ever written to disk. That is a fault of the unmodified game.
    /// </summary>
    [HarmonyPatch(typeof(LocalizationSystem), nameof(LocalizationSystem.SetCurrentLanguage))]
    internal static class SetCurrentLanguagePatch
    {
        private static void Postfix(Language language)
        {
            LanguageMemory.Remember(language);
        }
    }

    /// <summary>
    /// Restores the remembered language as the setting's default. Only reached on a
    /// profile that has never saved a language.
    /// </summary>
    [HarmonyPatch(typeof(LanguageSetting), "GetDefaultValue")]
    internal static class LanguageSettingDefaultPatch
    {
        private static void Postfix(ref int __result)
        {
            var preferred = LanguageMemory.PreferredOptionIndex();
            if (preferred >= 0)
            {
                __result = preferred;
            }
        }
    }

    /// <summary>
    /// Restores the remembered language on the path that actually runs.
    ///
    /// Saving settings is broken in the unmodified game, but *loading* is not, and the
    /// existing settings file pins the language to whatever was stored the last time
    /// saving still worked. So the default above is never consulted, and the stale value
    /// wins. Overriding it here is what makes the choice survive a restart.
    /// </summary>
    [HarmonyPatch(typeof(OptionSetting), "GetSettingValueFrom")]
    internal static class OptionSettingValuePatch
    {
        private static void Postfix(OptionSetting __instance, ref int __result)
        {
            if (__instance.TryCast<LanguageSetting>() == null)
            {
                return;
            }

            var preferred = LanguageMemory.PreferredOptionIndex();
            if (preferred >= 0)
            {
                __result = preferred;
            }
        }
    }

    /// <summary>
    /// Diagnostic net. ResetTime is where a null line used to kill the dialogue; if it
    /// ever throws again we want the key in the log, not a bare stack trace.
    /// </summary>
    [HarmonyPatch(typeof(DrawDistance.Rony.Runtime.AutoSkipController), "ResetTime")]
    internal static class AutoSkipResetTimePatch
    {
        private static Exception Finalizer(Exception __exception,
                                           DrawDistance.Rony.Runtime.AutoSkipController __instance)
        {
            if (__exception == null)
            {
                return null;
            }

            try
            {
                var key = __instance._cachedLocalizationKey;
                Plugin.Logger.LogError(
                    $"AutoSkipController.ResetTime threw on key {(key == null ? "<null>" : $"'{key}'")}: "
                    + __exception.Message);
            }
            catch
            {
                // Diagnostics must never make things worse.
            }

            return __exception;
        }
    }
}
