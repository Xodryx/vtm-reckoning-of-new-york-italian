namespace RonyItalian
{
    /// <summary>
    /// Serving the translation on I2's own lookup, shared by the patches that sit on it.
    ///
    /// Parts of the interface are not built by the game's code at all: they are I2
    /// Localize components sitting on the scene objects, which ask I2 directly instead
    /// of going through the game's localization database. The character selection
    /// screen is one of them.
    /// </summary>
    internal static class SceneTranslation
    {
        internal static void Serve(string term, string overrideLanguage, ref string result,
                                   string via)
        {
            if (term == null || !WantsItalian(overrideLanguage))
            {
                return;
            }

            if (!Plugin.Translations.TryGet(term, out var italian))
            {
                return;
            }

            result = italian;
            CharacterPanelText.NoteRequestedTerm(term);
            Diagnostics.SceneTermServed(term, via);
        }

        /// <summary>
        /// A caller that names a language means it, and it is usually asking for English
        /// on purpose. Only an unqualified call follows the player's choice.
        /// </summary>
        internal static bool WantsItalian(string overrideLanguage)
        {
            if (string.IsNullOrEmpty(overrideLanguage))
            {
                return CurrentLanguage.IsItalian;
            }

            return overrideLanguage == LanguageRegistration.LanguageName
                   || overrideLanguage == LanguageRegistration.LanguageCode;
        }
    }
}
