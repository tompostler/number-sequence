using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries.Scans
{
    /// <summary>
    /// The scanning itself. The page is a shell: <c>wwwroot/js/library-scan-session.js</c> loads the entries from the api,
    /// posts each code to it, and lists what each matched. An applied session goes to its reconcile view instead.
    /// </summary>
    [RequiresToken(AccountRoles.Library)]
    public sealed class ScanModel : LibraryPageModel
    {
        public ScanModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        public LibraryScanSession Session { get; private set; }

        public Dictionary<long, string> Paths { get; private set; }

        public async Task<IActionResult> OnGetAsync(long libraryId, long sessionId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }
            try
            {
                this.Session = await this.NsClient.Library.GetScanSessionAsync(libraryId, sessionId, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }
            if (this.Session.CompletedDate.HasValue || !this.CanEdit)
            {
                return this.Redirect($"/ui/library/{libraryId}/scans/{sessionId}/reconcile");
            }

            this.Paths = LibraryDisplay.Paths(await this.NsClient.Library.GetLocationsAsync(libraryId, cancellationToken));
            return this.Page();
        }
    }
}
