using System.Globalization;
using System.Text.RegularExpressions;

namespace number_sequence.Utilities
{
    /// <summary>
    /// A series name and, when known, the position in it.
    /// </summary>
    public readonly record struct LibrarySeriesPart(string Name, decimal? Number);

    /// <summary>
    /// Finding a series in the free text book sources hand back, and matching it to the spelling a library already uses.
    /// <para>
    /// Open Library has no one place for a series. Its search index has a structured series for some works, an edition may
    /// have a free-text <c>series</c> field, and otherwise the series is written into the title or subtitle by whoever
    /// catalogued that edition: "The Great Hunt (The Wheel of Time Book 2)", "Book Two of 'The Wheel of Time'", "The Dark
    /// Tower III" with the real title in the subtitle. Each is written differently, so these parse the forms seen and leave
    /// anything else alone.
    /// </para>
    /// <para>
    /// Titles are parsed more cautiously than a series field. A title may legitimately end in a number (<i>Fahrenheit
    /// 451</i>), so in a title only a bracketed series with a marker word ("Book", "#", "Vol") or a whole "Book N of X"
    /// subtitle counts. A bare trailing number or numeral only counts in a series field, or in a title when the series
    /// name is already known from somewhere else (<see cref="IsSeriesLabel"/>).
    /// </para>
    /// </summary>
    public static partial class LibrarySeries
    {
        private static readonly string[] NumberWords =
        [
            "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
            "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen", "twenty",
        ];

        /// <summary>
        /// A series field such as "Discworld (3)", "Harry Potter #1", "The Wheel of Time, Book Ten;", "The Dark Tower VII",
        /// "Discworld ; 3" or "H.I.V.E -- [#5]". One with no number recognizable at the end is the name alone. Null for blank
        /// input.
        /// </summary>
        public static LibrarySeriesPart? ParseField(string text)
        {
            string trimmed = Tidy(text);
            if (trimmed == null)
            {
                return null;
            }

            Match match = FieldRegex().Match(trimmed);
            if (match.Success && TryNumber(match.Groups["number"].Value, out decimal number) && Tidy(match.Groups["name"].Value) is string name)
            {
                return new(name, number);
            }
            return new(trimmed, null);
        }

        /// <summary>
        /// Takes a series written into a title or subtitle out of it: a trailing "(The Wheel of Time, Book 6)", or the whole of
        /// "Book Two of 'The Wheel of Time'". Returns the text without it (null when nothing is left) and the series, or the
        /// text unchanged (only trimmed) and null when there's no series in it: tidying is for series text, and would take the
        /// period off "H.I.V.E." and the apostrophe off "'Salem's Lot".
        /// </summary>
        public static (string Text, LibrarySeriesPart? Series) Extract(string text)
        {
            string trimmed = Tidy(text);
            if (trimmed == null)
            {
                return (null, null);
            }

            Match whole = BookOfRegex().Match(trimmed);
            if (whole.Success && TryNumber(whole.Groups["number"].Value, out decimal wholeNumber, romanAnyCase: true) && Tidy(whole.Groups["name"].Value) is string wholeName)
            {
                return (null, new(Capitalize(wholeName), wholeNumber));
            }

            Match bracketed = BracketedRegex().Match(trimmed);
            if (bracketed.Success && TryNumber(bracketed.Groups["number"].Value, out decimal number, romanAnyCase: true) && Tidy(bracketed.Groups["name"].Value) is string name)
            {
                return (Tidy(bracketed.Groups["rest"].Value), new(name, number));
            }

            return (text.Trim(), null);
        }

        /// <summary>
        /// Whether <paramref name="title"/> is only a label for <paramref name="series"/>, with the book's own title in the
        /// subtitle, so the caller uses the subtitle instead. Either the series and a number ("The Dark Tower III" over "The
        /// Waste Lands"), or the series name alone when the book is known to be past the first ("H.I.V.E." over "Escape
        /// Velocity", book 3). The name alone doesn't count for book 1, which is often named for the series (<i>Artemis
        /// Fowl</i>), nor without a subtitle: the seventh Dark Tower book is titled "The Dark Tower". <paramref name="number"/>
        /// is the number in the title, if it had one.
        /// </summary>
        public static bool IsSeriesLabel(string title, string subtitle, LibrarySeriesPart series, out decimal? number)
        {
            number = null;
            if (Tidy(subtitle) == null || Key(series.Name).Length == 0)
            {
                return false;
            }
            if (ParseField(title) is LibrarySeriesPart parsed && parsed.Number.HasValue && Key(parsed.Name) == Key(series.Name))
            {
                number = parsed.Number;
                return true;
            }
            return Key(title) == Key(series.Name) && series.Number > 1;
        }

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
        public static string Key(string series)
        {
            string lower = (series ?? string.Empty).Trim().ToLowerInvariant();
            if (lower.StartsWith("the ", StringComparison.Ordinal))
            {
                lower = lower[4..];
            }
            return new string(lower.Where(char.IsLetterOrDigit).ToArray());
        }

