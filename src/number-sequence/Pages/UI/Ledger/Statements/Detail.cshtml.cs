using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Statements
{
    [RequiresToken]
    public sealed class DetailModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public DetailModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public Statement Statement { get; private set; }
        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                this.Statement = await this.nsClient.Ledger.GetStatementAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }

            return this.Page();
        }

        public async Task<IActionResult> OnPostProcessAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                _ = await this.nsClient.Ledger.UpdateStatementForProcessAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                try { this.Statement = await this.nsClient.Ledger.GetStatementAsync(id, cancellationToken); }
                catch (NsTcpWtfClientException) { }
                return this.Page();
            }

            return this.Redirect($"/ui/ledger/statements/{id}");
        }
    }
}
