using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Customers
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
        public Customer Customer { get; set; }

        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                this.Customer = await this.nsClient.Ledger.GetCustomerAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long id, CancellationToken cancellationToken)
        {
            this.ModelState.Remove($"{nameof(this.Customer)}.{nameof(this.Customer.AccountName)}");

            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            this.Customer.Id = id;

            try
            {
                _ = await this.nsClient.Ledger.UpdateCustomerAsync(this.Customer, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }

            return this.Redirect($"/ui/ledger/customers/{id}");
        }
    }
}
