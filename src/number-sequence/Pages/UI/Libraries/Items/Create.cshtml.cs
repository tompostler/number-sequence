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
    /// Title first: the lookup is all GET, a title in <c>find</c> lists candidates, and picking one comes back with
    /// <c>source</c> and <c>sourceId</c>. The copy's barcode is last and optional. One can arrive in <c>barcode</c>, from a
    /// scan's reconcile view or a search that found nothing; it goes on the copy, is never looked up, and is carried
    /// through the title search with the location and "add another".
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
        /// Set when adding a new code from a scan's reconcile view: once the copy is saved, that scan entry is marked added
        /// as it, and the page goes back to the reconcile view. Carried through the lookup like the barcode.
        /// </summary>
        [BindProperty(SupportsGet = true, Name = "scan")]
        public long? ScanId { get; set; }

        [BindProperty(SupportsGet = true, Name = "scanEntry")]
        public long? ScanEntryId { get; set; }

        public bool FromScan => this.ScanId.HasValue && this.ScanEntryId.HasValue;

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
        /// The title searched for.
        /// </summary>
        public string Find { get; private set; }

        /// <summary>
        /// Title search results to choose from, or null when no title search was made.
        /// </summary>
        public List<LibraryLookupResult> Candidates { get; private set; }

        /// <summary>
        /// Why nothing was filled in, such as a provider that's down. Advice, not an error: the form still works by hand.
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
            this.Copy = new LibraryCopy();
            this.LocationId = location;
            this.AddAnother = another;
            this.Find = find?.Trim();

            if (LibraryBarcode.TryParse(barcode, out LibraryBarcode parsed, out _))
            {
                this.Copy.Barcode = parsed.Value;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(sourceId))
                {
                    this.Apply(await this.NsClient.Library.GetLookupAsync(source, sourceId, cancellationToken));
                }
                else if (!string.IsNullOrWhiteSpace(this.Find))
                {
                    this.Candidates = await this.NsClient.Library.SearchLookupAsync(this.Item.MediaType, this.Find, cancellationToken);
                }
            }
            catch (NsTcpWtfClientException ex)
            {
                this.LookupNote = $"Lookup didn't work: {ErrorText(ex)} Fill it in below.";
            }

            if (this.LookupSource != null || this.Copy.Barcode != null)
            {
                try
                {
                    this.Duplicates = await this.FindDuplicatesAsync(libraryId, cancellationToken);
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

            if (this.AddCopy && !string.IsNullOrWhiteSpace(this.Copy?.Barcode))
            {
                if (LibraryBarcode.TryParse(this.Copy.Barcode, out LibraryBarcode parsed, out string error))
                {
                    this.Copy.Barcode = parsed.Value;
                }
                else
                {
                    this.ModelState.AddModelError($"{nameof(this.Copy)}.{nameof(this.Copy.Barcode)}", error);
                }
            }
            if (this.AddCopy && !string.IsNullOrWhiteSpace(this.Copy?.Isbn))
            {
                if (LibraryBarcode.TryParseIsbn(this.Copy.Isbn, out LibraryBarcode parsed, out string error))
                {
                    this.Copy.Isbn = parsed.Value;
                }
                else
                {
                    this.ModelState.AddModelError($"{nameof(this.Copy)}.{nameof(this.Copy.Isbn)}", error);
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
                        this.Duplicates = await this.FindDuplicatesAsync(libraryId, cancellationToken);
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

            LibraryCopy createdCopy = null;
            if (this.AddCopy)
            {
                this.Copy.ItemId = target.Id;
                this.Copy.LocationId = this.LocationId;
                try
                {
                    createdCopy = await this.NsClient.Library.CreateCopyAsync(libraryId, this.Copy, cancellationToken);
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
            if (this.FromScan)
            {
                if (createdCopy != null)
                {
                    try
                    {
                        _ = await this.NsClient.Library.ResolveScanEntryAsync(libraryId, this.ScanId.Value, this.ScanEntryId.Value, createdCopy.Id, cancellationToken);
                    }
                    catch (NsTcpWtfClientException ex)
                    {
                        this.StatusMessage += $" The scan still lists it as new: {ErrorText(ex)}";
                    }
                }
                return this.Redirect($"/ui/library/{libraryId}/scans/{this.ScanId}/reconcile");
            }
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
            if (this.FromScan)
            {
                query.Add($"scan={this.ScanId}&scanEntry={this.ScanEntryId}");
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
            this.Item.Series = LibrarySeries.MatchExisting(result.Series, this.SeriesNames);
            this.Item.SeriesNumber = result.SeriesNumber;
            this.Item.Year = result.Year;
            this.Item.Summary = result.Summary;
            this.Item.ExternalIds = result.ExternalIds;
            this.Item.Attributes = result.Attributes;

            this.Copy.Barcode ??= result.Barcode;
            this.Copy.Isbn ??= result.Barcode;
            this.Copy.Format = result.Format;
            this.Copy.Publisher = result.Publisher;
            this.Copy.PublishedYear = result.PublishedYear;

            this.CoverUrls = result.CoverUrls ?? [];
            this.LookupSource = result.Source;
            this.LookupSourceUrl = result.SourceUrl;
        }

        /// <summary>
        /// Items this might already be: one with a copy carrying the same barcode or ISBN (either one matching either column,
        /// as everywhere else), one with the same lookup record (any shared external id, which is how a paperback finds the
        /// hardcover's item), or one with the same title.
        /// </summary>
        private async Task<List<LibraryItem>> FindDuplicatesAsync(long libraryId, CancellationToken cancellationToken)
        {
            List<LibraryItem> found = [];
            foreach (string code in new[] { this.Copy?.Barcode, this.Copy?.Isbn }.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
            {
                List<LibraryItem> byCode = await this.NsClient.Library.GetItemsAsync(libraryId, code, take: 10, cancellationToken: cancellationToken);
                found.AddRange(byCode.Where(x => x.Copies.Any(c => c.Barcode == code || c.Isbn == code)));
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
            await this.LoadSeriesNamesAsync(libraryId, cancellationToken);
            return null;
        }
    }
}
