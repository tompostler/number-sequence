using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcpWtf.NumberSequence.Client;

namespace number_sequence.Pages.UI.History
{
    public sealed class IndexModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public IndexModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public string History { get; private set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            this.History = await this.nsClient.History.GetAsync(cancellationToken);
            return this.Page();
        }
    }
}
