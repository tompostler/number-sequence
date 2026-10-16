using TcpWtf.NumberSequence.Contracts.Libraries;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LibraryOperations
    {
        /// <summary>
        /// Get a library's loans, each with its copy and item. Open loans sort by due date (none last); all loans sort newest
        /// first.
        /// </summary>
        public async Task<List<LibraryLoan>> GetLoansAsync(long libraryId, bool open = true, int skip = 0, int take = 100, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{libraryId}/loans?open={(open ? "true" : "false")}&skip={skip}&take={take}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<LibraryLoan>>(cancellationToken);
        }

        /// <summary>
        /// "Do I already own this?" Copies with this exact barcode, across every library the caller can access, each with its
        /// item and open loan. Disposed copies are left out.
        /// </summary>
        public async Task<List<LibraryCopy>> GetCopiesByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/barcodes/{Uri.EscapeDataString(barcode)}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<LibraryCopy>>(cancellationToken);
        }
    }
}
