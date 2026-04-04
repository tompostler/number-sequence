using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Customers
{
    [RequiresToken]
    public sealed class DetailModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public DetailModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public Customer Customer { get; private set; }

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
    }
}
