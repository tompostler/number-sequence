using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Businesses
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
        public Business Business { get; set; }

        public string ErrorMessage { get; private set; }

        public IActionResult OnGet()
        {
            this.Business = new Business();
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            this.ModelState.Remove($"{nameof(this.Business)}.{nameof(this.Business.AccountName)}");

            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            try
            {
                Business created = await this.nsClient.Ledger.CreateBusinessAsync(this.Business, cancellationToken);
                return this.Redirect($"/ui/ledger/businesses/{created.Id}");
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }
        }
    }
}
