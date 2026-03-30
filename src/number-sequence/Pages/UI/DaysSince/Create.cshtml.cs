using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;

namespace number_sequence.Pages.UI.DaysSince
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
        public TcpWtf.NumberSequence.Contracts.DaysSince DaysSince { get; set; }

        public string ErrorMessage { get; private set; }

        public IActionResult OnGet()
        {
            this.DaysSince = new() { Id = Guid.NewGuid().ToString().Split('-')[0] };
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            // LastOccurrence is server-set and not in the form; remove from validation to avoid
            // the DateOnlyWithinTenYears custom validation failing on the default value.
            _ = this.ModelState.Remove($"{nameof(this.DaysSince)}.{nameof(this.DaysSince.LastOccurrence)}");

            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            try
            {
                this.DaysSince = await this.nsClient.DaysSince.CreateAsync(this.DaysSince, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }

            return this.Redirect($"/ui/days-since/{this.DaysSince.Id.ToLowerInvariant()}");
        }
    }
}
