using System.Text.Json;
using System.Text.RegularExpressions;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Services.Libraries
{
    /// <summary>
    /// Books from Open Library, which needs no key. Its work id is what recognizes a second edition of a book already in the
    /// library, so it is asked first and Google Books only fills in when it has no record.
    /// <para>
    /// A book's details are spread over three records: the edition (publisher, format, pages, cover), the work (summary,
    /// first publication) and the search index, which carries author names and first publication year already resolved.
    /// Using the search index saves a request per author.
    /// </para>
    /// </summary>
    public sealed partial class OpenLibraryProvider
    {
        private const string Name = "Open Library";
        private const string SearchFields = "key,title,subtitle,author_name,first_publish_year,cover_i,number_of_pages_median";

        private readonly LibraryLookupHttp http;

        public OpenLibraryProvider(LibraryLookupHttp http)
        {
            this.http = http;
        }

        /// <summary>
        /// The edition with this ISBN-13 and its work, or null when Open Library doesn't have it.
        /// </summary>
        public async Task<LibraryLookupResult> GetByIsbnAsync(string isbn13, CancellationToken cancellationToken)
        {
            Task<JsonDocument> editionTask = this.http.GetJsonAsync(Name, $"https://openlibrary.org/isbn/{isbn13}.json", cancellationToken);
            Task<JsonDocument> searchTask = this.http.GetJsonAsync(Name, $"https://openlibrary.org/search.json?isbn={isbn13}&fields={SearchFields}&limit=1", cancellationToken);
            using JsonDocument editionDoc = await editionTask;
            using JsonDocument searchDoc = await searchTask;
            if (editionDoc == null)
            {
                return null;
            }

            JsonElement edition = editionDoc.RootElement;
            string editionWorkKey = LookupText.Array(edition, "works").Select(x => Key(LookupText.Str(x, "key"), WorkKeyRegex())).FirstOrDefault(x => x != null);
            JsonElement? indexed = FirstDoc(searchDoc, editionWorkKey);
            string workKey = editionWorkKey ?? (indexed.HasValue ? Key(LookupText.Str(indexed.Value, "key"), WorkKeyRegex()) : null);

            using JsonDocument workDoc = workKey == null ? null : await this.http.GetJsonAsync(Name, $"https://openlibrary.org{workKey}.json", cancellationToken);
            JsonElement? work = workDoc?.RootElement;

            LibraryLookupResult result = await this.BuildAsync(workKey, work, indexed, edition, cancellationToken);
            result.Barcode = isbn13;
            result.Publisher = LookupText.Strings(edition, "publishers").FirstOrDefault();
            result.PublishedYear = LookupText.Year(LookupText.Str(edition, "publish_date"));
            result.Format = Format(LookupText.Str(edition, "physical_format"));

            string editionKey = Key(LookupText.Str(edition, "key"), EditionKeyRegex());
            if (editionKey != null)
            {
                result.ExternalIds[LibraryExternalIds.OpenLibraryEdition] = editionKey.Replace("/books/", string.Empty);
                result.SourceUrl = $"https://openlibrary.org{editionKey}";
            }
            if (LookupText.Int(edition, "number_of_pages") is int pages && pages > 0)
            {
                result.Attributes[LibraryAttributes.Pages] = pages.ToString();
            }
            if (LookupText.Strings(edition, "series").FirstOrDefault() is string series)
            {
                (result.Series, result.SeriesNumber) = Series(series);
            }

            // The edition's own cover beats the work's, which may be another edition's.
            List<string> covers = CoverUrls(LookupText.Array(edition, "covers"));
            if (covers.Count > 0)
            {
                result.CoverUrls = covers;
            }
            return result;
        }

        /// <summary>
        /// Works matching <paramref name="q"/>, by title or author. Each is a candidate without its summary; fetch the chosen
        /// one with <see cref="GetWorkAsync"/>.
        /// </summary>
        public async Task<List<LibraryLookupResult>> SearchAsync(string q, CancellationToken cancellationToken)
        {
            using JsonDocument searchDoc = await this.http.GetJsonAsync(Name, $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(q)}&fields={SearchFields}&limit=10", cancellationToken);
            if (searchDoc == null)
            {
                return [];
            }

            List<LibraryLookupResult> results = [];
            foreach (JsonElement doc in LookupText.Array(searchDoc.RootElement, "docs"))
            {
                string workKey = Key(LookupText.Str(doc, "key"), WorkKeyRegex());
                if (workKey == null)
                {
                    continue;
                }
                LibraryLookupResult result = NewResult(workKey);
                ApplyIndexed(result, doc);
                results.Add(result);
            }
            return results;
        }

        /// <summary>
        /// A work by its id, such as <c>OL45883W</c>, or null when there's no such work.
        /// </summary>
        public async Task<LibraryLookupResult> GetWorkAsync(string workId, CancellationToken cancellationToken)
        {
            if (!WorkIdRegex().IsMatch(workId))
            {
                return null;
            }
            string workKey = $"/works/{workId}";
            Task<JsonDocument> workTask = this.http.GetJsonAsync(Name, $"https://openlibrary.org{workKey}.json", cancellationToken);
            Task<JsonDocument> searchTask = this.http.GetJsonAsync(Name, $"https://openlibrary.org/search.json?q={Uri.EscapeDataString($"key:{workKey}")}&fields={SearchFields}&limit=1", cancellationToken);
            using JsonDocument workDoc = await workTask;
            using JsonDocument searchDoc = await searchTask;
            if (workDoc == null)
            {
                return null;
            }
            return await this.BuildAsync(workKey, workDoc.RootElement, FirstDoc(searchDoc, workKey), edition: null, cancellationToken);
        }

        /// <summary>
        /// The item fields, from the work where it has them, else the search index, else the edition. Authors fall back to
        /// fetching each author's record when the search index didn't have the book.
        /// </summary>
        private async Task<LibraryLookupResult> BuildAsync(string workKey, JsonElement? work, JsonElement? indexed, JsonElement? edition, CancellationToken cancellationToken)
        {
            LibraryLookupResult result = NewResult(workKey);
            if (indexed.HasValue)
            {
                ApplyIndexed(result, indexed.Value);
            }

            if (work.HasValue)
            {
                result.Title = LookupText.Str(work.Value, "title") ?? result.Title;
                result.Subtitle = LookupText.Str(work.Value, "subtitle") ?? result.Subtitle;
                result.Summary = LookupText.Summary(Description(work.Value));
                result.Year ??= LookupText.Year(LookupText.Str(work.Value, "first_publish_date"));
                if (result.CoverUrls == null && CoverUrls(LookupText.Array(work.Value, "covers")) is { Count: > 0 } workCovers)
                {
                    result.CoverUrls = workCovers;
                }
            }

            if (edition.HasValue)
            {
                result.Title ??= LookupText.Str(edition.Value, "title");
                result.Subtitle ??= LookupText.Str(edition.Value, "subtitle");
                result.Summary ??= LookupText.Summary(Description(edition.Value));
            }

            if (result.Creators == null)
            {
                IEnumerable<string> authorKeys = work.HasValue
                    ? LookupText.Array(work.Value, "authors").Select(x => x.ValueKind == JsonValueKind.Object && x.TryGetProperty("author", out JsonElement author) ? Key(LookupText.Str(author, "key"), AuthorKeyRegex()) : null)
                    : [];
                if (!authorKeys.Any(x => x != null) && edition.HasValue)
                {
                    authorKeys = LookupText.Array(edition.Value, "authors").Select(x => Key(LookupText.Str(x, "key"), AuthorKeyRegex()));
                }
                string[] names = await Task.WhenAll(authorKeys.Where(x => x != null).Distinct().Take(4).Select(x => this.GetAuthorNameAsync(x, cancellationToken)));
                result.Creators = LookupText.Creators(names);
            }

            return result;
        }

        private async Task<string> GetAuthorNameAsync(string authorKey, CancellationToken cancellationToken)
        {
            using JsonDocument author = await this.http.GetJsonAsync(Name, $"https://openlibrary.org{authorKey}.json", cancellationToken);
            return author == null ? null : LookupText.Str(author.RootElement, "name");
        }

        private static LibraryLookupResult NewResult(string workKey)
        {
            string workId = workKey?.Replace("/works/", string.Empty);
            return new()
            {
                Source = LibraryLookupSources.OpenLibrary,
                SourceId = workId,
                SourceUrl = workKey == null ? null : $"https://openlibrary.org{workKey}",
                MediaType = LibraryMediaType.Book,
                ExternalIds = workId == null ? [] : new() { [LibraryExternalIds.OpenLibraryWork] = workId },
                Attributes = [],
            };
        }

        private static void ApplyIndexed(LibraryLookupResult result, JsonElement doc)
        {
            result.Title = LookupText.Str(doc, "title");
            result.Subtitle = LookupText.Str(doc, "subtitle");
            result.Creators = LookupText.Creators(LookupText.Strings(doc, "author_name"));
            result.Year = LookupText.Int(doc, "first_publish_year");
            if (LookupText.Int(doc, "cover_i") is int coverId && coverId > 0)
            {
                result.CoverUrls = [$"https://covers.openlibrary.org/b/id/{coverId}-L.jpg", $"https://covers.openlibrary.org/b/id/{coverId}-M.jpg"];
            }
            if (LookupText.Int(doc, "number_of_pages_median") is int pages && pages > 0)
            {
                result.Attributes[LibraryAttributes.Pages] = pages.ToString();
            }
        }

        /// <summary>
        /// <paramref name="key"/> if it has the shape of an Open Library key, else null. Keys carry their leading slash
        /// (<c>/works/OL45883W</c>), so urls are built as <c>openlibrary.org{key}</c>. They come from Open Library's responses,
        /// so they're checked before going into a url: right after the host, a key like <c>@other.host/x</c> would send the
        /// request to another host, and the lookup client has no host allowlist.
        /// </summary>
        private static string Key(string key, Regex shape) => key != null && shape.IsMatch(key) ? key : null;

        /// <summary>
        /// The search index's record for the book, used for its resolved author names. Only trusted when it is for
        /// <paramref name="workKey"/> (when that's known): a search that didn't understand the query would return some
        /// other book, and its authors would be filled in without anyone noticing.
        /// </summary>
        private static JsonElement? FirstDoc(JsonDocument searchDoc, string workKey)
        {
            JsonElement? doc = searchDoc == null ? null : LookupText.Array(searchDoc.RootElement, "docs").Select(x => (JsonElement?)x).FirstOrDefault();
            return doc.HasValue && (workKey == null || LookupText.Str(doc.Value, "key") == workKey) ? doc : null;
        }

        /// <summary>
        /// Open Library stores a description either as a string or as <c>{"type": "/type/text", "value": "..."}</c>. Many
        /// end with a "----------" line followed by links to other editions, which isn't part of the summary.
        /// </summary>
        private static string Description(JsonElement element)
        {
            string text = LookupText.Str(element, "description");
            if (text == null && element.TryGetProperty("description", out JsonElement description))
            {
                text = LookupText.Str(description, "value");
            }
            int rule = text?.IndexOf("\n----------", StringComparison.Ordinal) ?? -1;
            return rule > 0 ? text[..rule] : text;
        }

        /// <summary>
        /// Cover ids are positive; -1 is Open Library's placeholder for a deleted cover.
        /// </summary>
        private static List<string> CoverUrls(IEnumerable<JsonElement> covers)
        {
            int? coverId = covers.Where(x => x.ValueKind == JsonValueKind.Number && x.TryGetInt32(out int id) && id > 0).Select(x => (int?)x.GetInt32()).FirstOrDefault();
            return coverId.HasValue ? [$"https://covers.openlibrary.org/b/id/{coverId}-L.jpg", $"https://covers.openlibrary.org/b/id/{coverId}-M.jpg"] : [];
        }

        /// <summary>
        /// Maps Open Library's free-text physical format onto the suggested <c>LibraryCopy.Format</c> values. Anything else
        /// is left blank rather than stored in whatever form one cataloguer typed it.
        /// </summary>
        private static string Format(string physicalFormat)
        {
            string format = physicalFormat?.ToLowerInvariant() ?? string.Empty;
            return format switch
            {
                _ when format.Contains("mass market") => "Mass market",
                _ when format.Contains("hardcover") || format.Contains("hardback") || format.Contains("library binding") => "Hardcover",
                _ when format.Contains("paperback") || format.Contains("softcover") || format.Contains("trade paper") => "Paperback",
                _ when format.Contains("audio") => "Audiobook",
                _ when format.Contains("ebook") || format.Contains("e-book") => "eBook",
                _ => null,
            };
        }

        /// <summary>
        /// Splits a series string such as "Discworld (3)" or "Harry Potter #1" into name and number. One with no number
        /// recognizable at the end is kept whole.
        /// </summary>
        private static (string Series, decimal? Number) Series(string series)
        {
            Match match = SeriesRegex().Match(series);
            return match.Success && decimal.TryParse(match.Groups["number"].Value, System.Globalization.CultureInfo.InvariantCulture, out decimal number)
                ? (match.Groups["name"].Value.Trim(), number)
                : (series, null);
        }

        [GeneratedRegex(@"^OL\d+W\z")]
        private static partial Regex WorkIdRegex();

        [GeneratedRegex(@"^/works/OL\d+W\z")]
        private static partial Regex WorkKeyRegex();

        [GeneratedRegex(@"^/books/OL\d+M\z")]
        private static partial Regex EditionKeyRegex();

        [GeneratedRegex(@"^/authors/OL\d+A\z")]
        private static partial Regex AuthorKeyRegex();

        [GeneratedRegex(@"^(?<name>.+?)[\s,;]*(?:\(|#|\bbook\s+|\bvol(?:ume)?\.?\s*|\bno\.?\s*)(?<number>\d+(?:\.\d+)?)\)?\s*$", RegexOptions.IgnoreCase)]
        private static partial Regex SeriesRegex();
    }
}
