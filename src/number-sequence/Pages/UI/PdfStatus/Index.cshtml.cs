using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.PdfStatus
{
    [RequiresToken(AccountRoles.Chiro, AccountRoles.PdfStatus)]
    public sealed class IndexModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public IndexModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public TcpWtf.NumberSequence.Contracts.PdfStatus Status { get; private set; }

        [FromQuery]
        public double HoursOffset { get; set; } = 0;

        [FromQuery]
        public int TakeAmount { get; set; } = 20;

        [FromQuery]
        public int DaysLookback { get; set; } = 30;

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            this.Status = await this.nsClient.PdfStatus.GetAsync(
                hoursOffset: this.HoursOffset,
                takeAmount: this.TakeAmount,
                daysLookback: this.DaysLookback,
                cancellationToken: cancellationToken);
            return this.Page();
        }
    }
}
