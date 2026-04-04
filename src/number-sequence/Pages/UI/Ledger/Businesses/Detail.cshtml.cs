using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Pages.UI.Ledger.Businesses
{
    [RequiresToken]
    public sealed class DetailModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public DetailModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public Business Business { get; private set; }
        public bool HasLogo { get; private set; }
        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                this.Business = await this.nsClient.Ledger.GetBusinessAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }

            try
            {
                _ = await this.nsClient.Ledger.GetBusinessLogoAsync(id, cancellationToken);
                this.HasLogo = true;
            }
            catch (NsTcpWtfClientException) { }

            return this.Page();
        }

        public async Task<IActionResult> OnPostLogoAsync(long id, IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                this.ErrorMessage = "No file selected.";
                await this.LoadBusinessAsync(id, cancellationToken);
                return this.Page();
            }

            byte[] data;
            using (MemoryStream ms = new())
            {
                await file.CopyToAsync(ms, cancellationToken);
                data = ms.ToArray();
            }

            BusinessLogo logo = new() { ContentType = file.ContentType, Data = data };

            try
            {
                if (this.HasLogo)
                {
                    _ = await this.nsClient.Ledger.UpdateBusinessLogoAsync(id, logo, cancellationToken);
                }
                else
                {
                    _ = await this.nsClient.Ledger.CreateBusinessLogoAsync(id, logo, cancellationToken);
                }
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadBusinessAsync(id, cancellationToken);
                return this.Page();
            }

            return this.Redirect($"/ui/ledger/businesses/{id}");
        }

        public async Task<IActionResult> OnPostDeleteLogoAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                await this.nsClient.Ledger.DeleteBusinessLogoAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadBusinessAsync(id, cancellationToken);
                return this.Page();
            }

            return this.Redirect($"/ui/ledger/businesses/{id}");
        }

        private async Task LoadBusinessAsync(long id, CancellationToken cancellationToken)
        {
            try
            {
                this.Business = await this.nsClient.Ledger.GetBusinessAsync(id, cancellationToken);
                _ = await this.nsClient.Ledger.GetBusinessLogoAsync(id, cancellationToken);
                this.HasLogo = true;
            }
            catch (NsTcpWtfClientException) { }
        }
    }
}
