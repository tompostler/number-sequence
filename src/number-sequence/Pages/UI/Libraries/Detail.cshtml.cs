using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries
{
    [RequiresToken(AccountRoles.Library)]
    public sealed class DetailModel : LibraryPageModel
    {
        public const int PageSize = 50;

        public DetailModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        [BindProperty(SupportsGet = true)]
        public string Q { get; set; }

        [BindProperty(SupportsGet = true)]
        public LibraryMediaType? Type { get; set; }

        [BindProperty(SupportsGet = true)]
        public long? Location { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool LoanedOut { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Skip { get; set; }

        public List<LibraryLocation> Locations { get; private set; }

        public Dictionary<long, string> Paths { get; private set; }

        public List<LibraryItem> Items { get; private set; }

        public bool Filtered => !string.IsNullOrWhiteSpace(this.Q) || this.Type.HasValue || this.Location.HasValue || this.LoanedOut;

        public async Task<IActionResult> OnGetAsync(long libraryId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }

            this.Locations = await this.NsClient.Library.GetLocationsAsync(libraryId, cancellationToken);
            this.Paths = LibraryDisplay.Paths(this.Locations);
            try
            {
                this.Items = await this.NsClient.Library.GetItemsAsync(libraryId, this.Q, this.Type, this.Location, this.LoanedOut, Math.Max(0, this.Skip), PageSize, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.FormError = ErrorText(ex);
                this.Items = [];
            }
            return this.Page();
        }

        /// <summary>
        /// The query string for another page of the same filters.
        /// </summary>
        public string PageQuery(int skip)
        {
            Dictionary<string, string> query = [];
            if (!string.IsNullOrWhiteSpace(this.Q))
            {
                query["q"] = this.Q;
            }
            if (this.Type.HasValue)
            {
                query["type"] = this.Type.ToString();
            }
            if (this.Location.HasValue)
            {
                query["location"] = this.Location.ToString();
            }
            if (this.LoanedOut)
            {
                query["loanedOut"] = "true";
            }
            if (skip > 0)
            {
                query["skip"] = skip.ToString();
            }
            return query.Count == 0 ? string.Empty : "?" + string.Join("&", query.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
        }

        public async Task<IActionResult> OnPostLeaveAsync(long libraryId, CancellationToken cancellationToken)
            => await this.RunAndRedirectAsync(
                () => this.NsClient.Library.DeleteShareAsync(libraryId, this.User.Identity.Name, cancellationToken),
                "You've left the library. Its owner can share it with you again.",
                "/ui/library");
    }
}
