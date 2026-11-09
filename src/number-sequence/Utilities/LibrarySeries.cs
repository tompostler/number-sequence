namespace number_sequence.Utilities
{
    /// <summary>
    /// Matching a looked-up series to the spelling a library already uses. TMDB collections and BoardGameGeek base games
    /// fill a series in, and sources write one series differently ("Wheel of Time", "The Wheel of Time").
    /// </summary>
    public static class LibrarySeries
    {
        /// <summary>
        /// The spelling of <paramref name="series"/> a library already uses, when one of <paramref name="existing"/> is the
        /// same series written differently ("Wheel of Time" for "The Wheel of Time"), else <paramref name="series"/>. Used
        /// when a lookup fills a series in, so one series doesn't become two.
        /// </summary>
        public static string MatchExisting(string series, IEnumerable<string> existing)
        {
            string key = Key(series);
            if (key.Length == 0)
            {
                return series;
            }
            return existing?.Where(x => Key(x) == key).OrderBy(x => x, StringComparer.Ordinal).FirstOrDefault() ?? series;
        }

        /// <summary>
        /// What two spellings of one series have in common: letters and digits only, lowercased, without a leading "the".
        /// </summary>
        private static string Key(string series)
        {
            string lower = (series ?? string.Empty).Trim().ToLowerInvariant();
            if (lower.StartsWith("the ", StringComparison.Ordinal))
            {
                lower = lower[4..];
            }
            return new string(lower.Where(char.IsLetterOrDigit).ToArray());
        }
    }
}
