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
    /// (same barcode, or same title) and offers to add the copy to that instead, which is the main guard against duplicate
    /// items.
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

        public List<LibraryLocation> Locations { get; private set; }

        public Dictionary<long, string> Paths { get; private set; }

        public List<LibraryItem> Duplicates { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync(long libraryId, string barcode, LibraryMediaType? type, long? location, bool another, CancellationToken cancellationToken)
        {
            if (await this.LoadAsync(libraryId, cancellationToken) is IActionResult result)
            {
                return result;
            }

            this.Item = new LibraryItem { Title = null, MediaType = type ?? LibraryMediaType.Book };
            this.Copy = new LibraryCopy { Barcode = barcode };
            this.LocationId = location;
            this.AddAnother = another;
            if (barcode != null && LibraryBarcode.TryParse(barcode, out LibraryBarcode parsed, out _) && parsed.Kind != LibraryBarcodeKind.Isbn)
            {
                // A scanned code that isn't an ISBN is far more often a movie or game than a book.
                this.Item.MediaType = type ?? LibraryMediaType.Movie;
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

            long itemId;
            string title;
            try
            {
                if (this.AsCopyOfItemId.HasValue)
                {
                    LibraryItem existing = await this.NsClient.Library.GetItemAsync(libraryId, this.AsCopyOfItemId.Value, cancellationToken);
                    itemId = existing.Id;
                    title = existing.Title;
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

                    LibraryItem created = await this.NsClient.Library.CreateItemAsync(libraryId, this.Item, cancellationToken);
                    itemId = created.Id;
                    title = created.Title;
                }
            }
            catch (NsTcpWtfClientException ex)
            {
                this.FormError = ErrorText(ex);
                return this.Page();
            }

            if (this.AddCopy)
            {
                this.Copy.ItemId = itemId;
                this.Copy.LocationId = this.LocationId;
                try
                {
                    _ = await this.NsClient.Library.CreateCopyAsync(libraryId, this.Copy, cancellationToken);
                }
                catch (NsTcpWtfClientException ex)
                {
                    // The item exists by now, so send them to it rather than offering to create it a second time.
                    this.ErrorMessage = $"{title} was saved, but its copy wasn't: {ErrorText(ex)}";
                    return this.Redirect($"/ui/library/{libraryId}/items/{itemId}");
                }
            }

            this.StatusMessage = this.AsCopyOfItemId.HasValue ? $"Added another copy of {title}." : $"Added {title}.";
            if (this.AddAnother)
            {
                string location = this.LocationId.HasValue ? $"&location={this.LocationId}" : string.Empty;
                return this.Redirect($"/ui/library/{libraryId}/items/create?another=true&type={this.Item?.MediaType ?? LibraryMediaType.Book}{location}");
            }
            return this.Redirect($"/ui/library/{libraryId}/items/{itemId}");
        }

        /// <summary>
        /// Items this might already be: one with a copy carrying the same barcode, or one with the same title.
        /// </summary>
        private async Task<List<LibraryItem>> FindDuplicatesAsync(long libraryId, string barcode, CancellationToken cancellationToken)
        {
            List<LibraryItem> found = [];
            if (barcode != null)
            {
                List<LibraryItem> byBarcode = await this.NsClient.Library.GetItemsAsync(libraryId, barcode, take: 10, cancellationToken: cancellationToken);
                found.AddRange(byBarcode.Where(x => x.Copies.Any(c => c.Barcode == barcode)));
            }

            string titleTerm = this.Item.Title.Trim();
            List<LibraryItem> byTitle = await this.NsClient.Library.GetItemsAsync(libraryId, titleTerm, take: 50, cancellationToken: cancellationToken);
            found.AddRange(byTitle.Where(x => string.Equals(x.Title.Trim(), titleTerm, StringComparison.OrdinalIgnoreCase)));

            return found.DistinctBy(x => x.Id).ToList();
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
