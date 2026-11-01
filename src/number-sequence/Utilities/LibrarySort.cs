using System.Globalization;
using System.Text.RegularExpressions;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Utilities
{
    /// <summary>
    /// Shelf and title order for items (<see cref="LibraryItemSort"/>). Done in memory rather than in sql: a last name has
    /// to be worked out from free-text creators, and a stored sort key would go stale whenever a path that writes an item
    /// forgot it. A library is at most a few thousand items, so sorting every match and then taking the page is cheap.
    /// </summary>
    public static partial class LibrarySort
    {
        /// <summary>
        /// Case and accents ignored, so "Émile" sorts with "Emile".
        /// </summary>
        private static readonly StringComparer Text = StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);

        private static readonly string[] Articles = ["The ", "A ", "An "];

        /// <summary>
        /// Words that belong with the last name rather than the first: "Ursula K. Le Guin" files under L, as a library would.
        /// </summary>
        private static readonly HashSet<string> Particles = new(
            ["da", "de", "del", "della", "den", "der", "des", "di", "du", "la", "le", "ten", "ter", "van", "von"],
            StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Items in <paramref name="sort"/> order. <see cref="LibraryItemSort.Added"/> isn't sorted here: it's a column, so
        /// sql does it.
        /// </summary>
        public static IEnumerable<LibraryItem> Order(IEnumerable<LibraryItem> items, LibraryItemSort sort)
            => sort switch
            {
                LibraryItemSort.Shelf => items
                    .Select(x => (Item: x, Creator: CreatorKey(x.Creators)))
                    // No creator goes after everything that has one, rather than first.
                    .OrderBy(x => x.Creator.Length == 0)
                    .ThenBy(x => x.Creator, Text)
                    .ThenBy(x => TitleKey(string.IsNullOrWhiteSpace(x.Item.Series) ? x.Item.Title : x.Item.Series), Text)
                    .ThenBy(x => !x.Item.SeriesNumber.HasValue)
                    .ThenBy(x => x.Item.SeriesNumber)
                    .ThenBy(x => !x.Item.Year.HasValue)
                    .ThenBy(x => x.Item.Year)
                    .ThenBy(x => TitleKey(x.Item.Title), Text)
                    .ThenBy(x => x.Item.Id)
                    .Select(x => x.Item),
                LibraryItemSort.Title => items
                    .OrderBy(x => TitleKey(x.Title), Text)
                    .ThenBy(x => x.Subtitle ?? string.Empty, Text)
                    .ThenBy(x => x.Id),
                _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, "Sorted by sql, not here."),
            };

        /// <summary>
        /// A title without a leading "The", "A" or "An": <i>The Waste Lands</i> files under W.
        /// </summary>
        public static string TitleKey(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            string trimmed = title.Trim();
            foreach (string article in Articles)
            {
                if (trimmed.Length > article.Length && trimmed.StartsWith(article, StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed[article.Length..].TrimStart();
                }
            }
            return trimmed;
        }

        /// <summary>
        /// The first creator as "Last First": "Robert Jordan; Brandon Sanderson" is "Jordan Robert". A trailing Jr., Sr. or
        /// roman numeral is dropped, a particle stays with the last name, and a name already written "Last, First" is taken
        /// as it is. Empty when there's no creator.
        /// </summary>
        public static string CreatorKey(string creators)
        {
            string first = creators?.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (first == null)
            {
                return string.Empty;
            }

            string name = SuffixRegex().Replace(first, string.Empty).Trim();
            int comma = name.IndexOf(',');
            if (comma > 0)
            {
                return $"{name[..comma].Trim()} {name[(comma + 1)..].Trim()}".Trim();
            }

            string[] words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return string.Empty;
            }
            int start = words.Length - 1;
            // The first word stays a first name, even if it looks like a particle (Van Morrison).
            while (start > 1 && Particles.Contains(words[start - 1]))
            {
                start--;
            }
            return $"{string.Join(' ', words[start..])} {string.Join(' ', words[..start])}".Trim();
        }

        [GeneratedRegex(@",?\s+(?:(?i:jr|sr)\.?|II|III|IV)$")]
        private static partial Regex SuffixRegex();
    }
}
