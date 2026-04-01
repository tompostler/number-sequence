using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Counts
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
        public Count Count { get; set; }

        public string ErrorMessage { get; private set; }

        public IActionResult OnGet()
        {
            this.Count = new Count { Name = string.Empty, Value = 0 };
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            // Account is set server-side by the controller based on the token.
            this.ModelState.Remove($"{nameof(this.Count)}.{nameof(this.Count.Account)}");

            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            try
            {
                Count created = await this.nsClient.Count.CreateAsync(this.Count, cancellationToken);
                return this.Redirect($"/ui/counts/{created.Name}");
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }
        }
    }
}
