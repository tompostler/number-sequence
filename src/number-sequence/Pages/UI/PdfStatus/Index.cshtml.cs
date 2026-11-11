using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.PdfStatus
{
    [RequiresToken(AccountRoles.Admin, AccountRoles.Chiro, AccountRoles.Ledger)]
    public sealed class IndexModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public IndexModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public TcpWtf.NumberSequence.Contracts.PdfStatus Status { get; private set; }

        // A section the viewer's roles don't reach isn't rendered at all. These mirror the per-section checks in
        // PdfStatusController, which leaves those lists empty.
        public bool ShowChiro => this.User.IsInRole(AccountRoles.Admin) || this.User.IsInRole(AccountRoles.Chiro);
        public bool ShowLedger => this.User.IsInRole(AccountRoles.Admin) || this.User.IsInRole(AccountRoles.Ledger);
        public bool ShowUnclassified => this.User.IsInRole(AccountRoles.Admin) && this.Status.UnclassifiedEmailDocuments.Count > 0;

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
