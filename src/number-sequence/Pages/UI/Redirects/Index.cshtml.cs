using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Redirects
{
    [RequiresToken]
    public sealed class IndexModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public IndexModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public List<Redirect> Redirects { get; private set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            this.Redirects = await this.nsClient.Redirect.ListAsync(cancellationToken);
            return this.Page();
        }
    }
}
