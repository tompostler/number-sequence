using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.LineDefaults
{
    [RequiresToken]
    public sealed class IndexModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public IndexModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public List<InvoiceLineDefault> LineDefaults { get; private set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            this.LineDefaults = await this.nsClient.Ledger.GetInvoiceLineDefaultsAsync(cancellationToken);
            return this.Page();
        }
    }
}
