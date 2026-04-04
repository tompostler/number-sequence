using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Invoices
{
    [RequiresToken]
    public sealed class CreateModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public CreateModel(NsTcpWtfClient nsClient)
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

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            this.Invoice = new Invoice { DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)) };
            await this.LoadDropdownsAsync(cancellationToken);
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            this.ModelState.Remove($"{nameof(this.Invoice)}.{nameof(this.Invoice.AccountName)}");
            this.ModelState.Remove($"{nameof(this.Invoice)}.{nameof(this.Invoice.Business)}");
            this.ModelState.Remove($"{nameof(this.Invoice)}.{nameof(this.Invoice.Customer)}");

            if (!this.ModelState.IsValid)
            {
                await this.LoadDropdownsAsync(cancellationToken);
                return this.Page();
            }

            this.Invoice.Business = new Business { Id = this.BusinessId };
            this.Invoice.Customer = new Customer { Id = this.CustomerId };

            try
            {
                Invoice created = await this.nsClient.Ledger.CreateInvoiceAsync(this.Invoice, cancellationToken);
                return this.Redirect($"/ui/ledger/invoices/{created.Id}");
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadDropdownsAsync(cancellationToken);
                return this.Page();
            }
        }

        private async Task LoadDropdownsAsync(CancellationToken cancellationToken)
        {
            this.Businesses = await this.nsClient.Ledger.GetBusinessesAsync(cancellationToken);
            this.Customers = await this.nsClient.Ledger.GetCustomersAsync(cancellationToken);
        }
    }
}
