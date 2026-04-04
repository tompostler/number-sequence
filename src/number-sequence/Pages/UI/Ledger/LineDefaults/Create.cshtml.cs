using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.LineDefaults
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
        public InvoiceLineDefault LineDefault { get; set; }

        public string ErrorMessage { get; private set; }

        public IActionResult OnGet()
        {
            this.LineDefault = new InvoiceLineDefault { Quantity = 1 };
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            this.ModelState.Remove($"{nameof(this.LineDefault)}.{nameof(this.LineDefault.AccountName)}");

            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            try
            {
                _ = await this.nsClient.Ledger.CreateInvoiceLineDefaultAsync(this.LineDefault, cancellationToken);
                return this.Redirect("/ui/ledger/line-defaults");
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }
        }
    }
}
