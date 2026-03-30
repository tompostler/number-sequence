using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcpWtf.NumberSequence.Client;

namespace number_sequence.Pages.UI.Account
{
    public sealed class CreateModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public CreateModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        [BindProperty]
        public TcpWtf.NumberSequence.Contracts.Account Account { get; set; }

        public string ErrorMessage { get; private set; }

        public IActionResult OnGet() => this.Page();

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            try
            {
                _ = await this.nsClient.Account.CreateAsync(this.Account, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }

            return this.Redirect("/ui/login");
        }
    }
}
