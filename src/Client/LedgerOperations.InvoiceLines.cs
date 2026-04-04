using TcpWtf.NumberSequence.Contracts.Ledger;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LedgerOperations
    {
        /// <summary>
        /// Add a new line to an existing invoice.
        /// </summary>
        public async Task<Invoice> CreateInvoiceLineAsync(long invoiceId, InvoiceLine line, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"ledger/invoices/{invoiceId}/lines")
                {
                    Content = line.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }

        /// <summary>
        /// Update a single line on an existing invoice.
        /// </summary>
        public async Task<Invoice> UpdateInvoiceLineAsync(long invoiceId, long lineId, InvoiceLine line, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"ledger/invoices/{invoiceId}/lines/{lineId}")
                {
                    Content = line.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }

        /// <summary>
        /// Delete a single line from an existing invoice.
        /// </summary>
        public async Task<Invoice> DeleteInvoiceLineAsync(long invoiceId, long lineId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"ledger/invoices/{invoiceId}/lines/{lineId}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }
    }
}
