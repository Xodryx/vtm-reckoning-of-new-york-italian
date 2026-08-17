using System;
using DrawDistance.Localization;
using DrawDistance.Settings;
using HarmonyLib;
using I2.Loc;
using TMPro;
using UnityEngine;

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
    /// The third read path, and the only one the scenes themselves use.
    ///
    /// Part of the interface is not built by the game's code at all: it is I2's own
    /// Localize component sitting on the object, and that resolves its term straight
    /// through the source, without passing through I2LocalizationDatabase.GetValue or
    /// the term's own GetTranslation. Worse, it picks the column from
    /// LocalizationManager.CurrentLanguage — I2's idea of the language, a plain string
    /// that the game never syncs with its own Language enum. So it reads column 0 and
    /// renders English no matter what the player chose, which is why the character
    /// selection screen stayed English while everything around it was Italian.
    ///
    /// Answering here fixes it without touching either of those two mechanisms.
    /// </summary>
    [HarmonyPatch(typeof(LanguageSourceData), nameof(LanguageSourceData.TryGetTranslation))]
    internal static class SourceTryGetTranslationPatch
    {
        private static void Postfix(string term, ref string Translation,
                                    string overrideLanguage, ref bool __result)
        {
            if (term == null || !SceneTranslation.WantsItalian(overrideLanguage))
            {
                return;
            }

            if (!Plugin.Translations.TryGet(term, out var italian))
            {
                return;
            }

            Translation = italian;
            __result = true;
            Diagnostics.SceneTermServed(term, "source.TryGetTranslation");
        }
    }

    /// <summary>
    /// The same scene path, taken at the top, where the answer comes back by value.
    ///
    /// The chain is LocalizationManager.GetTranslation -> the static TryGetTranslation
    /// -> the source's own. Patching the innermost call is right about the path and
    /// useless in practice: it hands its answer back through a by-reference parameter,
    /// and a postfix writing into one of those does not reach a caller in native code.
    /// The log will happily report a translation that never arrived anywhere.
    ///
    /// This one returns a string, exactly like the other two read patches, and that
    /// does arrive.
    /// </summary>
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation),
        new[] { typeof(string), typeof(bool), typeof(int), typeof(bool), typeof(bool),
                typeof(GameObject), typeof(string), typeof(bool) })]
    internal static class ManagerGetTranslationPatch
    {
        /// <summary>__0 is the term, __6 the language a caller explicitly asked for.</summary>
        private static void Postfix(string __0, string __6, ref string __result)
        {
            SceneTranslation.Serve(__0, __6, ref __result, "LocalizationManager.GetTranslation");
        }
    }

    /// <summary>The other name I2 offers for the same lookup; components use either.</summary>
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTermTranslation),
        new[] { typeof(string), typeof(bool), typeof(int), typeof(bool), typeof(bool),
                typeof(GameObject), typeof(string), typeof(bool) })]
    internal static class ManagerGetTermTranslationPatch
    {
        private static void Postfix(string __0, string __6, ref string __result)
        {
            SceneTranslation.Serve(__0, __6, ref __result, "LocalizationManager.GetTermTranslation");
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
            CurrentLanguage.Set(language);
            LanguageMemory.Remember(language);
            Diagnostics.LanguageChanged(language);
            LanguageRegistration.RefreshComponents(Plugin.Logger);
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
    /// Where I2 finishes localizing a component: the moment its label is settled.
    ///
    /// This is both the report on what each component owns and wrote, and the hook the
    /// character panel needs — the two labels the game never writes have to be filled
    /// after everything else on that screen is done.
    /// </summary>
    [HarmonyPatch(typeof(Localize), nameof(Localize.OnLocalize))]
    internal static class LocalizeOnLocalizePatch
    {
        private static void Postfix(Localize __instance)
        {
            Diagnostics.LocalizeRan(__instance);
            CharacterPanelText.OnLocalized(__instance);
            BakedText.Apply(__instance);
            UntranslatedReport.InspectLocalize(__instance);
            UntranslatedReport.SweepAll();
        }
    }

    /// <summary>
    /// Every label, as it appears on screen. Only does anything when the survey of
    /// untranslated text is switched on.
    /// </summary>
    [HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
    internal static class LabelShownPatch
    {
        private static void Postfix(TextMeshProUGUI __instance)
        {
            UntranslatedReport.Inspect(__instance);
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