        /// <summary>
        /// A position given on its own, such as the "7" or "1.5" of a structured series, or null when it isn't a number.
        /// </summary>
        public static decimal? ParseNumber(string token) => TryNumber(token?.Trim(), out decimal number, romanAnyCase: true) ? number : null;

        /// <summary>
        /// Digits ("3", "1.5"), a number word up to twenty ("Ten"), or a Roman numeral up to 39. Roman numerals must be
        /// uppercase unless <paramref name="romanAnyCase"/>, which is for text after a marker word such as "Book", where
        /// "Book iv" can't be anything else; at the end of a series field, a lowercase "i" or "v" could be a word.
        /// </summary>
        private static bool TryNumber(string token, out decimal number, bool romanAnyCase = false)
        {
            number = 0;
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            if (decimal.TryParse(token, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number))
            {
                return true;
            }
            int word = Array.IndexOf(NumberWords, token.ToLowerInvariant());
            if (word > 0)
            {
                number = word;
                return true;
            }
            string roman = romanAnyCase ? token.ToUpperInvariant() : token;
            if (RomanRegex().IsMatch(roman))
            {
                number = Roman(roman);
                return true;
            }
            return false;
        }

        private static int Roman(string numeral)
        {
            int total = 0;
            for (int i = 0; i < numeral.Length; i++)
            {
                int value = RomanDigit(numeral[i]);
                total += i + 1 < numeral.Length && RomanDigit(numeral[i + 1]) > value ? -value : value;
            }
            return total;
        }

        private static int RomanDigit(char c) => c switch { 'I' => 1, 'V' => 5, 'X' => 10, _ => 0 };

        /// <summary>
        /// Trimmed of whitespace, the quotes some cataloguers wrap a series in, and trailing punctuation, including the
        /// "--" of a library-catalogue series ("H.i.v.e -- 3"); null when blank.
        /// </summary>
        private static string Tidy(string text)
        {
            string trimmed = text?.Trim().Trim('\'', '"', '‘', '’', '“', '”').TrimEnd(';', ',', '.', ':', '-', '–', '—', ' ').Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }

        private static string Capitalize(string name) => name.StartsWith("the ", StringComparison.Ordinal) ? "T" + name[1..] : name;

        private const string Number = @"\d+(?:\.\d+)?|[IVXivx]+|[A-Za-z]+";
        private const string Separator = @"(?:\s+[-–—]+\s*|[\s,;:]*)";
        private const string Marker = @"(?:#|\bbook\b|\bbk\.?|\bvol(?:ume)?\.?|\bno\.?|\bpart\b)";

        /// <summary>
        /// A name, then a number after a marker, inside brackets, or after a separator. A dash only separates with space
        /// around it ("H.i.v.e -- 3"), so "Spider-Man 2" keeps its name.
        /// </summary>
        [GeneratedRegex(@"^(?<name>.+?)(?:" + Separator + @"[(\[]?\s*" + Marker + @"\s*|" + Separator + @"[(\[]\s*|(?:\s+[-–—]+\s+|[\s,;:]+))(?<number>" + Number + @")\s*[)\]]?$", RegexOptions.IgnoreCase)]
        private static partial Regex FieldRegex();

        /// <summary>
        /// "Book Two of 'The Wheel of Time'", "Book 3 of the Riftwar Saga", "Volume IV in The Dark Tower".
        /// </summary>
        [GeneratedRegex(@"^(?:book|vol(?:ume)?\.?|part)\s+(?<number>" + Number + @")\s+(?:of|in)\s+(?<name>.+)$", RegexOptions.IgnoreCase)]
        private static partial Regex BookOfRegex();

        /// <summary>
        /// Anything, then a bracketed series with a marker at the end: "(The Wheel of Time, Book 6)", "[Discworld #3]".
        /// </summary>
        [GeneratedRegex(@"^(?<rest>.*?)\s*[(\[]\s*(?<name>[^()\[\]]+?)[\s,;:]*" + Marker + @"\s*(?<number>" + Number + @")\s*[)\]]$", RegexOptions.IgnoreCase)]
        private static partial Regex BracketedRegex();

        [GeneratedRegex(@"^(?=[IVX])X{0,3}(?:IX|IV|V?I{0,3})$")]
        private static partial Regex RomanRegex();
    }
}
