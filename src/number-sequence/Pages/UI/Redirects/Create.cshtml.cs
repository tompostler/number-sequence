using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Redirects
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
        public Redirect Redirect { get; set; }

        public string ErrorMessage { get; private set; }

        public IActionResult OnGet()
        {
            this.Redirect = new() { Id = Guid.NewGuid().ToString().Split('-')[0] };
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            try
            {
                await this.nsClient.Redirect.CreateAsync(this.Redirect, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }

            return this.LocalRedirect("/ui/redirects");
        }
    }
}
