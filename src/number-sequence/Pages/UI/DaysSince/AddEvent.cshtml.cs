using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.DaysSince
{
    [RequiresToken]
    public sealed class AddEventModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public AddEventModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public TcpWtf.NumberSequence.Contracts.DaysSince DaysSince { get; private set; }

        [BindProperty]
        public DaysSinceEvent Event { get; set; }

        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken)
        {
            TcpWtf.NumberSequence.Contracts.DaysSince daysSince;
            try
            {
                daysSince = await this.nsClient.DaysSince.GetAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }

            if (daysSince.AccountName != this.User.Identity.Name)
            {
                return this.NotFound();
            }

            this.DaysSince = daysSince;
            this.Event = new DaysSinceEvent { EventDate = DateOnly.FromDateTime(DateTime.UtcNow) };
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(string id, CancellationToken cancellationToken)
        {
            TcpWtf.NumberSequence.Contracts.DaysSince daysSince;
            try
            {
                daysSince = await this.nsClient.DaysSince.GetAsync(id, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }

            if (daysSince.AccountName != this.User.Identity.Name)
            {
                return this.NotFound();
            }

            this.DaysSince = daysSince;

            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            // The GET response sets Value to the joined ValueLines. The controller rejects having
            // both Value and ValueLine1 set, so clear Value and let the controller use ValueLine1-4.
            daysSince.Value = null;
            daysSince.Events ??= [];
            daysSince.Events.Add(this.Event);

            try
            {
                _ = await this.nsClient.DaysSince.UpdateAsync(daysSince, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }

            return this.Redirect($"/ui/days-since/{id}");
        }
    }
}
