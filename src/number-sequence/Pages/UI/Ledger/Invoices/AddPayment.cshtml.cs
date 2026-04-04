using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Invoices
{
    [RequiresToken]
    public sealed class AddPaymentModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public AddPaymentModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public Invoice Invoice { get; private set; }

        [BindProperty]
        public InvoicePayment Payment { get; set; }

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

            this.Payment = new InvoicePayment { PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow) };
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long id, CancellationToken cancellationToken)
        {
            this.ModelState.Remove($"{nameof(this.Payment)}.{nameof(this.Payment.AccountName)}");

            if (!this.ModelState.IsValid)
            {
                await this.LoadInvoiceAsync(id, cancellationToken);
                return this.Page();
            }

            try
            {
                _ = await this.nsClient.Ledger.CreateInvoicePaymentAsync(id, this.Payment, cancellationToken);
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
            try { this.Invoice = await this.nsClient.Ledger.GetInvoiceAsync(id, cancellationToken); }
            catch (NsTcpWtfClientException) { }
        }
    }
}
