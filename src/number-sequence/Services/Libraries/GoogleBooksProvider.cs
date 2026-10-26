using System.Text.Json;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Services.Libraries
{
    /// <summary>
    /// Books from Google Books, asked only for an ISBN that Open Library has no record of. Used without a key: that is
    /// allowed at low volume, and as a fallback it rarely gets asked.
    /// <para>
    /// Google only knows the edition, so <see cref="LibraryLookupResult.Year"/> is left empty rather than set to a reprint's
    /// year; the edition's year goes on the copy.
    /// </para>
    /// </summary>
    public sealed class GoogleBooksProvider
    {
        private const string Name = "Google Books";

        private readonly LibraryLookupHttp http;

        public GoogleBooksProvider(LibraryLookupHttp http)
        {
            this.http = http;
        }

        public async Task<LibraryLookupResult> GetByIsbnAsync(string isbn13, CancellationToken cancellationToken)
        {
            using JsonDocument doc = await this.http.GetJsonAsync(Name, $"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn13}&maxResults=1", cancellationToken);
            JsonElement? volume = doc == null ? null : LookupText.Array(doc.RootElement, "items").Select(x => (JsonElement?)x).FirstOrDefault();
            if (!volume.HasValue || !volume.Value.TryGetProperty("volumeInfo", out JsonElement info))
            {
                return null;
            }

            string id = LookupText.Str(volume.Value, "id");
            LibraryLookupResult result = new()
            {
                Source = LibraryLookupSources.GoogleBooks,
                SourceId = id,
                SourceUrl = LookupText.Str(info, "infoLink") ?? (id == null ? null : $"https://books.google.com/books?id={Uri.EscapeDataString(id)}"),
                MediaType = LibraryMediaType.Book,
                Title = LookupText.Str(info, "title"),
                Subtitle = LookupText.Str(info, "subtitle"),
                Creators = LookupText.Creators(LookupText.Strings(info, "authors")),
                Summary = LookupText.Summary(LookupText.Str(info, "description")),
                ExternalIds = id == null ? [] : new() { [LibraryExternalIds.GoogleBooks] = id },
                Attributes = [],
                Barcode = isbn13,
                Publisher = LookupText.Str(info, "publisher"),
                PublishedYear = LookupText.Year(LookupText.Str(info, "publishedDate")),
            };
            if (LookupText.Int(info, "pageCount") is int pages && pages > 0)
            {
                result.Attributes[LibraryAttributes.Pages] = pages.ToString();
            }
            if (info.TryGetProperty("imageLinks", out JsonElement images))
            {
                // Google hands these out as http, with a page-curl effect drawn on unless asked not to.
                result.CoverUrls = new[] { LookupText.Str(images, "thumbnail"), LookupText.Str(images, "smallThumbnail") }
                    .Where(x => x != null)
                    .Select(x => x.Replace("http://", "https://").Replace("&edge=curl", string.Empty))
                    .ToList();
            }
            return result;
        }
    }
}
