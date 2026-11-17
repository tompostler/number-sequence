using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Counts
{
    [RequiresToken]
    public sealed class DetailModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public DetailModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public Count Count { get; private set; }
        public List<CountEvent> Events { get; private set; }

        /// <summary>
        /// The recorded-time column only appears when at least one listed event was back-dated; otherwise it would be empty.
        /// </summary>
        public bool ShowRecorded => this.Events?.Any(IsBackdated) == true;

        /// <summary>
        /// Recorded more than a minute after it happened, so the recorded time is worth showing.
        /// </summary>
        public static bool IsBackdated(CountEvent countEvent) => countEvent.CreatedDate - countEvent.EventDate > TimeSpan.FromMinutes(1);

        [BindProperty]
        public ulong IncrementAmount { get; set; } = 1;

        /// <summary>
        /// Optional earlier time for the increment, posted as UTC by the page's script from the browser's local time.
        /// </summary>
        [BindProperty]
        public DateTimeOffset? At { get; set; }

        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync(string name, CancellationToken cancellationToken)
        {
            try
            {
                this.Count = await this.nsClient.Count.GetAsync(name, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }

            this.Events = await this.nsClient.Count.GetEventsAsync(name, take: 100, cancellationToken: cancellationToken);
            return this.Page();
        }

        public async Task<IActionResult> OnPostIncrementAsync(string name, CancellationToken cancellationToken)
        {
            try
            {
                _ = await this.nsClient.Count.IncrementAsync(name, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadCountAndEventsAsync(name, cancellationToken);
                return this.Page();
            }

            return this.Redirect($"/ui/counts/{name}");
        }

        public async Task<IActionResult> OnPostSetOverflowAsync(string name, bool overflow, CancellationToken cancellationToken)
        {
            try
            {
                _ = await this.nsClient.Count.UpdateOverflowDropsOldestEventsAsync(name, overflow, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadCountAndEventsAsync(name, cancellationToken);
                return this.Page();
            }

            return this.Redirect($"/ui/counts/{name}");
        }

        public async Task<IActionResult> OnPostIncrementByAsync(string name, CancellationToken cancellationToken)
        {
            try
            {
                _ = await this.nsClient.Count.IncrementByAmountAsync(name, this.IncrementAmount, this.At, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                await this.LoadCountAndEventsAsync(name, cancellationToken);
                return this.Page();
            }

            return this.Redirect($"/ui/counts/{name}");
        }

        private async Task LoadCountAndEventsAsync(string name, CancellationToken cancellationToken)
        {
            try
            {
                this.Count = await this.nsClient.Count.GetAsync(name, cancellationToken);
                this.Events = await this.nsClient.Count.GetEventsAsync(name, take: 100, cancellationToken: cancellationToken);
            }
            catch (NsTcpWtfClientException) { }
        }
    }
}
