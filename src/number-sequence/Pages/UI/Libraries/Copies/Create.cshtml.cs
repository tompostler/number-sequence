using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries.Copies
{
    [RequiresToken(AccountRoles.Library)]
    public sealed class CreateModel : LibraryPageModel
    {
        public CreateModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        [BindProperty]
        public LibraryCopy Copy { get; set; }

        [BindProperty]
        public long? LocationId { get; set; }

        public LibraryItem Item { get; private set; }

        public List<LibraryLocation> Locations { get; private set; }

        public async Task<IActionResult> OnGetAsync(long libraryId, long itemId, CancellationToken cancellationToken)
        {
            if (await this.LoadAsync(libraryId, itemId, cancellationToken) is IActionResult result)
            {
                return result;
            }

            // Most often another copy is a second format, or the same one on another shelf: start from the last copy's
            // location, and leave the rest blank.
            this.Copy = new LibraryCopy();
            this.LocationId = this.Item.Copies.LastOrDefault(x => x.Status != LibraryCopyStatus.Disposed)?.LocationId;
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long libraryId, long itemId, CancellationToken cancellationToken)
        {
            if (await this.LoadAsync(libraryId, itemId, cancellationToken) is IActionResult result)
            {
                return result;
            }
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            this.Copy.ItemId = itemId;
            this.Copy.LocationId = this.LocationId;
            try
            {
                _ = await this.NsClient.Library.CreateCopyAsync(libraryId, this.Copy, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.FormError = ErrorText(ex);
                return this.Page();
            }

            this.StatusMessage = $"Added a copy of {this.Item.Title}.";
            return this.Redirect($"/ui/library/{libraryId}/items/{itemId}");
        }

        private async Task<IActionResult> LoadAsync(long libraryId, long itemId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }
            if (!this.CanEdit)
            {
                return this.Redirect($"/ui/library/{libraryId}/items/{itemId}");
            }

            try
            {
                this.Item = await this.NsClient.Library.GetItemAsync(libraryId, itemId, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }
            this.Locations = await this.NsClient.Library.GetLocationsAsync(libraryId, cancellationToken);
            return null;
        }
    }
}
