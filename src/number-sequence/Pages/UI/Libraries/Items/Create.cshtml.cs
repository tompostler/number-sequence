using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries.Items
{
    /// <summary>
    /// Adds an item and, usually, its first copy in one form. Before creating, it looks for an item this might already be
    /// (same barcode, same lookup record, or same title) and offers to add the copy to that instead, which is the main guard
    /// against duplicate items.
    /// <para>
    /// The lookup is all GET: an ISBN in <c>barcode</c> or <c>find</c> fills the form in, a title in <c>find</c> lists
    /// candidates, and picking one comes back with <c>source</c> and <c>sourceId</c>. Each of those carries the barcode,
    /// location and "add another" along, so a UPC scanned before a title search still lands on the copy.
    /// </para>
    /// </summary>
    [RequiresToken(AccountRoles.Library)]
    public sealed class CreateModel : LibraryPageModel
    {
        public CreateModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        [BindProperty]
        public LibraryItem Item { get; set; }

        [BindProperty]
        public LibraryCopy Copy { get; set; }

        [BindProperty]
        public bool AddCopy { get; set; } = true;

        [BindProperty]
        public long? LocationId { get; set; }

        /// <summary>
        /// Return to this form after saving, keeping the type and location, for entering a shelf at a time.
        /// </summary>
        [BindProperty]
        public bool AddAnother { get; set; }

        /// <summary>
        /// Set once the caller has seen <see cref="Duplicates"/> and wants a new item anyway.
        /// </summary>
        [BindProperty]
        public bool ConfirmNew { get; set; }

        /// <summary>
        /// Set when the caller picked one of <see cref="Duplicates"/>: the copy is added to it and no item is created.
        /// </summary>
        [BindProperty]
        public long? AsCopyOfItemId { get; set; }

        /// <summary>
        /// The filled-in lookup result's cover urls, fetched into the item's cover once it's saved.
        /// </summary>
        [BindProperty]
        public List<string> CoverUrls { get; set; } = [];

        /// <summary>
        /// Where the form was filled in from (a <see cref="LibraryLookupSources"/> value), carried through the post so the
        /// form shown again can still say so.
        /// </summary>
        [BindProperty]
        public string LookupSource { get; set; }

        [BindProperty]
        public string LookupSourceUrl { get; set; }

        /// <summary>
        /// What was typed in the lookup box.
        /// </summary>
        public string Find { get; private set; }

        /// <summary>
        /// Title search results to choose from, or null when no title search was made.
        /// </summary>
        public List<LibraryLookupResult> Candidates { get; private set; }

        /// <summary>
        /// Why nothing was filled in, such as an ISBN no provider knows or a provider that's down. Advice, not an error: the
        /// form still works by hand.
        /// </summary>
        public string LookupNote { get; private set; }

        public List<LibraryLocation> Locations { get; private set; }

        public Dictionary<long, string> Paths { get; private set; }

        public List<LibraryItem> Duplicates { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync(
            long libraryId,
            string barcode,
            LibraryMediaType? type,
            long? location,
            bool another,
            string find,
            string source,
            string sourceId,
            CancellationToken cancellationToken)
        {
            if (await this.LoadAsync(libraryId, cancellationToken) is IActionResult result)
            {
                return result;
            }

            this.Item = new LibraryItem { Title = null, MediaType = type ?? LibraryMediaType.Book };
            this.Copy = new LibraryCopy { Barcode = barcode };
            this.LocationId = location;
            this.AddAnother = another;
            this.Find = find?.Trim();

            // A barcode typed into the lookup box replaces any carried along; anything else in it is a title.
            string titleSearch = null;
            if (!string.IsNullOrWhiteSpace(this.Find))
            {
                if (LibraryBarcode.TryParse(this.Find, out LibraryBarcode typed, out _) && typed.Kind != LibraryBarcodeKind.Other)
                {
                    this.Copy.Barcode = typed.Value;
                }
                else
                {
                    titleSearch = this.Find;
                }
            }

            LibraryBarcode? parsed = this.Copy.Barcode != null && LibraryBarcode.TryParse(this.Copy.Barcode, out LibraryBarcode b, out _) ? b : null;
            if (parsed.HasValue)
            {
                this.Copy.Barcode = parsed.Value.Value;
                if (parsed.Value.Kind != LibraryBarcodeKind.Isbn)
                {
                    // A scanned code that isn't an ISBN is far more often a movie or game than a book.
                    this.Item.MediaType = type ?? LibraryMediaType.Movie;
                }
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(sourceId))
                {
                    this.Apply(await this.NsClient.Library.GetLookupAsync(source, sourceId, cancellationToken));
                }
                else if (titleSearch != null)
                {
                    this.Candidates = await this.NsClient.Library.SearchLookupAsync(this.Item.MediaType, titleSearch, cancellationToken);
                }
                else if (parsed?.Kind == LibraryBarcodeKind.Isbn)
                {
                    List<LibraryLookupResult> found = await this.NsClient.Library.LookupIsbnAsync(parsed.Value.Value, cancellationToken);
                    if (found.Count > 0)
                    {
                        this.Apply(found[0]);
                    }
                    else
                    {
                        this.LookupNote = $"Neither Open Library nor Google Books knows ISBN {parsed.Value.Value}. Search by title above, or fill it in below.";
                    }
                }
                else if (parsed?.Kind == LibraryBarcodeKind.Gtin)
                {
                    this.LookupNote = "Movies and games can't be looked up by barcode. Search by title above to fill in the details; the barcode stays with the copy.";
                }
            }
            catch (NsTcpWtfClientException ex)
            {
                this.LookupNote = $"Lookup didn't work: {ErrorText(ex)} Fill it in below.";
            }

            if (this.LookupSource != null || parsed.HasValue)
            {
                try
                {
                    this.Duplicates = await this.FindDuplicatesAsync(libraryId, parsed?.Value, cancellationToken);
                }
                catch (NsTcpWtfClientException ex)
                {
                    this.FormError = ErrorText(ex);
                }
            }
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long libraryId, CancellationToken cancellationToken)
        {
            if (await this.LoadAsync(libraryId, cancellationToken) is IActionResult result)
            {
                return result;
            }

            if (this.AsCopyOfItemId.HasValue || !this.AddCopy)
            {
                // Only one half of the form is in use; don't let the other half's validation block it.
                string ignored = this.AsCopyOfItemId.HasValue ? $"{nameof(this.Item)}." : $"{nameof(this.Copy)}.";
                foreach (string key in this.ModelState.Keys.Where(x => x.StartsWith(ignored, StringComparison.Ordinal)).ToList())
                {
                    _ = this.ModelState.Remove(key);
                }
            }

            string barcode = null;
            if (this.AddCopy && !string.IsNullOrWhiteSpace(this.Copy?.Barcode))
            {
                if (LibraryBarcode.TryParse(this.Copy.Barcode, out LibraryBarcode parsed, out string error))
                {
                    barcode = parsed.Value;
                }
                else
                {
                    this.ModelState.AddModelError($"{nameof(this.Copy)}.{nameof(this.Copy.Barcode)}", error);
                }
            }
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            LibraryItem target;
            try
            {
                if (this.AsCopyOfItemId.HasValue)
                {
                    target = await this.NsClient.Library.GetItemAsync(libraryId, this.AsCopyOfItemId.Value, cancellationToken);
                    target = await this.AddMissingExternalIdsAsync(libraryId, target, cancellationToken);
                }
                else
                {
                    if (!this.ConfirmNew)
                    {
                        this.Duplicates = await this.FindDuplicatesAsync(libraryId, barcode, cancellationToken);
                        if (this.Duplicates.Count > 0)
                        {
                            return this.Page();
                        }
                    }

                    target = await this.NsClient.Library.CreateItemAsync(libraryId, this.Item, cancellationToken);
                }
            }
            catch (NsTcpWtfClientException ex)
            {
                this.FormError = ErrorText(ex);
                return this.Page();
            }

            if (this.AddCopy)
            {
                this.Copy.ItemId = target.Id;
                this.Copy.LocationId = this.LocationId;
                try
                {
                    _ = await this.NsClient.Library.CreateCopyAsync(libraryId, this.Copy, cancellationToken);
                }
                catch (NsTcpWtfClientException ex)
                {
                    // The item exists by now, so send them to it rather than offering to create it a second time.
                    this.ErrorMessage = $"{target.Title} was saved, but its copy wasn't: {ErrorText(ex)}";
                    return this.Redirect($"/ui/library/{libraryId}/items/{target.Id}");
                }
            }

            string coverNote = string.Empty;
            if (!target.HasCover && this.CoverUrls.Count > 0)
            {
                try
                {
                    _ = await this.NsClient.Library.SetItemCoverAsync(libraryId, target.Id, new LibraryCover { SourceUrls = this.CoverUrls }, cancellationToken);
                }
                catch (NsTcpWtfClientException ex)
                {
                    coverNote = $" Its cover couldn't be saved: {ErrorText(ex)}";
                }
            }

            this.StatusMessage = (this.AsCopyOfItemId.HasValue ? $"Added another copy of {target.Title}." : $"Added {target.Title}.") + coverNote;
            if (this.AddAnother)
            {
                string location = this.LocationId.HasValue ? $"&location={this.LocationId}" : string.Empty;
                return this.Redirect($"/ui/library/{libraryId}/items/create?another=true&type={this.Item?.MediaType ?? target.MediaType}{location}");
            }
            return this.Redirect($"/ui/library/{libraryId}/items/{target.Id}");
        }

        /// <summary>
        /// The link for picking a search candidate, carrying the barcode, location and "add another" along.
        /// </summary>
        public string PickUrl(LibraryLookupResult candidate)
        {
            List<string> query =
            [
                $"source={Uri.EscapeDataString(candidate.Source)}",
                $"sourceId={Uri.EscapeDataString(candidate.SourceId)}",
                $"type={candidate.MediaType}",
            ];
            if (!string.IsNullOrWhiteSpace(this.Copy?.Barcode))
            {
                query.Add($"barcode={Uri.EscapeDataString(this.Copy.Barcode)}");
            }
            if (this.LocationId.HasValue)
            {
                query.Add($"location={this.LocationId}");
            }
            if (this.AddAnother)
            {
                query.Add("another=true");
            }
            return $"/ui/library/{this.Library.Id}/items/create?{string.Join("&", query)}";
        }

        /// <summary>
        /// Fills the form in from a lookup result. A barcode already on the copy is kept: it's what was scanned.
        /// </summary>
        private void Apply(LibraryLookupResult result)
        {
            this.Item.MediaType = result.MediaType;
            this.Item.Title = result.Title;
            this.Item.Subtitle = result.Subtitle;
            this.Item.Creators = result.Creators;
            this.Item.Series = result.Series;
            this.Item.SeriesNumber = result.SeriesNumber;
            this.Item.Year = result.Year;
            this.Item.Summary = result.Summary;
            this.Item.ExternalIds = result.ExternalIds;
            this.Item.Attributes = result.Attributes;

            this.Copy.Barcode ??= result.Barcode;
            this.Copy.Format = result.Format;
            this.Copy.Publisher = result.Publisher;
            this.Copy.PublishedYear = result.PublishedYear;

            this.CoverUrls = result.CoverUrls ?? [];
            this.LookupSource = result.Source;
            this.LookupSourceUrl = result.SourceUrl;
        }

        /// <summary>
        /// Items this might already be: one with a copy carrying the same barcode, one with the same lookup record (any shared
        /// external id, which is how a paperback finds the hardcover's item), or one with the same title.
        /// </summary>
        private async Task<List<LibraryItem>> FindDuplicatesAsync(long libraryId, string barcode, CancellationToken cancellationToken)
        {
            List<LibraryItem> found = [];
            if (barcode != null)
            {
                List<LibraryItem> byBarcode = await this.NsClient.Library.GetItemsAsync(libraryId, barcode, take: 10, cancellationToken: cancellationToken);
                found.AddRange(byBarcode.Where(x => x.Copies.Any(c => c.Barcode == barcode)));
            }

            foreach ((string key, string value) in this.Item.ExternalIds ?? [])
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    found.AddRange(await this.NsClient.Library.GetItemsByExternalIdAsync(libraryId, key, value, cancellationToken));
                }
            }

            string titleTerm = this.Item.Title?.Trim();
            if (!string.IsNullOrEmpty(titleTerm))
            {
                List<LibraryItem> byTitle = await this.NsClient.Library.GetItemsAsync(libraryId, titleTerm, take: 50, cancellationToken: cancellationToken);
                found.AddRange(byTitle.Where(x => string.Equals(x.Title.Trim(), titleTerm, StringComparison.OrdinalIgnoreCase)));
            }

            return found.DistinctBy(x => x.Id).ToList();
        }

        /// <summary>
        /// When a looked-up copy joins an item that was typed in by hand (found by title), gives the item the lookup's
        /// external ids it doesn't have yet, so the next edition finds it by id. Existing ids are never changed.
        /// </summary>
        private async Task<LibraryItem> AddMissingExternalIdsAsync(long libraryId, LibraryItem item, CancellationToken cancellationToken)
        {
            Dictionary<string, string> missing = (this.Item?.ExternalIds ?? [])
                .Where(x => !string.IsNullOrWhiteSpace(x.Value) && item.ExternalIds?.ContainsKey(x.Key) != true)
                .ToDictionary(x => x.Key, x => x.Value);
            if (missing.Count == 0)
            {
                return item;
            }

            item.ExternalIds ??= [];
            foreach ((string key, string value) in missing)
            {
                item.ExternalIds[key] = value;
            }
            item.Copies = null;
            return await this.NsClient.Library.UpdateItemAsync(libraryId, item, cancellationToken);
        }

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

            this.Locations = await this.NsClient.Library.GetLocationsAsync(libraryId, cancellationToken);
            this.Paths = LibraryDisplay.Paths(this.Locations);
            return null;
        }
    }
}
