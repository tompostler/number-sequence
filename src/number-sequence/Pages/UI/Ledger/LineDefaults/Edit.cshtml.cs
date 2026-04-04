using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.LineDefaults
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
        public InvoiceLineDefault LineDefault { get; set; }

        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                this.LineDefault = await this.nsClient.Ledger.GetInvoiceLineDefaultAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long id, CancellationToken cancellationToken)
        {
            this.ModelState.Remove($"{nameof(this.LineDefault)}.{nameof(this.LineDefault.AccountName)}");

            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            this.LineDefault.Id = id;

            try
            {
                _ = await this.nsClient.Ledger.UpdateInvoiceLineDefaultAsync(this.LineDefault, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }

            return this.Redirect("/ui/ledger/line-defaults");
        }
    }
}
