using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Invoices
{
    [RequiresToken]
    public sealed class EditLineModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public EditLineModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public Invoice Invoice { get; private set; }

        [BindProperty]
        public InvoiceLine Line { get; set; }

        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(long id, long lineId, CancellationToken cancellationToken)
        {
            Invoice invoice;
            try
            {
                invoice = await this.nsClient.Ledger.GetInvoiceAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }

            InvoiceLine line = invoice.Lines?.SingleOrDefault(x => x.Id == lineId);
            if (line == null)
            {
                return this.NotFound();
            }

            this.Invoice = invoice;
            this.Line = line;
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long id, long lineId, CancellationToken cancellationToken)
        {
            if (!this.ModelState.IsValid)
            {
                await this.LoadInvoiceAsync(id, cancellationToken);
                return this.Page();
            }

            try
            {
                _ = await this.nsClient.Ledger.UpdateInvoiceLineAsync(id, lineId, this.Line, cancellationToken);
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
