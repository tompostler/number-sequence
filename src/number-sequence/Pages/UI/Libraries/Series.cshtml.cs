using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using System.Globalization;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries
{
    /// <summary>
    /// Sets one series on several items at once: search, tick, type the name once, and a number per book. The bulk fix
    /// for books added without their series, and for renaming a misspelled series everywhere it's used.
    /// </summary>
    [RequiresToken(AccountRoles.Library)]
    public sealed class SeriesModel : LibraryPageModel
    {
        private const int MaxShown = 200;

        public SeriesModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        [BindProperty(SupportsGet = true)]
        public string Q { get; set; }

        /// <summary>
        /// The series to set. Prefilled with the one most of the matches already have.
        /// </summary>
        [BindProperty]
        public string Series { get; set; }

        /// <summary>
        /// The ticked item ids.
        /// </summary>
        [BindProperty]
        public List<long> Take { get; set; } = [];

        /// <summary>
        /// Each row's number box by item id, posted as <c>Number[id]</c>.
        /// </summary>
        [BindProperty]
        public Dictionary<long, string> Number { get; set; } = [];

        public List<LibraryItem> Items { get; private set; } = [];

        public bool Truncated { get; private set; }

        /// <summary>
        /// Whether the boxes hold what was posted, shown again after a mistake, rather than what's saved.
        /// </summary>
        public bool Reshown { get; private set; }

        public async Task<IActionResult> OnGetAsync(long libraryId, CancellationToken cancellationToken)
        {
            if (await this.LoadAsync(libraryId, cancellationToken) is IActionResult result)
            {
                return result;
            }

            // Whichever series most of the matches already have, as a starting point.
            this.Series = this.Items
                .Where(x => !string.IsNullOrWhiteSpace(x.Series))
                .GroupBy(x => x.Series)
                .OrderByDescending(x => x.Count())
                .Select(x => x.Key)
                .FirstOrDefault();
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long libraryId, CancellationToken cancellationToken)
        {
            string here = $"/ui/library/{libraryId}/series";
            if (this.Take.Count == 0)
            {
                this.ErrorMessage = "Nothing was ticked, so nothing changed.";
                return this.Redirect(here + Query(this.Q));
            }

            LibrarySeriesAssign assign = new() { Series = this.Series };
            List<string> unreadable = [];
            foreach (long itemId in this.Take.Distinct())
            {
                string text = this.Number.GetValueOrDefault(itemId)?.Trim();
                decimal? number = null;
                if (!string.IsNullOrEmpty(text))
                {
                    if (decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal parsed))
                    {
                        number = parsed;
                    }
                    else
                    {
                        unreadable.Add(text);
                    }
                }
                assign.Items.Add(new() { ItemId = itemId, Number = number });
            }

            string error = unreadable.Count > 0 ? $"[{string.Join("], [", unreadable)}] isn't a number." : null;
            if (error == null)
            {
                try
                {
                    List<LibraryItem> saved = await this.NsClient.Library.SetSeriesAsync(libraryId, assign, cancellationToken);
                    string series = string.IsNullOrWhiteSpace(this.Series) ? null : this.Series.Trim();
                    this.StatusMessage = series == null
                        ? $"Took {Count(saved.Count)} out of their series."
                        : $"Put {Count(saved.Count)} in {series}.";
                    // Show the series as it now is.
                    return this.Redirect(here + Query(series ?? this.Q));
                }
                catch (NsTcpWtfClientException ex)
                {
                    error = ErrorText(ex);
                }
            }

            if (await this.LoadAsync(libraryId, cancellationToken) is IActionResult result)
            {
                return result;
            }
            this.FormError = error;
            this.Reshown = true;
            return this.Page();
        }

        /// <summary>
        /// The number box's value: what was posted when shown again, else the saved number.
        /// </summary>
        public string NumberFor(LibraryItem item)
            => this.Reshown ? this.Number.GetValueOrDefault(item.Id) : item.SeriesNumber?.ToString("0.##", CultureInfo.InvariantCulture);

        public bool IsTicked(LibraryItem item) => !this.Reshown || this.Take.Contains(item.Id);

        private async Task<IActionResult> LoadAsync(long libraryId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }
            if (!this.CanEdit)
            {
                return this.Redirect($"/ui/library/{libraryId}");
            }

            await this.LoadSeriesNamesAsync(libraryId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(this.Q))
            {
                List<LibraryItem> found = await this.NsClient.Library.GetItemsAsync(libraryId, this.Q.Trim(), take: MaxShown, cancellationToken: cancellationToken);
                this.Truncated = found.Count == MaxShown;
                // Likely series order rather than shelf order: numbered books by number, then the rest by year, which is
                // usually publication order and so what "number them in this order" should follow.
                this.Items = found
                    .OrderBy(x => !x.SeriesNumber.HasValue)
                    .ThenBy(x => x.SeriesNumber)
                    .ThenBy(x => !x.Year.HasValue)
                    .ThenBy(x => x.Year)
                    .ThenBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();
            }
            return null;
        }

        private static string Query(string q) => string.IsNullOrWhiteSpace(q) ? string.Empty : $"?q={Uri.EscapeDataString(q.Trim())}";

        private static string Count(int n) => n == 1 ? "1 item" : $"{n} items";
    }
}
