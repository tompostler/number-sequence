using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Invoices
{
    [RequiresToken]
    public sealed class AddLineModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public AddLineModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public Invoice Invoice { get; private set; }
        public List<InvoiceLineDefault> LineDefaults { get; private set; }

        [BindProperty]
        public InvoiceLine Line { get; set; }

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

            this.LineDefaults = await this.nsClient.Ledger.GetInvoiceLineDefaultsAsync(cancellationToken);
            this.Line = new InvoiceLine { Quantity = 1 };
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long id, CancellationToken cancellationToken)
        {
            if (!this.ModelState.IsValid)
            {
                await this.LoadAsync(id, cancellationToken);
                return this.Page();
            }

            try
            {
                _ = await this.nsClient.Ledger.CreateInvoiceLineAsync(id, this.Line, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadAsync(id, cancellationToken);
                return this.Page();
            }

            return this.Redirect($"/ui/ledger/invoices/{id}");
        }

        private async Task LoadAsync(long id, CancellationToken cancellationToken)
        {
            try { this.Invoice = await this.nsClient.Ledger.GetInvoiceAsync(id, cancellationToken); }
            catch (NsTcpWtfClientException) { }
            this.LineDefaults = await this.nsClient.Ledger.GetInvoiceLineDefaultsAsync(cancellationToken);
        }
    }
}
