using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries.Items
{
    /// <summary>
    /// Shows the api's comparison of an item with a fresh lookup (<c>LibraryController.Refresh</c>), field by field, for
    /// choosing which to take. Each offered value is an editable box, so a series can be respelled while taking it, and the
    /// boxes are what's posted back.
    /// </summary>
    [RequiresToken(AccountRoles.Library)]
    public sealed class RefreshModel : LibraryPageModel
    {
        public RefreshModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        /// <summary>
        /// The copy to look up by, when the item has several with an ISBN.
        /// </summary>
        [BindProperty(SupportsGet = true, Name = "copy")]
        public long? CopyId { get; set; }

        /// <summary>
        /// The ticked fields, from <see cref="LibraryItemRefreshFields"/>.
        /// </summary>
        [BindProperty]
        public List<string> Take { get; set; } = [];

        /// <summary>
        /// Each box's value by field, posted as <c>Value[field]</c>.
        /// </summary>
        [BindProperty]
        public Dictionary<string, string> Value { get; set; } = [];

        public LibraryItem Item { get; private set; }

        public LibraryItemRefresh Refresh { get; private set; }

        /// <summary>
        /// Why there's nothing to compare, from the api or from the call failing.
        /// </summary>
        public string LookupNote { get; private set; }

        public async Task<IActionResult> OnGetAsync(long libraryId, long itemId, CancellationToken cancellationToken)
        {
            if (await this.LoadAsync(libraryId, itemId, cancellationToken) is IActionResult result)
            {
                return result;
            }

            try
            {
                this.Refresh = await this.NsClient.Library.GetItemRefreshAsync(libraryId, itemId, this.CopyId, cancellationToken);
                this.LookupNote = this.Refresh.Note;
            }
            catch (NsTcpWtfClientException ex)
            {
                this.LookupNote = $"Lookup didn't work: {ErrorText(ex)}";
            }
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long libraryId, long itemId, CancellationToken cancellationToken)
        {
            string here = $"/ui/library/{libraryId}/items/{itemId}";
            if (this.Take.Count == 0)
            {
                this.StatusMessage = "Nothing taken; the item is as it was.";
                return this.Redirect(here);
            }

            LibraryItemRefreshApply apply = new()
            {
                CopyId = this.CopyId,
                Take = this.Take.Distinct().ToDictionary(x => x, x => this.Value.GetValueOrDefault(x)),
            };
            try
            {
                LibraryItemRefreshApplied applied = await this.NsClient.Library.ApplyItemRefreshAsync(libraryId, itemId, apply, cancellationToken);
                this.StatusMessage = (applied.Taken.Count == 0 ? "Nothing changed." : $"Took the {Join(applied.Taken)}.") + (applied.CoverNote == null ? string.Empty : $" {applied.CoverNote}");
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = $"Nothing was taken: {ErrorText(ex)}";
            }
            return this.Redirect(here);
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
            await this.LoadSeriesNamesAsync(libraryId, cancellationToken);
            return null;
        }

        private static string Join(List<string> parts)
            => parts.Count <= 1 ? string.Join(string.Empty, parts) : $"{string.Join(", ", parts.Take(parts.Count - 1))} and {parts[^1]}";
    }
}
