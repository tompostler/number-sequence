using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries.Items
{
    /// <summary>
    /// Edits an item's fields, and its cover. A cover comes only from a lookup: <c>coverQ</c> is a title search whose
    /// results with a cover are offered, and picking one sets only the cover, leaving the item's fields as they are.
    /// </summary>
    [RequiresToken(AccountRoles.Library)]
    public sealed class EditModel : LibraryPageModel
    {
        public EditModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        [BindProperty]
        public LibraryItem Item { get; set; }

        /// <summary>
        /// The title searched for a cover, or null when no search was made.
        /// </summary>
        [BindProperty(SupportsGet = true, Name = "coverQ")]
        public string CoverQ { get; set; }

        /// <summary>
        /// Search results that have a cover, or null when no search was made.
        /// </summary>
        public List<LibraryLookupResult> CoverCandidates { get; private set; }

        /// <summary>
        /// Why the cover search didn't work, such as a provider that's down.
        /// </summary>
        public string CoverNote { get; private set; }

        /// <summary>
        /// Whether the item's media type has a lookup to take a cover from.
        /// </summary>
        public bool CanFindCover => this.Item?.MediaType is LibraryMediaType.Book or LibraryMediaType.Movie or LibraryMediaType.BoardGame;

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
            await this.LoadSeriesNamesAsync(libraryId, cancellationToken);

            this.CoverQ = this.CoverQ?.Trim();
            if (this.CanFindCover && !string.IsNullOrEmpty(this.CoverQ))
            {
                try
                {
                    List<LibraryLookupResult> results = await this.NsClient.Library.SearchLookupAsync(this.Item.MediaType, this.CoverQ, cancellationToken);
                    this.CoverCandidates = results.Where(x => x.CoverUrls?.Count > 0).ToList();
                }
                catch (NsTcpWtfClientException ex)
                {
                    this.CoverNote = $"The search didn't work: {ErrorText(ex)}";
                }
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
                await this.LoadSeriesNamesAsync(libraryId, cancellationToken);
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
            stored.Notes = this.Item.Notes?.Trim();
            stored.Tags = this.Item.Tags?.Trim();
            stored.Copies = null;

            try
            {
                _ = await this.NsClient.Library.UpdateItemAsync(libraryId, stored, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.FormError = ErrorText(ex);
                await this.LoadSeriesNamesAsync(libraryId, cancellationToken);
                return this.Page();
            }

            this.StatusMessage = "Saved.";
            return this.Redirect($"/ui/library/{libraryId}/items/{itemId}");
        }

        /// <summary>
        /// Sets the cover from a search result picked on this page. The record is fetched again by id rather than taking
        /// cover urls from the form, so what's stored is the lookup's own cover; the api checks the hosts either way.
        /// </summary>
        public async Task<IActionResult> OnPostSetCoverAsync(long libraryId, long itemId, string source, string sourceId, CancellationToken cancellationToken)
        {
            string here = $"/ui/library/{libraryId}/items/{itemId}/edit";
            LibraryLookupResult result;
            try
            {
                result = await this.NsClient.Library.GetLookupAsync(source, sourceId, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = $"The cover couldn't be looked up: {ErrorText(ex)}";
                return this.Redirect(here);
            }
            if (!(result?.CoverUrls?.Count > 0))
            {
                this.ErrorMessage = "That result has no cover.";
                return this.Redirect(here);
            }

            return await this.RunAndRedirectAsync(
                () => this.NsClient.Library.SetItemCoverAsync(libraryId, itemId, new LibraryCover { SourceUrls = result.CoverUrls }, cancellationToken),
                "Cover saved.",
                here);
        }

        /// <summary>
        /// Clears a wrong cover from a lookup, for an item that's better with none.
        /// </summary>
        public Task<IActionResult> OnPostRemoveCoverAsync(long libraryId, long itemId, CancellationToken cancellationToken)
            => this.RunAndRedirectAsync(
                () => this.NsClient.Library.DeleteItemCoverAsync(libraryId, itemId, cancellationToken),
                "Cover removed.",
                $"/ui/library/{libraryId}/items/{itemId}/edit");
    }
}
