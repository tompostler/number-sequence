using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Invoices
{
    [RequiresToken]
    public sealed class IndexModel : PageModel
    {
        private const int Take = 20;
        private readonly NsTcpWtfClient nsClient;

        public IndexModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        [FromQuery]
        public int Skip { get; set; } = 0;

        public List<Invoice> Invoices { get; private set; }
        public bool HasMore { get; private set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            this.Invoices = await this.nsClient.Ledger.GetInvoicesAsync(this.Skip, Take + 1, cancellationToken);
            this.HasMore = this.Invoices.Count > Take;
            if (this.HasMore)
            {
                this.Invoices.RemoveAt(Take);
            }

            return this.Page();
        }
    }
}
