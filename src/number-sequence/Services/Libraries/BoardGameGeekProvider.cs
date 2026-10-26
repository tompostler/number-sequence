using Microsoft.Extensions.Options;
using number_sequence.Exceptions;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Services.Libraries
{
    /// <summary>
    /// Board games and expansions from the BoardGameGeek XML API2, which has required a registered application token since
    /// 2025.
    /// <para>
    /// BGG's search returns every game with a matching name, in no useful order, and nothing but names and years. "Catan"
    /// matches hundreds. So the names are ranked here (exact, then prefix, then shortest), the best twenty are fetched in
    /// one <c>thing</c> request, and those are ordered by how many people have rated them, which puts the game most people
    /// mean first.
    /// </para>
    /// </summary>
    public sealed class BoardGameGeekProvider
    {
        private const string Name = "BoardGameGeek";

        /// <summary>
        /// The most ids BGG accepts in one <c>thing</c> request.
        /// </summary>
        private const int MaxThingIds = 20;

        private readonly LibraryLookupHttp http;
        private readonly string token;

        public BoardGameGeekProvider(LibraryLookupHttp http, IOptions<Options.LibraryLookup> options)
        {
            this.http = http;
            this.token = LookupText.ConfiguredToken(options.Value.BggToken);
        }

        public bool IsConfigured => this.token != null;

        public async Task<List<LibraryLookupResult>> SearchAsync(string q, CancellationToken cancellationToken)
        {
            XDocument search = await this.GetXmlAsync($"https://boardgamegeek.com/xmlapi2/search?query={Uri.EscapeDataString(q)}&type=boardgame,boardgameexpansion", cancellationToken);
            if (search == null)
            {
                return [];
            }

            string term = q.Trim();
            List<string> ids = search.Root.Elements("item")
                .Select(x => (Id: (string)x.Attribute("id"), Name: (string)x.Element("name")?.Attribute("value")))
                // The ids go into the thing request's query unescaped.
                .Where(x => x.Id?.Length > 0 && x.Id.All(char.IsAsciiDigit) && x.Name != null)
                .GroupBy(x => x.Id)
                .Select(g => g.OrderBy(x => NameRank(x.Name, term)).First())
                .OrderBy(x => NameRank(x.Name, term))
                .ThenBy(x => x.Name.Length)
                .Take(MaxThingIds)
                .Select(x => x.Id)
                .ToList();
            if (ids.Count == 0)
            {
                return [];
            }

            List<(LibraryLookupResult Result, int Ratings)> things = await this.GetThingsAsync(ids, cancellationToken);
            return things
                .OrderBy(x => NameRank(x.Result.Title, term))
                .ThenByDescending(x => x.Ratings)
                .Take(10)
                .Select(x => x.Result)
                .ToList();
        }

        public async Task<LibraryLookupResult> GetAsync(string id, CancellationToken cancellationToken)
            => int.TryParse(id, out int thingId)
                ? (await this.GetThingsAsync([thingId.ToString()], cancellationToken)).Select(x => x.Result).FirstOrDefault()
                : null;

        private async Task<List<(LibraryLookupResult Result, int Ratings)>> GetThingsAsync(List<string> ids, CancellationToken cancellationToken)
        {
            XDocument doc = await this.GetXmlAsync($"https://boardgamegeek.com/xmlapi2/thing?id={string.Join(",", ids)}&stats=1", cancellationToken);
            if (doc == null)
            {
                return [];
            }

            List<(LibraryLookupResult, int)> results = [];
            foreach (XElement item in doc.Root.Elements("item"))
            {
                string id = (string)item.Attribute("id");
                string title = (string)item.Elements("name").FirstOrDefault(x => (string)x.Attribute("type") == "primary")?.Attribute("value");
                if (id == null || title == null)
                {
                    continue;
                }
                bool expansion = (string)item.Attribute("type") == "boardgameexpansion";

                LibraryLookupResult result = new()
                {
                    Source = LibraryLookupSources.BoardGameGeek,
                    SourceId = id,
                    SourceUrl = $"https://boardgamegeek.com/{(expansion ? "boardgameexpansion" : "boardgame")}/{id}",
                    MediaType = LibraryMediaType.BoardGame,
                    Title = WebUtility.HtmlDecode(title),
                    Year = PositiveInt(item, "yearpublished"),
                    // The description arrives entity-encoded twice ("&amp;#10;" for a newline): xml decoding undoes one level
                    // and this undoes the other.
                    Summary = LookupText.Summary((string)item.Element("description")),
                    Creators = LookupText.Creators(Links(item, "boardgamedesigner").Where(x => x != "(Uncredited)")),
                    // An expansion's inbound expansion link is the game it expands, which is the natural series.
                    Series = expansion ? Links(item, "boardgameexpansion", inbound: true).FirstOrDefault() : null,
                    ExternalIds = new() { [LibraryExternalIds.BoardGameGeek] = id },
                    Attributes = [],
                    CoverUrls = new[] { (string)item.Element("image"), (string)item.Element("thumbnail") }.Select(LookupText.NullIfBlank).Where(x => x != null).ToList(),
                    Format = expansion ? "Expansion" : "Base game",
                };

                int? minPlayers = PositiveInt(item, "minplayers");
                int? maxPlayers = PositiveInt(item, "maxplayers");
                if (minPlayers.HasValue)
                {
                    result.Attributes[LibraryAttributes.Players] = maxPlayers.HasValue && maxPlayers != minPlayers ? $"{minPlayers}–{maxPlayers}" : minPlayers.ToString();
                }
                if (PositiveInt(item, "playingtime") is int playTime)
                {
                    result.Attributes[LibraryAttributes.PlayTimeMinutes] = playTime.ToString();
                }
                if (PositiveInt(item, "minage") is int minAge)
                {
                    result.Attributes[LibraryAttributes.MinAge] = minAge.ToString();
                }

                int ratings = int.TryParse((string)item.Element("statistics")?.Element("ratings")?.Element("usersrated")?.Attribute("value"), out int rated) ? rated : 0;
                results.Add((result, ratings));
            }
            return results;
        }

        private async Task<XDocument> GetXmlAsync(string url, CancellationToken cancellationToken)
        {
            string body = await this.http.GetStringAsync(Name, url, cancellationToken, this.token);
            if (body == null)
            {
                return null;
            }
            try
            {
                XDocument doc = XDocument.Parse(body);
                // A bad request can still be a 200, with <errors> in place of <items>.
                string error = doc.Root?.Name == "errors" ? (string)doc.Root.Element("error")?.Element("message") : null;
                if (error != null)
                {
                    throw new FailedDependencyException($"{Name} refused the request: {error}");
                }
                return doc.Root?.Name == "items" ? doc : null;
            }
            catch (XmlException ex)
            {
                throw new FailedDependencyException($"{Name} returned something that isn't xml.", ex);
            }
        }

        private static int NameRank(string name, string term)
            => string.Equals(name, term, StringComparison.OrdinalIgnoreCase) ? 0
                : name.StartsWith(term, StringComparison.OrdinalIgnoreCase) ? 1
                : name.Contains(term, StringComparison.OrdinalIgnoreCase) ? 2
                : 3;

        private static int? PositiveInt(XElement item, string element)
            => int.TryParse((string)item.Element(element)?.Attribute("value"), out int value) && value > 0 ? value : null;

        private static IEnumerable<string> Links(XElement item, string type, bool inbound = false)
            => item.Elements("link")
                .Where(x => (string)x.Attribute("type") == type && ((string)x.Attribute("inbound") == "true") == inbound)
                .Select(x => (string)x.Attribute("value"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(WebUtility.HtmlDecode);
    }
}
