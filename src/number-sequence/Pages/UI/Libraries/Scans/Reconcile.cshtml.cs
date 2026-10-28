using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries.Scans
{
    /// <summary>
    /// What finishing a scan would do, as choices, and applying them. Entries are grouped by where their copy is now, not
    /// where it was when scanned, because that's what applying acts on. An applied session shows the same groups read-only.
    /// </summary>
    [RequiresToken(AccountRoles.Library)]
    public sealed class ReconcileModel : LibraryPageModel
    {
        public ReconcileModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        public LibraryScanReconcile Reconcile { get; private set; }

        public Dictionary<long, string> Paths { get; private set; }

        public LibraryScanSession Session => this.Reconcile.Session;

        public bool Applied => this.Session.CompletedDate.HasValue;

        public string LocationPath => LibraryDisplay.LocationPath(this.Paths, this.Session.LocationId);

        /// <summary>
        /// Matched copies recorded here: these are verified.
        /// </summary>
        public List<LibraryScanEntry> Here => this.Matched.Where(x => x.ResolvedCopy.LocationId == this.Session.LocationId).ToList();

        /// <summary>
        /// Matched copies recorded somewhere else, or nowhere: these can be moved here.
        /// </summary>
        public List<LibraryScanEntry> Elsewhere => this.Matched.Where(x => x.ResolvedCopy.LocationId != this.Session.LocationId).ToList();

        /// <summary>
        /// Matched copies deleted or disposed since they were scanned, which applying leaves alone.
        /// </summary>
        public List<LibraryScanEntry> Gone => this.Session.Entries
            .Where(x => x.Resolution is LibraryScanResolution.Expected or LibraryScanResolution.Elsewhere)
            .Where(x => x.ResolvedCopy == null || x.ResolvedCopy.Status == LibraryCopyStatus.Disposed)
            .ToList();

        public List<LibraryScanEntry> New => this.Session.Entries.Where(x => x.Resolution == LibraryScanResolution.NotInLibrary).ToList();

        public List<LibraryScanEntry> Added => this.Session.Entries.Where(x => x.Resolution == LibraryScanResolution.Added).ToList();

        private IEnumerable<LibraryScanEntry> Matched => this.Session.Entries
            .Where(x => x.Resolution is LibraryScanResolution.Expected or LibraryScanResolution.Elsewhere)
            .Where(x => x.ResolvedCopy != null && x.ResolvedCopy.Status != LibraryCopyStatus.Disposed);

        public static bool IsIsbn(LibraryScanEntry entry)
            => LibraryBarcode.TryParse(entry.Barcode, out LibraryBarcode code, out _) && code.Kind == LibraryBarcodeKind.Isbn;

        /// <summary>
        /// The add page for a scan that can't be added by lookup alone, coming back here once saved.
        /// </summary>
        public string AddUrl(LibraryScanEntry entry)
            => $"/ui/library/{this.Library.Id}/items/create?barcode={Uri.EscapeDataString(entry.Barcode)}&location={this.Session.LocationId}&scan={this.Session.Id}&scanEntry={entry.Id}";

        public async Task<IActionResult> OnGetAsync(long libraryId, long sessionId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }
            try
            {
                this.Reconcile = await this.NsClient.Library.GetScanReconcileAsync(libraryId, sessionId, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }
            this.Paths = LibraryDisplay.Paths(await this.NsClient.Library.GetLocationsAsync(libraryId, cancellationToken));
            return this.Page();
        }

        public async Task<IActionResult> OnPostApplyAsync(long libraryId, long sessionId, List<long> move, List<long> returned, List<long> missing, CancellationToken cancellationToken)
        {
            string here = $"/ui/library/{libraryId}/scans/{sessionId}/reconcile";
            try
            {
                _ = await this.NsClient.Library.ApplyScanSessionAsync(
                    libraryId,
                    sessionId,
                    new LibraryScanApply { MoveCopyIds = move ?? [], ReturnCopyIds = returned ?? [], MissingCopyIds = missing ?? [] },
                    cancellationToken);
                List<string> done = [];
                if (move?.Count > 0)
                {
                    done.Add($"{move.Count} moved here");
                }
                if (returned?.Count > 0)
                {
                    done.Add($"{returned.Count} returned");
                }
                if (missing?.Count > 0)
                {
                    done.Add($"{missing.Count} marked missing");
                }
                this.StatusMessage = done.Count == 0 ? "Applied. Every copy scanned here is verified." : $"Applied: {string.Join(", ", done)}, and every copy scanned here verified.";
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ErrorText(ex);
            }
            return this.Redirect(here);
        }

        public Task<IActionResult> OnPostDeleteAsync(long libraryId, long sessionId, CancellationToken cancellationToken)
            => this.RunAndRedirectAsync(
                () => this.NsClient.Library.DeleteScanSessionAsync(libraryId, sessionId, cancellationToken),
                "Scan discarded. Copies added from it are kept.",
                $"/ui/library/{libraryId}/scans");
    }
}
