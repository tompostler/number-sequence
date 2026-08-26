using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.PdfStatus
{
    [RequiresToken(AccountRoles.Chiro, AccountRoles.PdfStatus)]
    public sealed class ChartModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public ChartModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public async Task<IActionResult> OnGetAsync(int daysLookback, double hoursOffset, CancellationToken cancellationToken)
        {
            byte[] bytes = await this.nsClient.PdfStatus.GetChartAsync(
                daysLookback: daysLookback,
                hoursOffset: hoursOffset,
                width: 1200,
                height: 600,
                cancellationToken: cancellationToken);
            if (bytes == null)
            {
                return this.NotFound();
            }

            return this.File(bytes, "image/png");
        }
    }
}
