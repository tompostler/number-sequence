using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries.Items
{
    [RequiresToken(AccountRoles.Library)]
    public sealed class EditModel : LibraryPageModel
    {
        public EditModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        [BindProperty]
        public LibraryItem Item { get; set; }

        public async Task<IActionResult> OnGetAsync(long libraryId, long itemId, CancellationToken cancellationToken)
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
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long libraryId, long itemId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }
            this.Item.Id = itemId;
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            // The update replaces every editable field, and the form doesn't carry external ids or attributes. Apply the
            // form onto the stored item so those survive.
            LibraryItem stored;
            try
            {
                stored = await this.NsClient.Library.GetItemAsync(libraryId, itemId, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }
            stored.MediaType = this.Item.MediaType;
            stored.Title = this.Item.Title?.Trim();
            stored.Subtitle = this.Item.Subtitle?.Trim();
            stored.Creators = this.Item.Creators?.Trim();
            stored.Series = this.Item.Series?.Trim();
            stored.SeriesNumber = this.Item.SeriesNumber;
            stored.Year = this.Item.Year;
            stored.Summary = this.Item.Summary?.Trim();
            stored.Tags = this.Item.Tags?.Trim();
            stored.Copies = null;

            try
            {
                _ = await this.NsClient.Library.UpdateItemAsync(libraryId, stored, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.FormError = ErrorText(ex);
                return this.Page();
            }

            this.StatusMessage = "Saved.";
            return this.Redirect($"/ui/library/{libraryId}/items/{itemId}");
        }
    }
}
