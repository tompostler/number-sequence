using Microsoft.Extensions.Caching.Memory;
using number_sequence.Exceptions;
using System.Diagnostics;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Services.Libraries
{
    /// <summary>
    /// Chooses the provider for a lookup and caches answers for an hour, misses included. An add page asks about the same
    /// record several times (search, pick, then the form shown again after a validation error), and a provider's data
    /// doesn't change on that scale. Failures aren't cached, so a provider that was down is asked again next time.
    /// <para>
    /// Every result is cut to the item and copy column lengths here, once, so a long title from any provider can't fail
    /// the save it's used for.
    /// </para>
    /// </summary>
    public sealed class LibraryLookup
    {
        private static readonly TimeSpan CacheFor = TimeSpan.FromHours(1);

        private readonly OpenLibraryProvider openLibrary;
        private readonly GoogleBooksProvider googleBooks;
        private readonly TmdbProvider tmdb;
        private readonly BoardGameGeekProvider boardGameGeek;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<LibraryLookup> logger;

        public LibraryLookup(
            OpenLibraryProvider openLibrary,
            GoogleBooksProvider googleBooks,
            TmdbProvider tmdb,
            BoardGameGeekProvider boardGameGeek,
            IMemoryCache memoryCache,
            ILogger<LibraryLookup> logger)
        {
            this.openLibrary = openLibrary;
            this.googleBooks = googleBooks;
            this.tmdb = tmdb;
            this.boardGameGeek = boardGameGeek;
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        /// <summary>
        /// A book by its ISBN-13 from Open Library, or Google Books when Open Library has no record or is down. Null when
        /// neither has it.
        /// </summary>
        public Task<LibraryLookupResult> LookupIsbnAsync(string isbn13, CancellationToken cancellationToken)
            => this.CachedAsync($"isbn:{isbn13}", () => this.LookupIsbnUncachedAsync(isbn13, cancellationToken));

        private async Task<LibraryLookupResult> LookupIsbnUncachedAsync(string isbn13, CancellationToken cancellationToken)
        {
            FailedDependencyException openLibraryFailure = null;
            try
            {
                if (await this.openLibrary.GetByIsbnAsync(isbn13, cancellationToken) is LibraryLookupResult found)
                {
                    return found;
                }
            }
            catch (FailedDependencyException ex)
            {
                openLibraryFailure = ex;
            }

            LibraryLookupResult fallback = await this.googleBooks.GetByIsbnAsync(isbn13, cancellationToken);
            if (fallback == null && openLibraryFailure != null)
            {
                // Google not having it says nothing about Open Library, so this is a failure to report, not a miss to cache.
                throw openLibraryFailure;
            }
            return fallback;
        }

        /// <summary>
        /// Candidates for <paramref name="q"/> as a title. A 400 for a media type that has no lookup or whose provider has no
        /// token configured, with the reason.
        /// </summary>
        public async Task<List<LibraryLookupResult>> SearchAsync(LibraryMediaType mediaType, string q, CancellationToken cancellationToken)
        {
            string term = q.Trim();
            Func<Task<List<LibraryLookupResult>>> search = mediaType switch
            {
                LibraryMediaType.Book => () => this.openLibrary.SearchAsync(term, cancellationToken),
                LibraryMediaType.Movie when this.tmdb.IsConfigured => () => this.tmdb.SearchAsync(term, cancellationToken),
                LibraryMediaType.Movie => throw new BadRequestException("Movie lookup isn't set up: there's no TMDB token configured."),
                LibraryMediaType.BoardGame when this.boardGameGeek.IsConfigured => () => this.boardGameGeek.SearchAsync(term, cancellationToken),
                LibraryMediaType.BoardGame => throw new BadRequestException("Board game lookup isn't set up: there's no BoardGameGeek token configured."),
                _ => throw new BadRequestException($"There's no lookup for {mediaType}."),
            };
            return await this.CachedAsync($"search:{mediaType}:{term.ToLowerInvariant()}", search) ?? [];
        }

        /// <summary>
        /// The full record for a search candidate, or null when the source has no such record.
        /// </summary>
        public async Task<LibraryLookupResult> GetAsync(string source, string sourceId, CancellationToken cancellationToken)
        {
            Func<Task<LibraryLookupResult>> get = source switch
            {
                LibraryLookupSources.OpenLibrary => () => this.openLibrary.GetWorkAsync(sourceId, cancellationToken),
                LibraryLookupSources.TmdbMovie when this.tmdb.IsConfigured => () => this.tmdb.GetMovieAsync(sourceId, cancellationToken),
                LibraryLookupSources.TmdbTv when this.tmdb.IsConfigured => () => this.tmdb.GetTvAsync(sourceId, cancellationToken),
                LibraryLookupSources.TmdbMovie or LibraryLookupSources.TmdbTv => throw new BadRequestException("Movie lookup isn't set up: there's no TMDB token configured."),
                LibraryLookupSources.BoardGameGeek when this.boardGameGeek.IsConfigured => () => this.boardGameGeek.GetAsync(sourceId, cancellationToken),
                LibraryLookupSources.BoardGameGeek => throw new BadRequestException("Board game lookup isn't set up: there's no BoardGameGeek token configured."),
                _ => throw new BadRequestException($"[{source}] isn't a lookup source."),
            };
            return await this.CachedAsync($"get:{source}:{sourceId}", get);
        }

        private async Task<T> CachedAsync<T>(string key, Func<Task<T>> fetch) where T : class
        {
            string cacheKey = $"library-lookup:{key}";
            if (this.memoryCache.TryGetValue(cacheKey, out Cached<T> hit))
            {
                this.logger.LogInformation($"Lookup [{key}] cache hit: {Describe(hit.Value)}.");
                return hit.Value;
            }

            Stopwatch sw = Stopwatch.StartNew();
            T value = await fetch();
            switch (value)
            {
                case LibraryLookupResult result:
                    Fit(result);
                    break;
                case List<LibraryLookupResult> results:
                    results.ForEach(Fit);
                    break;
            }
            _ = this.memoryCache.Set(cacheKey, new Cached<T>(value), CacheFor);
            this.logger.LogInformation($"Lookup [{key}] cache miss: {Describe(value)} in {sw.ElapsedMilliseconds}ms.");
            return value;
        }

        private static string Describe(object value) => value switch
        {
            null => "not found",
            LibraryLookupResult result => $"found {result.Source} [{result.SourceId}]",
            List<LibraryLookupResult> results => $"{results.Count} results",
            _ => "found",
        };

        /// <summary>
        /// Cuts each field to its column on <see cref="LibraryItem"/> or <see cref="LibraryCopy"/>, and drops a source link
        /// that isn't a web address. The add page puts that link in an <c>href</c>, where a provider's <c>javascript:</c>
        /// url would run on this site.
        /// </summary>
        private static void Fit(LibraryLookupResult result)
        {
            if (!Uri.TryCreate(result.SourceUrl, UriKind.Absolute, out Uri sourceUrl) || (sourceUrl.Scheme != Uri.UriSchemeHttps && sourceUrl.Scheme != Uri.UriSchemeHttp))
            {
                result.SourceUrl = null;
            }
            result.Title = LookupText.Fit(result.Title, 256);
            result.Subtitle = LookupText.Fit(result.Subtitle, 256);
            result.Creators = LookupText.Fit(result.Creators, 512);
            result.Series = LookupText.Fit(result.Series, 128);
            result.Summary = LookupText.Fit(result.Summary, LookupText.MaxSummaryLength);
            result.Publisher = LookupText.Fit(result.Publisher, 128);
            result.Format = LookupText.Fit(result.Format, 32);
        }

        /// <summary>
        /// Wraps a cached value so that a cached miss (null) can be told apart from nothing cached.
        /// </summary>
        private sealed record Cached<T>(T Value);
    }
}
