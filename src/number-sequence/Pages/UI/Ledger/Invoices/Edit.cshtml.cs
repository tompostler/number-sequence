using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Invoices
{
    [RequiresToken]
    public sealed class EditModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public EditModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        [BindProperty]
        public Invoice Invoice { get; set; }

        [BindProperty]
        public long BusinessId { get; set; }

        [BindProperty]
        public long CustomerId { get; set; }

        public List<Business> Businesses { get; private set; }
        public List<Customer> Customers { get; private set; }
        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(long id, CancellationToken cancellationToken)
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

            this.Invoice = invoice;
            this.BusinessId = invoice.Business.Id;
            this.CustomerId = invoice.Customer.Id;
            await this.LoadDropdownsAsync(cancellationToken);
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long id, CancellationToken cancellationToken)
        {
            this.ModelState.Remove($"{nameof(this.Invoice)}.{nameof(this.Invoice.AccountName)}");
            this.ModelState.Remove($"{nameof(this.Invoice)}.{nameof(this.Invoice.Business)}");
            this.ModelState.Remove($"{nameof(this.Invoice)}.{nameof(this.Invoice.Customer)}");

            if (!this.ModelState.IsValid)
            {
                await this.LoadDropdownsAsync(cancellationToken);
                return this.Page();
            }

            // Preserve existing lines, payments, and processing flags from current server state.
            Invoice current = await this.nsClient.Ledger.GetInvoiceAsync(id, cancellationToken);
            this.Invoice.Id = id;
            this.Invoice.Business = new Business { Id = this.BusinessId };
            this.Invoice.Customer = new Customer { Id = this.CustomerId };
            this.Invoice.Lines = current.Lines;
            this.Invoice.Payments = current.Payments;
            this.Invoice.ReadyForProcessing = current.ReadyForProcessing;
            this.Invoice.ProcessedAt = current.ProcessedAt;
            this.Invoice.ReprocessRegularly = current.ReprocessRegularly;

            try
            {
                _ = await this.nsClient.Ledger.UpdateInvoiceAsync(this.Invoice, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadDropdownsAsync(cancellationToken);
                return this.Page();
            }

            return this.Redirect($"/ui/ledger/invoices/{id}");
        }

        private async Task LoadDropdownsAsync(CancellationToken cancellationToken)
        {
            this.Businesses = await this.nsClient.Ledger.GetBusinessesAsync(cancellationToken);
            this.Customers = await this.nsClient.Ledger.GetCustomersAsync(cancellationToken);
        }
    }
}
