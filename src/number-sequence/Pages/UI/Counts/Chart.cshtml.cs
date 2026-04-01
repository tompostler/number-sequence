using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;

namespace number_sequence.Pages.UI.Counts
{
    [RequiresToken]
    public sealed class ChartModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public ChartModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public async Task<IActionResult> OnGetAsync(string name, CancellationToken cancellationToken)
        {
            byte[] bytes = await this.nsClient.Count.GetChartAsync(name, width: 1200, height: 400, cancellationToken: cancellationToken);
            if (bytes == null)
            {
                return this.NotFound();
            }

            return this.File(bytes, "image/png");
        }
    }
}
