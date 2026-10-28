using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries.Copies
{
    /// <summary>
    /// A copy's descriptive fields, plus moving it to another item, which is how a duplicate item gets merged away: move
    /// each of its copies onto the item that should have them, then delete the empty one.
    /// </summary>
    [RequiresToken(AccountRoles.Library)]
    public sealed class EditModel : LibraryPageModel
    {
        public EditModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        [BindProperty]
        public LibraryCopy Copy { get; set; }

        public LibraryItem Item { get; private set; }

        /// <summary>
        /// A search for the item to move this copy to.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string MergeQ { get; set; }

        public List<LibraryItem> MergeCandidates { get; private set; } = [];

        public Dictionary<long, string> Paths { get; private set; }

        public async Task<IActionResult> OnGetAsync(long libraryId, long copyId, CancellationToken cancellationToken)
        {
            if (await this.LoadAsync(libraryId, copyId, cancellationToken) is IActionResult result)
            {
                return result;
            }

            if (!string.IsNullOrWhiteSpace(this.MergeQ))
            {
                List<LibraryItem> found = await this.NsClient.Library.GetItemsAsync(libraryId, this.MergeQ, take: 20, cancellationToken: cancellationToken);
                this.MergeCandidates = found.Where(x => x.Id != this.Item.Id).ToList();
                this.Paths = LibraryDisplay.Paths(await this.NsClient.Library.GetLocationsAsync(libraryId, cancellationToken));
            }
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long libraryId, long copyId, CancellationToken cancellationToken)
        {
            LibraryCopy posted = this.Copy;
            if (await this.LoadAsync(libraryId, copyId, cancellationToken) is IActionResult result)
            {
                return result;
            }
            if (!this.ModelState.IsValid)
            {
                this.Copy = posted;
                return this.Page();
            }

            // Apply the form onto the stored copy, so the item it belongs to comes from the server and not the form.
            this.Copy.Format = posted.Format?.Trim();
            this.Copy.Barcode = posted.Barcode?.Trim();
            this.Copy.Isbn = posted.Isbn?.Trim();
            this.Copy.Publisher = posted.Publisher?.Trim();
            this.Copy.PublishedYear = posted.PublishedYear;
            this.Copy.Condition = posted.Condition?.Trim();
            this.Copy.AcquiredDate = posted.AcquiredDate;
            this.Copy.AcquiredFrom = posted.AcquiredFrom?.Trim();
            this.Copy.PricePaid = posted.PricePaid;
            this.Copy.Notes = posted.Notes?.Trim();
            try
            {
                _ = await this.NsClient.Library.UpdateCopyAsync(libraryId, this.Copy, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.FormError = ErrorText(ex);
                return this.Page();
            }

            this.StatusMessage = "Saved.";
            return this.Redirect($"/ui/library/{libraryId}/items/{this.Copy.ItemId}");
        }

        public async Task<IActionResult> OnPostMoveToItemAsync(long libraryId, long copyId, long targetItemId, CancellationToken cancellationToken)
        {
            if (await this.LoadAsync(libraryId, copyId, cancellationToken) is IActionResult result)
            {
                return result;
            }

            this.Copy.ItemId = targetItemId;
            try
            {
                _ = await this.NsClient.Library.UpdateCopyAsync(libraryId, this.Copy, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ErrorText(ex);
                return this.Redirect($"/ui/library/{libraryId}/copies/{copyId}/edit");
            }

            bool emptied = this.Item.Copies.Count == 1;
            this.StatusMessage = emptied
                ? $"Moved the copy here. {this.Item.Title} has no copies left; delete it from its page if it was a duplicate."
                : $"Moved the copy here from {this.Item.Title}.";
            return this.Redirect($"/ui/library/{libraryId}/items/{targetItemId}");
        }

        public async Task<IActionResult> OnPostDeleteAsync(long libraryId, long copyId, CancellationToken cancellationToken)
        {
            if (await this.LoadAsync(libraryId, copyId, cancellationToken) is IActionResult result)
            {
                return result;
            }

            return await this.RunAndRedirectAsync(
                () => this.NsClient.Library.DeleteCopyAsync(libraryId, copyId, cancellationToken),
                "Copy deleted.",
                $"/ui/library/{libraryId}/items/{this.Item.Id}");
        }

        /// <summary>
        /// Loads the stored copy into <see cref="Copy"/>, and its item.
        /// </summary>
        private async Task<IActionResult> LoadAsync(long libraryId, long copyId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }
            if (!this.CanEdit)
            {
                return this.Redirect($"/ui/library/{libraryId}");
            }

            try
            {
                this.Copy = await this.NsClient.Library.GetCopyAsync(libraryId, copyId, cancellationToken);
                this.Item = await this.NsClient.Library.GetItemAsync(libraryId, this.Copy.ItemId, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }
            return null;
        }
    }
}
