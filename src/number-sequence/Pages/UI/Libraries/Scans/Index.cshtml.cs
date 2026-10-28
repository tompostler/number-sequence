using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries.Scans
{
    /// <summary>
    /// Where scanning starts: open sessions to resume, recently applied ones, and starting one at a location.
    /// </summary>
    [RequiresToken(AccountRoles.Library)]
    public sealed class IndexModel : LibraryPageModel
    {
        public IndexModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        /// <summary>
        /// The location to preselect, from a location's Scan link.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public long? Location { get; set; }

        public List<LibraryLocation> Locations { get; private set; }

        public Dictionary<long, string> Paths { get; private set; }

        public List<LibraryScanSession> Open { get; private set; }

        public List<LibraryScanSession> Applied { get; private set; }

        public async Task<IActionResult> OnGetAsync(long libraryId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }

            this.Locations = await this.NsClient.Library.GetLocationsAsync(libraryId, cancellationToken);
            this.Paths = LibraryDisplay.Paths(this.Locations);
            this.Open = await this.NsClient.Library.GetScanSessionsAsync(libraryId, open: true, cancellationToken);
            this.Applied = (await this.NsClient.Library.GetScanSessionsAsync(libraryId, open: false, cancellationToken)).Where(x => x.CompletedDate.HasValue).ToList();
            return this.Page();
        }

        public async Task<IActionResult> OnPostStartAsync(long libraryId, CancellationToken cancellationToken)
        {
            if (!this.Location.HasValue)
            {
                this.ErrorMessage = "Pick the location you're scanning.";
                return this.Redirect($"/ui/library/{libraryId}/scans");
            }
            try
            {
                LibraryScanSession session = await this.NsClient.Library.CreateScanSessionAsync(libraryId, this.Location.Value, cancellationToken);
                return this.Redirect($"/ui/library/{libraryId}/scans/{session.Id}");
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ErrorText(ex);
                return this.Redirect($"/ui/library/{libraryId}/scans?location={this.Location}");
            }
        }

        public Task<IActionResult> OnPostDeleteAsync(long libraryId, long sessionId, CancellationToken cancellationToken)
            => this.RunAndRedirectAsync(
                () => this.NsClient.Library.DeleteScanSessionAsync(libraryId, sessionId, cancellationToken),
                "Scan discarded. Nothing it would have changed was changed.",
                $"/ui/library/{libraryId}/scans");
    }
}
