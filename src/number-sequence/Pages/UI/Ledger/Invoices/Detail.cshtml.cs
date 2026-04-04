using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Invoices
{
    [RequiresToken]
    public sealed class DetailModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public DetailModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public Invoice Invoice { get; private set; }
        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                this.Invoice = await this.nsClient.Ledger.GetInvoiceAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }

            return this.Page();
        }

        public async Task<IActionResult> OnPostProcessAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                _ = await this.nsClient.Ledger.UpdateInvoiceForProcessAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadInvoiceAsync(id, cancellationToken);
                return this.Page();
            }

            return this.Redirect($"/ui/ledger/invoices/{id}");
        }

        public async Task<IActionResult> OnPostReprocessRegularlyAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                _ = await this.nsClient.Ledger.UpdateInvoiceReprocessRegularlyAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadInvoiceAsync(id, cancellationToken);
                return this.Page();
            }

            return this.Redirect($"/ui/ledger/invoices/{id}");
        }

        public async Task<IActionResult> OnPostDeleteLineAsync(long id, long lineId, CancellationToken cancellationToken)
        {
            try
            {
                _ = await this.nsClient.Ledger.DeleteInvoiceLineAsync(id, lineId, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadInvoiceAsync(id, cancellationToken);
                return this.Page();
            }

            return this.Redirect($"/ui/ledger/invoices/{id}");
        }

        public async Task<IActionResult> OnPostDeletePaymentAsync(long id, long paymentId, CancellationToken cancellationToken)
        {
            try
            {
                _ = await this.nsClient.Ledger.DeleteInvoicePaymentAsync(id, paymentId, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadInvoiceAsync(id, cancellationToken);
                return this.Page();
            }

            return this.Redirect($"/ui/ledger/invoices/{id}");
        }

        private async Task LoadInvoiceAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                this.Invoice = await this.nsClient.Ledger.GetInvoiceAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException) { }
        }
    }
}
