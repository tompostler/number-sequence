using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Statements
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
        public Statement Statement { get; set; }

        [BindProperty]
        public long BusinessId { get; set; }

        [BindProperty]
        public long CustomerId { get; set; }

        public List<Business> Businesses { get; private set; }
        public List<Customer> Customers { get; private set; }
        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            this.Statement = new Statement
            {
                InvoiceStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                InvoiceEndDate = DateOnly.FromDateTime(DateTime.UtcNow),
            };
            await this.LoadDropdownsAsync(cancellationToken);
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            this.ModelState.Remove($"{nameof(this.Statement)}.{nameof(this.Statement.AccountName)}");
            this.ModelState.Remove($"{nameof(this.Statement)}.{nameof(this.Statement.Business)}");
            this.ModelState.Remove($"{nameof(this.Statement)}.{nameof(this.Statement.Customer)}");

            if (!this.ModelState.IsValid)
            {
                await this.LoadDropdownsAsync(cancellationToken);
                return this.Page();
            }

            this.Statement.Business = new Business { Id = this.BusinessId };
            this.Statement.Customer = new Customer { Id = this.CustomerId };

            try
            {
                Statement created = await this.nsClient.Ledger.CreateStatementAsync(this.Statement, cancellationToken);
                return this.Redirect($"/ui/ledger/statements/{created.Id}");
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
