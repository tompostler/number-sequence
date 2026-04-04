using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Businesses
{
    [RequiresToken]
    public sealed class LogoModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public LogoModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public async Task<IActionResult> OnGetAsync(long id, CancellationToken cancellationToken)
        {
            BusinessLogo logo;
            try
            {
                logo = await this.nsClient.Ledger.GetBusinessLogoAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }

            return this.File(logo.Data, logo.ContentType);
        }
    }
}
