using System;
using System.Collections.Generic;
using BepInEx.Logging;
using DrawDistance.Localization;
using I2.Loc;

namespace RonyItalian
{
    /// <summary>
    /// Makes "Italiano" appear in the game's language selector.
    ///
    /// This is the only change the plugin makes to the game's own data, and it is
    /// unavoidable: the selector is built from the languages present in the I2 source,
    /// via LanguageSetting.CreateOptions -> LocalizationSystem.GetLanguagesData ->
    /// I2LocalizationDatabase.CreateLanguagesData.
    ///
    /// Two subtleties, both discovered the hard way:
    ///  - there is more than one source. The database holds its own LanguageSourceAsset,
    ///    a different object from LocalizationManager.Sources[0], with the same content.
    ///    Registering with only one of them either does nothing visible, or leaves I2
    ///    indexing a language list that is one entry short.
    ///  - the list is cached the first time it is built, so this has to run before
    ///    CreateLanguagesData does.
    /// </summary>
    internal static class LanguageRegistration
    {
        internal const string LanguageName = "Italiano";
        internal const string LanguageCode = "it";

        private static bool _done;

        /// <summary>Column index of Italian, or -1 before registration. Same on every source.</summary>
        internal static int ItalianIndex { get; private set; } = -1;

        /// <summary>Column index of English, used for the fallback.</summary>
        internal static int EnglishIndex { get; private set; } = -1;

        internal static bool IsRegistered => _done;

        internal static void Register(I2LocalizationDatabase database, ManualLogSource log)
        {
            if (_done)
            {
                return;
            }

            try
            {
                var sources = CollectSources(database, log);
                if (sources.Count == 0)
                {
                    log.LogWarning("no language source available yet, will retry on the next hook");
                    return;
                }

                foreach (var source in sources)
                {
                    // Belt and braces for any read path we have not found: without this,
                    // a missing translation renders as the raw term key on screen.
                    source.OnMissingTranslation = LanguageSourceData.MissingTranslationAction.Fallback;

                    if (source.GetLanguageIndex(LanguageName, true, false) < 0)
                    {
                        source.AddLanguage(LanguageName, LanguageCode);
                    }

                    source.UpdateDictionary(true);
                }

                var first = sources[0];
                EnglishIndex = first.GetLanguageIndex("English", true, false);
                ItalianIndex = first.GetLanguageIndex(LanguageName, true, false);

                log.LogInfo($"registered {LanguageName} on {sources.Count} source(s); "
                            + $"english={EnglishIndex}, italian={ItalianIndex}");
                _done = ItalianIndex >= 0;
            }
            catch (Exception e)
            {
                log.LogError($"could not register {LanguageName}: {e}");
            }
        }

        /// <summary>Every distinct LanguageSourceData the game has, deduplicated by pointer.</summary>
        private static List<LanguageSourceData> CollectSources(I2LocalizationDatabase database,
                                                               ManualLogSource log)
        {
            var found = new List<LanguageSourceData>();
            var seen = new HashSet<IntPtr>();

            void Add(LanguageSourceData source)
            {
                if (source != null && seen.Add(source.Pointer))
                {
                    found.Add(source);
                }
            }

            try
            {
                Add(database?._databaseAsset?.mSource);
            }
            catch (Exception e)
            {
                log.LogWarning($"database source unavailable: {e.Message}");
            }

            try
            {
                var sources = LocalizationManager.Sources;
                for (int i = 0; i < sources.Count; i++)
                {
                    Add(sources[i]);
                }
            }
            catch (Exception e)
            {
                log.LogWarning($"LocalizationManager.Sources unavailable: {e.Message}");
            }

            return found;
        }
    }
}
