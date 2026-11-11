using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.PdfStatus
{
    [RequiresToken(AccountRoles.Admin, AccountRoles.Chiro)]
    public sealed class ChartModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public ChartModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public async Task<IActionResult> OnGetAsync(int daysLookback, double hoursOffset, ChiroChartGrouping groupBy, CancellationToken cancellationToken)
        {
            byte[] bytes = await this.nsClient.PdfStatus.GetChartAsync(
                daysLookback: daysLookback,
                hoursOffset: hoursOffset,
                width: 1440,
                height: 720,
                groupBy: groupBy,
                cancellationToken: cancellationToken);
            if (bytes == null)
            {
                return this.NotFound();
            }

            return this.File(bytes, "image/png");
        }
    }
}
