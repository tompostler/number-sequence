using TcpWtf.NumberSequence.Contracts.Ledger;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LedgerOperations
    {
        /// <summary>
        /// Create a new line default.
        /// </summary>
        public async Task<InvoiceLineDefault> CreateInvoiceLineDefaultAsync(InvoiceLineDefault lineDefault, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "ledger/invoicelinedefaults")
                {
                    Content = lineDefault.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceLineDefault>(cancellationToken);
        }

        /// <summary>
        /// Get an existing line default.
        /// </summary>
        public async Task<InvoiceLineDefault> GetInvoiceLineDefaultAsync(long id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"ledger/invoicelinedefaults/{id}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceLineDefault>(cancellationToken);
        }

        /// <summary>
        /// Get existing line defaults.
        /// </summary>
        public async Task<List<InvoiceLineDefault>> GetInvoiceLineDefaultsAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "ledger/invoicelinedefaults"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<InvoiceLineDefault>>(cancellationToken);
        }

        /// <summary>
        /// Update an existing line default.
        /// </summary>
        public async Task<InvoiceLineDefault> UpdateInvoiceLineDefaultAsync(InvoiceLineDefault lineDefault, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "ledger/invoicelinedefaults")
                {
                    Content = lineDefault.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceLineDefault>(cancellationToken);
        }
    }
}
