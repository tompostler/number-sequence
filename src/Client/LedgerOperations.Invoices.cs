using TcpWtf.NumberSequence.Contracts.Ledger;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LedgerOperations
    {
        /// <summary>
        /// Create a new invoice.
        /// </summary>
        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "ledger/invoices")
                {
                    Content = invoice.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }

        /// <summary>
        /// Get an existing invoice.
        /// </summary>
        public async Task<Invoice> GetInvoiceAsync(long id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"ledger/invoices/{id}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }

        /// <summary>
        /// Get existing invoices.
        /// </summary>
        public async Task<List<Invoice>> GetInvoicesAsync(int skip = 0, int take = 20, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"ledger/invoices?skip={skip}&take={take}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<Invoice>>(cancellationToken);
        }

        /// <summary>
        /// Update an existing invoice.
        /// </summary>
        public async Task<Invoice> UpdateInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "ledger/invoices")
                {
                    Content = invoice.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }
    }
}
