using TcpWtf.NumberSequence.Contracts.Libraries;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LibraryOperations
    {
        /// <summary>
        /// Get a library's open scan sessions, newest first, or with <paramref name="open"/> false the newest few of
        /// any kind. Each has its entries, without their copies.
        /// </summary>
        public async Task<List<LibraryScanSession>> GetScanSessionsAsync(long libraryId, bool open = true, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{libraryId}/scans?open={(open ? "true" : "false")}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<LibraryScanSession>>(cancellationToken);
        }

        /// <summary>
        /// Start a scan session at a location.
        /// </summary>
        public async Task<LibraryScanSession> CreateScanSessionAsync(long libraryId, long locationId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/scans")
                {
                    Content = new LibraryScanSession { LocationId = locationId }.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryScanSession>(cancellationToken);
        }

        /// <summary>
        /// Get a scan session with its entries in scan order, each with the copy it matched as that copy is now.
        /// </summary>
        public async Task<LibraryScanSession> GetScanSessionAsync(long libraryId, long sessionId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{libraryId}/scans/{sessionId}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryScanSession>(cancellationToken);
        }

        /// <summary>
        /// Abandon a scan session, or forget an applied one. Copies added from it stay.
        /// </summary>
        public async Task DeleteScanSessionAsync(long libraryId, long sessionId, CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"library/libraries/{libraryId}/scans/{sessionId}"),
                cancellationToken);
        }

        /// <summary>
        /// Record a scanned code, matched to a copy as it is recorded. The entry comes back with that copy.
        /// </summary>
        public async Task<LibraryScanEntry> AddScanEntryAsync(long libraryId, long sessionId, string barcode, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/scans/{sessionId}/entries")
                {
                    Content = new LibraryScanEntry { Barcode = barcode }.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryScanEntry>(cancellationToken);
        }

        /// <summary>
        /// Remove an accidental scan. The session's other entries are matched again, since this one may have claimed a
        /// copy a later scan missed.
        /// </summary>
        public async Task DeleteScanEntryAsync(long libraryId, long sessionId, long entryId, CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"library/libraries/{libraryId}/scans/{sessionId}/entries/{entryId}"),
                cancellationToken);
        }

        /// <summary>
        /// Add a not-in-library ISBN from a scan: look it up and put a copy at the session's location, on the matching
        /// item or a new one.
        /// </summary>
        public async Task<LibraryScanEntry> AddScanEntryToLibraryAsync(long libraryId, long sessionId, long entryId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/scans/{sessionId}/entries/{entryId}/add"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryScanEntry>(cancellationToken);
        }

        /// <summary>
        /// Mark a not-in-library entry as added, after its copy was created on the add page.
        /// </summary>
        public async Task<LibraryScanEntry> ResolveScanEntryAsync(long libraryId, long sessionId, long entryId, long copyId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/scans/{sessionId}/entries/{entryId}/resolve/{copyId}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryScanEntry>(cancellationToken);
        }

        /// <summary>
        /// Get what applying a scan session could do: its entries with their copies, and the copies recorded at its
        /// location that no scan matched.
        /// </summary>
        public async Task<LibraryScanReconcile> GetScanReconcileAsync(long libraryId, long sessionId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{libraryId}/scans/{sessionId}/reconcile"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryScanReconcile>(cancellationToken);
        }

        /// <summary>
        /// Apply a scan session with the choices from its reconcile view, and close it.
        /// </summary>
        public async Task<LibraryScanSession> ApplyScanSessionAsync(long libraryId, long sessionId, LibraryScanApply apply, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/scans/{sessionId}/apply")
                {
                    Content = apply.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryScanSession>(cancellationToken);
        }
    }
}
