using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries
{
    /// <summary>
    /// The libraries the caller can see, and one search across all of them. That search is the "do I already own this?"
    /// check while shopping, so it doesn't ask for a library first.
    /// </summary>
    [RequiresToken(AccountRoles.Library)]
    public sealed class IndexModel : LibraryPageModel
    {
        private const int TakePerLibrary = 25;

        public IndexModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        public List<Library> Libraries { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string Q { get; set; }

        /// <summary>
        /// The search, normalized, when it reads as a barcode. A barcode with no match offers to add it.
        /// </summary>
        public string QueryBarcode { get; private set; }

        public List<ItemCard> Results { get; private set; } = [];

        public bool AnyResultsTruncated { get; private set; }

        [BindProperty]
        public Library NewLibrary { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            this.Libraries = await this.NsClient.Library.GetLibrariesAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(this.Q))
            {
                return this.Page();
            }

            if (LibraryBarcode.TryParse(this.Q, out LibraryBarcode parsed, out _) && parsed.Kind != LibraryBarcodeKind.Other)
            {
                this.QueryBarcode = parsed.Value;
            }

            // A handful of libraries at most, so one search each is simpler than a cross-library search endpoint.
            foreach (Library library in this.Libraries)
            {
                List<LibraryItem> items = await this.NsClient.Library.GetItemsAsync(library.Id, this.Q, take: TakePerLibrary, cancellationToken: cancellationToken);
                if (items.Count == 0)
                {
                    continue;
                }
                this.AnyResultsTruncated |= items.Count == TakePerLibrary;
                Dictionary<long, string> paths = LibraryDisplay.Paths(await this.NsClient.Library.GetLocationsAsync(library.Id, cancellationToken));
                this.Results.AddRange(items.Select(x => new ItemCard(x, paths, this.Libraries.Count > 1 ? library.Name : null)));
            }

            return this.Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(this.NewLibrary?.Name))
            {
                this.ErrorMessage = "A library needs a name.";
                return this.Redirect("/ui/library");
            }

            try
            {
                Library created = await this.NsClient.Library.CreateLibraryAsync(this.NewLibrary, cancellationToken);
                this.StatusMessage = $"Created {created.Name}. Add some locations next, so copies have somewhere to live.";
                return this.Redirect($"/ui/library/{created.Id}/locations");
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ErrorText(ex);
                return this.Redirect("/ui/library");
            }
        }
    }
}
