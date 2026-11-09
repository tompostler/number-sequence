using System.Text.Json;
using System.Text.RegularExpressions;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Services.Libraries
{
    /// <summary>
    /// Books from Open Library by title, which needs no key. A search finds works, and the chosen one is fetched as a work.
    /// Its work id is what recognizes a second copy of a book already in the library.
    /// <para>
    /// A work's details are spread over two records: the work (title, summary, first publication, covers) and the search
    /// index, which carries author names and first publication year already resolved. Using the search index saves a
    /// request per author.
    /// </para>
    /// <para>
    /// No series is filled in. Open Library has one for only some works, in whatever form a cataloguer typed, so it's
    /// typed by hand (or set in bulk on the Series page) instead. The title is the work's as Open Library has it, which
    /// can have the series written into it; that's corrected on the add form.
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
            return await this.BuildAsync(workKey, workDoc.RootElement, FirstDoc(searchDoc, workKey), cancellationToken);
        }

        /// <summary>
        /// The item fields, from the work where it has them, else the search index. Authors fall back to fetching each
        /// author's record when the search index didn't have the work.
        /// </summary>
        private async Task<LibraryLookupResult> BuildAsync(string workKey, JsonElement work, JsonElement? indexed, CancellationToken cancellationToken)
        {
            LibraryLookupResult result = NewResult(workKey);
            if (indexed.HasValue)
            {
                ApplyIndexed(result, indexed.Value);
            }

            result.Title = LookupText.Str(work, "title") ?? result.Title;
            result.Subtitle = LookupText.Str(work, "subtitle") ?? result.Subtitle;
            result.Summary = LookupText.Summary(Description(work));
            result.Year ??= LookupText.Year(LookupText.Str(work, "first_publish_date"));
            if (result.CoverUrls == null && CoverUrls(LookupText.Array(work, "covers")) is { Count: > 0 } workCovers)
            {
                result.CoverUrls = workCovers;
            }

            if (result.Creators == null)
            {
                IEnumerable<string> authorKeys = LookupText.Array(work, "authors")
                    .Select(x => x.ValueKind == JsonValueKind.Object && x.TryGetProperty("author", out JsonElement author) ? Key(LookupText.Str(author, "key"), AuthorKeyRegex()) : null);
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

        [GeneratedRegex(@"^OL\d+W\z")]
        private static partial Regex WorkIdRegex();

        [GeneratedRegex(@"^/works/OL\d+W\z")]
        private static partial Regex WorkKeyRegex();

        [GeneratedRegex(@"^/authors/OL\d+A\z")]
        private static partial Regex AuthorKeyRegex();
    }
}
