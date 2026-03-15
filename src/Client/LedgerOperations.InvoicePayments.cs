using TcpWtf.NumberSequence.Contracts.Ledger;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LedgerOperations
    {
        /// <summary>
        /// Add a payment to an existing invoice.
        /// </summary>
        public async Task<Invoice> CreateInvoicePaymentAsync(long invoiceId, InvoicePayment payment, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"ledger/invoices/{invoiceId}/payments")
                {
                    Content = payment.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }

        /// <summary>
        /// Update the details of an existing payment (date and details only, not amount).
        /// </summary>
        public async Task<Invoice> UpdateInvoicePaymentAsync(long invoiceId, long paymentId, InvoicePayment payment, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"ledger/invoices/{invoiceId}/payments/{paymentId}")
                {
                    Content = payment.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }

        /// <summary>
        /// Remove a payment from an existing invoice.
        /// </summary>
        public async Task<Invoice> DeleteInvoicePaymentAsync(long invoiceId, long paymentId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"ledger/invoices/{invoiceId}/payments/{paymentId}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }
    }
}
