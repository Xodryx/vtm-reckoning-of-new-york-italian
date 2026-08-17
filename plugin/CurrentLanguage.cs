using System;
using DrawDistance.Localization;

namespace RonyItalian
{
    /// <summary>
    /// Which language the game is showing right now.
    ///
    /// The read patches sit on paths that run for every line of text, so they should not
    /// ask the localization system each time. SetCurrentLanguage is patched to keep this
    /// up to date; until that has run once, the value is fetched on demand.
    /// </summary>
    internal static class CurrentLanguage
    {
        private static Language? _known;

        internal static void Set(Language language)
        {
            _known = language;
        }

        internal static bool IsItalian
        {
            get
            {
                if (_known == null)
                {
                    try
                    {
                        _known = LocalizationSystem.Instance?.CurrentLanguage;
                    }
                    catch
                    {
                        return false;
                    }
                }

                return _known == Language.it;
            }
        }
    }
}
