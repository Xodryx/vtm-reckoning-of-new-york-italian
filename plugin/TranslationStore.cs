using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BepInEx.Logging;

namespace RonyItalian
{
    /// <summary>
    /// The Italian text, keyed by localization term.
    ///
    /// Read from disk at every startup so a translator can edit the file and relaunch
    /// without rebuilding the plugin. Deliberately never written into the game's own
    /// language source: filling that many cells crashes the process natively, and
    /// serving the text at read time costs nothing.
    /// </summary>
    internal sealed class TranslationStore
    {
        internal const string FileName = "italian.json";

        private readonly Dictionary<string, string> _entries;

        private TranslationStore(Dictionary<string, string> entries)
        {
            _entries = entries;
        }

        internal int Count => _entries.Count;

        internal bool TryGet(string key, out string value) => _entries.TryGetValue(key, out value);

        internal static TranslationStore Empty() =>
            new TranslationStore(new Dictionary<string, string>(StringComparer.Ordinal));

        /// <summary>
        /// Loads the file, tolerating its absence: a missing or broken file leaves the
        /// game entirely in English rather than breaking it.
        /// </summary>
        internal static TranslationStore Load(string path, ManualLogSource log)
        {
            if (!File.Exists(path))
            {
                log.LogWarning($"no translation file at {path} - the game stays in English");
                return Empty();
            }

            try
            {
                var entries = new Dictionary<string, string>(StringComparer.Ordinal);
                using var document = JsonDocument.Parse(
                    File.ReadAllText(path),
                    new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });

                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    log.LogError($"{FileName} must be a JSON object of \"key\": \"text\" pairs");
                    return Empty();
                }

                var skipped = 0;
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.String)
                    {
                        skipped++;
                        continue;
                    }

                    var text = property.Value.GetString();
                    if (string.IsNullOrEmpty(text))
                    {
                        // An empty value means "not translated yet"; leave it to English.
                        skipped++;
                        continue;
                    }

                    entries[property.Name] = text;
                }

                log.LogInfo($"loaded {entries.Count} translation(s) from {path}"
                            + (skipped > 0 ? $", skipped {skipped} empty or non-text entr(ies)" : ""));
                return new TranslationStore(entries);
            }
            catch (Exception e)
            {
                log.LogError($"could not read {path}: {e.Message}");
                return Empty();
            }
        }
    }
}
