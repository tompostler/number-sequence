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
