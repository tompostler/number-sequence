using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Customers
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
        public Customer Customer { get; set; }

        public string ErrorMessage { get; private set; }

        public IActionResult OnGet()
        {
            this.Customer = new Customer();
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            this.ModelState.Remove($"{nameof(this.Customer)}.{nameof(this.Customer.AccountName)}");

            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            try
            {
                Customer created = await this.nsClient.Ledger.CreateCustomerAsync(this.Customer, cancellationToken);
                return this.Redirect($"/ui/ledger/customers/{created.Id}");
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }
        }
    }
}
