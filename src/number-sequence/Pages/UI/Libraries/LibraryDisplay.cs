using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries
{
    /// <summary>
    /// The model for <c>_ItemCard</c>, one search result. <paramref name="LibraryName"/> is shown when results span
    /// libraries.
    /// </summary>
    public sealed record ItemCard(LibraryItem Item, IReadOnlyDictionary<long, string> Paths, string LibraryName = null);

    /// <summary>
    /// Formatting shared by the library pages.
    /// </summary>
    public static class LibraryDisplay
    {
        public static string MediaType(LibraryMediaType mediaType) => mediaType switch
        {
            LibraryMediaType.BoardGame => "Board game",
            _ => mediaType.ToString(),
        };

        /// <summary>
        /// Suggested <see cref="LibraryCopy.Format"/> values for a media type, offered in a datalist. Free text is still
        /// accepted.
        /// </summary>
        public static string[] FormatSuggestions(LibraryMediaType? mediaType) => mediaType switch
        {
            LibraryMediaType.Book => ["Hardcover", "Paperback", "Mass market", "eBook", "Audiobook"],
            LibraryMediaType.Movie => ["DVD", "Blu-ray", "4K UHD", "VHS"],
            LibraryMediaType.BoardGame => ["Base game", "Expansion"],
            _ => [],
        };

        /// <summary>
        /// The stored cover, served by the api directly (the token cookie authenticates it). The modified date in the query
        /// changes when the cover does, which is what lets the api cache it for a month.
        /// </summary>
        public static string CoverUrl(LibraryItem item, bool thumbnail = false)
            => $"/library/libraries/{item.LibraryId}/items/{item.Id}/cover?v={item.ModifiedDate.ToUnixTimeSeconds()}{(thumbnail ? "&thumbnail=true" : string.Empty)}";

        /// <summary>
        /// A lookup result's cover, as a thumbnail fetched through this site rather than hotlinked.
        /// </summary>
        public static string LookupCoverUrl(string url) => $"/library/lookup/cover?url={Uri.EscapeDataString(url)}";

        public static string LookupSourceName(string source) => source switch
        {
            LibraryLookupSources.OpenLibrary => "Open Library",
            LibraryLookupSources.GoogleBooks => "Google Books",
            LibraryLookupSources.TmdbMovie or LibraryLookupSources.TmdbTv => "TMDB",
            LibraryLookupSources.BoardGameGeek => "BoardGameGeek",
            _ => source,
        };

        /// <summary>
        /// The lookup sources an item's data came from, judged by its external ids, for crediting them.
        /// </summary>
        public static IEnumerable<string> LookupSources(LibraryItem item)
        {
            Dictionary<string, string> ids = item.ExternalIds ?? [];
            if (ids.ContainsKey(LibraryExternalIds.OpenLibraryWork) || ids.ContainsKey(LibraryExternalIds.OpenLibraryEdition))
            {
                yield return LibraryLookupSources.OpenLibrary;
            }
            if (ids.ContainsKey(LibraryExternalIds.GoogleBooks))
            {
                yield return LibraryLookupSources.GoogleBooks;
            }
            if (ids.ContainsKey(LibraryExternalIds.TmdbMovie) || ids.ContainsKey(LibraryExternalIds.TmdbTv))
            {
                yield return LibraryLookupSources.TmdbMovie;
            }
            if (ids.ContainsKey(LibraryExternalIds.BoardGameGeek))
            {
                yield return LibraryLookupSources.BoardGameGeek;
            }
        }

        /// <summary>
        /// <see cref="LibraryItem.Attributes"/> as short phrases, the known keys first in a fixed order and anything else as
        /// "key: value".
        /// </summary>
        public static List<string> Attributes(Dictionary<string, string> attributes)
        {
            if (attributes == null || attributes.Count == 0)
            {
                return [];
            }
            string[] known =
            [
                LibraryAttributes.Pages,
                LibraryAttributes.RuntimeMinutes,
                LibraryAttributes.Seasons,
                LibraryAttributes.Players,
                LibraryAttributes.PlayTimeMinutes,
                LibraryAttributes.MinAge,
                LibraryAttributes.Genres,
            ];
            return attributes
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .OrderBy(x => Array.IndexOf(known, x.Key) is int i && i >= 0 ? i : known.Length)
                .ThenBy(x => x.Key)
                .Select(x => x.Key switch
                {
                    LibraryAttributes.Pages => $"{x.Value} pages",
                    LibraryAttributes.RuntimeMinutes => $"{x.Value} min",
                    LibraryAttributes.Seasons => x.Value == "1" ? "1 season" : $"{x.Value} seasons",
                    LibraryAttributes.Players => x.Value == "1" ? "1 player" : $"{x.Value} players",
                    LibraryAttributes.PlayTimeMinutes => $"{x.Value} min to play",
                    LibraryAttributes.MinAge => $"Age {x.Value}+",
                    LibraryAttributes.Genres => x.Value,
                    _ => $"{x.Key}: {x.Value}",
                })
                .ToList();
        }

        public static Dictionary<long, string> Paths(IEnumerable<LibraryLocation> locations) => locations.ToDictionary(x => x.Id, x => x.Path);

        public static string LocationPath(IReadOnlyDictionary<long, string> paths, long? locationId)
            => !locationId.HasValue ? "No location" : paths.TryGetValue(locationId.Value, out string path) ? path : "(deleted location)";

        /// <summary>
        /// The open loan of a copy, if any. Works for both the search response (only the open loan is present) and the item
        /// response (every loan).
        /// </summary>
        public static LibraryLoan OpenLoan(LibraryCopy copy) => copy.Loans?.FirstOrDefault(x => x.ReturnedDate == null);

        /// <summary>
        /// One line about an item's copies for a search result: how many, where, and who has them.
        /// </summary>
        public static string CopiesSummary(LibraryItem item, IReadOnlyDictionary<long, string> paths)
        {
            List<LibraryCopy> copies = item.Copies?.Where(x => x.Status != LibraryCopyStatus.Disposed).ToList() ?? [];
            if (copies.Count == 0)
            {
                return item.Copies?.Count > 0 ? "No copies left (all disposed)" : "No copies";
            }

            List<string> parts = [copies.Count == 1 ? "1 copy" : $"{copies.Count} copies"];
            parts.Add(string.Join("; ", copies.Select(x => LocationPath(paths, x.LocationId)).Distinct()));
            int missing = copies.Count(x => x.Status == LibraryCopyStatus.Missing);
            if (missing > 0)
            {
                parts.Add($"{missing} missing");
            }
            return string.Join(" · ", parts);
        }

        public static string Byline(LibraryItem item)
        {
            List<string> parts = [];
            if (!string.IsNullOrWhiteSpace(item.Creators))
            {
                parts.Add(item.Creators.Replace(";", ",").Trim());
            }
            if (!string.IsNullOrWhiteSpace(item.Series))
            {
                parts.Add(item.SeriesNumber.HasValue ? $"{item.Series} #{item.SeriesNumber:0.##}" : item.Series);
            }
            if (item.Year.HasValue)
            {
                parts.Add(item.Year.ToString());
            }
            return string.Join(" · ", parts);
        }

        public static string Borrower(LibraryLoan loan)
            => loan.BorrowerAccountName == null || loan.BorrowerAccountName == loan.BorrowerName ? loan.BorrowerName : $"{loan.BorrowerName} ({loan.BorrowerAccountName})";

        public static bool IsOverdue(LibraryLoan loan)
            => loan.ReturnedDate == null && loan.DueDate.HasValue && loan.DueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);

        /// <summary>
        /// Date-only event times are midnight UTC of the date, so they show as a date with no conversion; see
        /// <see cref="LibraryCopyEvent.OccurredDateIsDateOnly"/>.
        /// </summary>
        public static string When(LibraryCopyEvent copyEvent)
            => copyEvent.OccurredDateIsDateOnly ? copyEvent.OccurredDate.ToString("yyyy-MM-dd") : $"{copyEvent.OccurredDate.UtcDateTime:yyyy-MM-dd HH:mm} UTC";

        public static string EventName(LibraryCopyEventType eventType) => eventType switch
        {
            LibraryCopyEventType.MarkedMissing => "Marked missing",
            _ => eventType.ToString(),
        };

        public static string DescribeEvent(LibraryCopyEvent copyEvent, IReadOnlyDictionary<long, string> paths, IReadOnlyDictionary<long, LibraryLoan> loans)
        {
            LibraryLoan loan = copyEvent.LoanId.HasValue ? loans.GetValueOrDefault(copyEvent.LoanId.Value) : null;
            string detail = copyEvent.EventType switch
            {
                LibraryCopyEventType.Acquired => copyEvent.ToLocationId.HasValue ? $"at {LocationPath(paths, copyEvent.ToLocationId)}" : null,
                LibraryCopyEventType.Moved => $"{LocationPath(paths, copyEvent.FromLocationId)} → {LocationPath(paths, copyEvent.ToLocationId)}",
                LibraryCopyEventType.Loaned => loan == null ? null : $"to {Borrower(loan)}",
                LibraryCopyEventType.Returned => loan == null ? null : $"from {Borrower(loan)}",
                _ => null,
            };
            return string.Join(" — ", new[] { detail, copyEvent.Notes }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
