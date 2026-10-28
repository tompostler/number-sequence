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
    /// The lookup is all GET: an ISBN in <c>barcode</c>, <c>isbn</c> or <c>find</c> fills the form in, a title in
    /// <c>find</c> lists candidates, and picking one comes back with <c>source</c> and <c>sourceId</c>. Each of those carries
    /// the barcode, ISBN, location and "add another" along, so a UPC scanned before a title search or an ISBN still lands
    /// on the copy.
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

        /// <summary>
        /// A book whose barcode is a retail UPC, still without an ISBN or details: what's typed in the lookup box next is
        /// most likely the ISBN from the copyright page, so the box gets the numeric keypad and takes nine digits as an
        /// ISBN-10 without its X (<see cref="LibraryBarcode.TryParseIsbn"/>).
        /// </summary>
        public bool IsbnExpected
            => this.Item?.MediaType == LibraryMediaType.Book
                && this.Copy?.Isbn == null
                && this.LookupSource == null
                && LibraryBarcode.TryParse(this.Copy?.Barcode, out LibraryBarcode barcode, out _)
                && barcode.Kind == LibraryBarcodeKind.Gtin;

        public async Task<IActionResult> OnGetAsync(
            long libraryId,
            string barcode,
            string isbn,
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

            // Carried codes first, then whatever is in the lookup box on top of them; anything else in the box is a title.
            this.TakeCode(barcode);
            this.TakeCode(isbn);
            string titleSearch = null;
            if (!string.IsNullOrWhiteSpace(this.Find))
            {
                if (this.TakeCode(this.Find, isbnExpected: this.IsbnExpected))
                {
                    // It rides along in the hidden fields now, leaving the box empty for what comes next: a title search
                    // for a movie or game, or the ISBN of a book whose barcode is a UPC.
                    this.Find = null;
                }
                else
                {
                    titleSearch = this.Find;
                }
            }

            LibraryBarcode? parsed = this.Copy.Barcode != null && LibraryBarcode.TryParse(this.Copy.Barcode, out LibraryBarcode b, out _) ? b : null;
            if (parsed?.Kind == LibraryBarcodeKind.Gtin && this.Copy.Isbn == null)
            {
                // A scanned code that isn't an ISBN is far more often a movie or game than a book.
                this.Item.MediaType = type ?? LibraryMediaType.Movie;
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
                else if (this.Copy.Isbn != null)
                {
                    List<LibraryLookupResult> found = await this.NsClient.Library.LookupIsbnAsync(this.Copy.Isbn, cancellationToken);
                    if (found.Count > 0)
                    {
                        this.Apply(found[0]);
                    }
                    else
                    {
                        this.LookupNote = $"Neither Open Library nor Google Books knows ISBN {this.Copy.Isbn}. Search by title above, or fill it in below.";
                    }
                }
                else if (parsed?.Kind == LibraryBarcodeKind.Gtin)
                {
                    // Many US books, mass-market paperbacks especially, carry a retail UPC on the back cover, with the ISBN
                    // barcode inside the front cover. The UPC doesn't contain the ISBN, so it can't be looked up.
                    this.LookupNote = this.Item.MediaType == LibraryMediaType.Book
                        ? $"{parsed.Value.Value} is a retail barcode, not an ISBN. It's kept as the copy's barcode. Type the ISBN from the copyright page above to look the book up (both will find it), or search by title."
                        : "TMDB and BoardGameGeek find things by title, not barcode. Search by title above to fill in the details; the barcode stays with the copy.";
                }
            }
            catch (NsTcpWtfClientException ex)
            {
                this.LookupNote = $"Lookup didn't work: {ErrorText(ex)} Fill it in below.";
            }

            if (this.LookupSource != null || parsed.HasValue || this.Copy.Isbn != null)
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
        /// The link for picking a search candidate, carrying the barcode, ISBN, location and "add another" along.
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
            if (!string.IsNullOrWhiteSpace(this.Copy?.Isbn))
            {
                query.Add($"isbn={Uri.EscapeDataString(this.Copy.Isbn)}");
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
        /// Puts a scanned or typed code on the copy, returning false when it isn't a product code (so it's a title). An ISBN
        /// is the copy's ISBN, and its barcode too unless the barcode is a retail UPC: a book printed with only a UPC keeps
        /// it as the barcode, so a rescan finds it, and is looked up by the ISBN typed from the copyright page. A UPC or
        /// EAN replaces the barcode and leaves the ISBN alone. With <paramref name="isbnExpected"/>, nine digits are an
        /// ISBN-10 without its X.
        /// </summary>
        private bool TakeCode(string code, bool isbnExpected = false)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }
            if (!(isbnExpected && LibraryBarcode.TryParseIsbn(code, out LibraryBarcode parsed, out _))
                && (!LibraryBarcode.TryParse(code, out parsed, out _) || parsed.Kind == LibraryBarcodeKind.Other))
            {
                return false;
            }
            if (parsed.Kind == LibraryBarcodeKind.Isbn)
            {
                this.Copy.Isbn = parsed.Value;
                bool barcodeIsUpc = LibraryBarcode.TryParse(this.Copy.Barcode, out LibraryBarcode current, out _) && current.Kind == LibraryBarcodeKind.Gtin;
                if (!barcodeIsUpc)
                {
                    this.Copy.Barcode = parsed.Value;
                }
            }
            else
            {
                this.Copy.Barcode = parsed.Value;
            }
            return true;
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
            return null;
        }
    }
}
