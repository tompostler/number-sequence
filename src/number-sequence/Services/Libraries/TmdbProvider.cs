using Microsoft.Extensions.Options;
using System.Text.Json;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Services.Libraries
{
    /// <summary>
    /// Movies and TV from TMDB, by title. A disc's UPC doesn't identify anything in TMDB, so a scanned movie keeps its
    /// barcode on the copy and gets its details from a title search. TMDB's terms require crediting it wherever its data is
    /// shown; see <c>_LookupAttribution</c>.
    /// </summary>
    public sealed class TmdbProvider
    {
        private const string Name = "TMDB";

        private readonly LibraryLookupHttp http;
        private readonly string token;

        public TmdbProvider(LibraryLookupHttp http, IOptions<Options.LibraryLookup> options)
        {
            this.http = http;
            this.token = LookupText.ConfiguredToken(options.Value.TmdbToken);
        }

        public bool IsConfigured => this.token != null;

        /// <summary>
        /// Movies and series matching <paramref name="q"/>, as candidates without creators; fetch the chosen one with
        /// <see cref="GetMovieAsync"/> or <see cref="GetTvAsync"/>.
        /// </summary>
        public async Task<List<LibraryLookupResult>> SearchAsync(string q, CancellationToken cancellationToken)
        {
            using JsonDocument doc = await this.http.GetJsonAsync(Name, $"https://api.themoviedb.org/3/search/multi?query={Uri.EscapeDataString(q)}&include_adult=false&page=1", cancellationToken, this.token);
            if (doc == null)
            {
                return [];
            }

            List<LibraryLookupResult> results = [];
            foreach (JsonElement x in LookupText.Array(doc.RootElement, "results"))
            {
                string kind = LookupText.Str(x, "media_type");
                if ((kind != "movie" && kind != "tv") || LookupText.Int(x, "id") is not int id)
                {
                    continue;
                }
                LibraryLookupResult result = NewResult(kind == "tv", id, x);
                result.Summary = LookupText.Summary(LookupText.Str(x, "overview"));
                results.Add(result);
                if (results.Count == 10)
                {
                    break;
                }
            }
            return results;
        }

        public async Task<LibraryLookupResult> GetMovieAsync(string id, CancellationToken cancellationToken)
        {
            if (!int.TryParse(id, out int movieId))
            {
                return null;
            }
            using JsonDocument doc = await this.http.GetJsonAsync(Name, $"https://api.themoviedb.org/3/movie/{movieId}?append_to_response=credits", cancellationToken, this.token);
            if (doc == null)
            {
                return null;
            }

            JsonElement movie = doc.RootElement;
            LibraryLookupResult result = NewResult(tv: false, movieId, movie);
            result.Summary = LookupText.Summary(LookupText.Str(movie, "overview"));
            if (movie.TryGetProperty("credits", out JsonElement credits))
            {
                result.Creators = LookupText.Creators(LookupText.Array(credits, "crew").Where(x => LookupText.Str(x, "job") == "Director").Select(x => LookupText.Str(x, "name")));
            }
            if (movie.TryGetProperty("belongs_to_collection", out JsonElement collection) && LookupText.Str(collection, "name") is string collectionName)
            {
                // TMDB names every collection "... Collection", which as a series name is noise.
                result.Series = collectionName.EndsWith(" Collection", StringComparison.Ordinal) ? collectionName[..^" Collection".Length] : collectionName;
            }
            if (LookupText.Int(movie, "runtime") is int runtime && runtime > 0)
            {
                result.Attributes[LibraryAttributes.RuntimeMinutes] = runtime.ToString();
            }
            AddGenres(result, movie);
            if (LookupText.Str(movie, "imdb_id") is string imdb)
            {
                result.ExternalIds[LibraryExternalIds.Imdb] = imdb;
            }
            return result;
        }

        public async Task<LibraryLookupResult> GetTvAsync(string id, CancellationToken cancellationToken)
        {
            if (!int.TryParse(id, out int tvId))
            {
                return null;
            }
            using JsonDocument doc = await this.http.GetJsonAsync(Name, $"https://api.themoviedb.org/3/tv/{tvId}", cancellationToken, this.token);
            if (doc == null)
            {
                return null;
            }

            JsonElement tv = doc.RootElement;
            LibraryLookupResult result = NewResult(tv: true, tvId, tv);
            result.Summary = LookupText.Summary(LookupText.Str(tv, "overview"));
            result.Creators = LookupText.Creators(LookupText.Array(tv, "created_by").Select(x => LookupText.Str(x, "name")));
            if (LookupText.Int(tv, "number_of_seasons") is int seasons && seasons > 0)
            {
                result.Attributes[LibraryAttributes.Seasons] = seasons.ToString();
            }
            AddGenres(result, tv);
            return result;
        }

        /// <summary>
        /// The fields a search result and a full record share. Movies have <c>title</c> and <c>release_date</c>; series have
        /// <c>name</c> and <c>first_air_date</c>.
        /// </summary>
        private static LibraryLookupResult NewResult(bool tv, int id, JsonElement x)
        {
            string posterPath = LookupText.Str(x, "poster_path");
            return new()
            {
                Source = tv ? LibraryLookupSources.TmdbTv : LibraryLookupSources.TmdbMovie,
                SourceId = id.ToString(),
                SourceUrl = $"https://www.themoviedb.org/{(tv ? "tv" : "movie")}/{id}",
                MediaType = LibraryMediaType.Movie,
                Title = LookupText.Str(x, tv ? "name" : "title"),
                Year = LookupText.Year(LookupText.Str(x, tv ? "first_air_date" : "release_date")),
                ExternalIds = new() { [tv ? LibraryExternalIds.TmdbTv : LibraryExternalIds.TmdbMovie] = id.ToString() },
                Attributes = [],
                CoverUrls = posterPath == null ? null : [$"https://image.tmdb.org/t/p/w780{posterPath}", $"https://image.tmdb.org/t/p/w342{posterPath}"],
            };
        }

        private static void AddGenres(LibraryLookupResult result, JsonElement x)
        {
            List<string> genres = LookupText.Array(x, "genres").Select(g => LookupText.Str(g, "name")).Where(g => g != null).ToList();
            if (genres.Count > 0)
            {
                result.Attributes[LibraryAttributes.Genres] = string.Join(", ", genres);
            }
        }
    }
}
