using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using number_sequence.Services.Libraries;
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

        /// <summary>
        /// Replaces the cover with an uploaded image, such as a photo taken on the phone. The api resizes it.
        /// </summary>
        public async Task<IActionResult> OnPostCoverAsync(long libraryId, long itemId, IFormFile coverFile, CancellationToken cancellationToken)
        {
            string here = $"/ui/library/{libraryId}/items/{itemId}/edit";
            if (coverFile == null || coverFile.Length == 0)
            {
                this.ErrorMessage = "Choose an image first.";
                return this.Redirect(here);
            }
            if (coverFile.Length > LibraryCovers.MaxInputBytes)
            {
                this.ErrorMessage = $"That image is {coverFile.Length / 1024 / 1024} MB; the most accepted is {LibraryCovers.MaxInputBytes / 1024 / 1024} MB.";
                return this.Redirect(here);
            }

            using MemoryStream buffer = new();
            await coverFile.CopyToAsync(buffer, cancellationToken);
            return await this.RunAndRedirectAsync(
                () => this.NsClient.Library.SetItemCoverAsync(libraryId, itemId, new LibraryCover { Data = buffer.ToArray() }, cancellationToken),
                "Cover saved.",
                here);
        }

        public Task<IActionResult> OnPostRemoveCoverAsync(long libraryId, long itemId, CancellationToken cancellationToken)
            => this.RunAndRedirectAsync(
                () => this.NsClient.Library.DeleteItemCoverAsync(libraryId, itemId, cancellationToken),
                "Cover removed.",
                $"/ui/library/{libraryId}/items/{itemId}/edit");
    }
}
