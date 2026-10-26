using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace number_sequence.Services.Libraries
{
    /// <summary>
    /// Reading and tidying the loosely shaped data the lookup providers return.
    /// </summary>
    internal static partial class LookupText
    {
        /// <summary>
        /// Matches the longest text column a lookup fills, <c>LibraryItem.Summary</c>.
        /// </summary>
        public const int MaxSummaryLength = 4000;

        public static string Str(JsonElement element, string property)
            => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.String
                ? NullIfBlank(value.GetString())
                : null;

        public static int? Int(JsonElement element, string property)
            => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number)
                ? number
                : null;

        public static IEnumerable<JsonElement> Array(JsonElement element, string property)
            => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.Array
                ? value.EnumerateArray()
                : [];

        public static IEnumerable<string> Strings(JsonElement element, string property)
            => Array(element, property).Where(x => x.ValueKind == JsonValueKind.String).Select(x => NullIfBlank(x.GetString())).Where(x => x != null);

        public static string NullIfBlank(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>
        /// A provider token from config, or null when there isn't one. Azure Pipelines passes a <c>$(name)</c> macro through
        /// unchanged when the variable group has no such secret, so until a token is registered and added to the group, the
        /// app sees the literal macro. That counts as no token rather than one every request would be refused with.
        /// </summary>
        public static string ConfiguredToken(string value)
        {
            string token = NullIfBlank(value);
            return token == null || token.StartsWith("$(", StringComparison.Ordinal) ? null : token;
        }

        /// <summary>
        /// The first four-digit year in a free-form date, such as "1937", "September 21, 1937" or "2001-09-04".
        /// </summary>
        public static int? Year(string date)
        {
            Match match = date == null ? Match.Empty : YearRegex().Match(date);
            return match.Success ? int.Parse(match.Value) : null;
        }

        /// <summary>
        /// Creators as <c>LibraryItem.Creators</c> stores them: semicolon separated, at most a few, so a 40-author anthology
        /// doesn't fill the column.
        /// </summary>
        public static string Creators(IEnumerable<string> names)
        {
            List<string> distinct = names.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToList();
            return distinct.Count == 0 ? null : string.Join("; ", distinct);
        }

        /// <summary>
        /// Plain text from a description that may hold html tags or entities, with runs of blank lines collapsed and cut to
        /// <see cref="MaxSummaryLength"/>.
        /// </summary>
        public static string Summary(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            string plain = TagRegex().Replace(BreakRegex().Replace(text, "\n"), string.Empty);
            plain = WebUtility.HtmlDecode(plain).Replace("\r\n", "\n");
            plain = BlankLinesRegex().Replace(plain, "\n\n").Trim();
            return plain.Length <= MaxSummaryLength ? NullIfBlank(plain) : plain[..(MaxSummaryLength - 1)].TrimEnd() + "…";
        }

        /// <summary>
        /// Cuts <paramref name="value"/> to fit a column of <paramref name="maxLength"/>.
        /// </summary>
        public static string Fit(string value, int maxLength) => value == null || value.Length <= maxLength ? value : value[..maxLength].TrimEnd();

        [GeneratedRegex(@"\b(1[0-9]|20)\d{2}\b")]
        private static partial Regex YearRegex();

        [GeneratedRegex(@"<\s*(br|/p)\s*/?\s*>", RegexOptions.IgnoreCase)]
        private static partial Regex BreakRegex();

        [GeneratedRegex(@"<[^>]+>")]
        private static partial Regex TagRegex();

        [GeneratedRegex(@"\n\s*\n\s*(\n\s*)+")]
        private static partial Regex BlankLinesRegex();
    }
}
