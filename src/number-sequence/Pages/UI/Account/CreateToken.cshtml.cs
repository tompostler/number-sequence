using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Account
{
    [RequiresToken]
    public sealed class CreateTokenModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public CreateTokenModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        [BindProperty]
        public Token Token { get; set; }

        public string ErrorMessage { get; private set; }

        public IActionResult OnGet()
        {
            this.Token = new() { Name = "default", ExpirationDate = DateTimeOffset.UtcNow.AddDays(90) };
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            // Account is set server-side; remove from validation to avoid Required failing on null.
            _ = this.ModelState.Remove($"{nameof(this.Token)}.{nameof(this.Token.Account)}");

            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            this.Token.Account = this.User.Identity.Name;

            try
            {
                _ = await this.nsClient.Token.CreateAsync(this.Token, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }

            return this.Redirect("/ui/account");
        }
    }
}
