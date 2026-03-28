using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.DaysSince
{
    [RequiresToken]
    public sealed class AddEventModel : PageModel
    {
        private readonly NsContext nsContext;

        public AddEventModel(NsContext nsContext)
        {
            this.nsContext = nsContext;
        }

        public TcpWtf.NumberSequence.Contracts.DaysSince DaysSince { get; private set; }

        [BindProperty]
        public DaysSinceEvent Event { get; set; }

        public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken)
        {
            this.DaysSince = await this.nsContext.DaysSinces
                .SingleOrDefaultAsync(x => x.Id == id && x.AccountName == this.User.Identity.Name, cancellationToken);

            if (this.DaysSince == default)
            {
                return this.NotFound();
            }

            this.Event = new DaysSinceEvent { EventDate = DateOnly.FromDateTime(DateTime.UtcNow) };
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(string id, CancellationToken cancellationToken)
        {
            this.DaysSince = await this.nsContext.DaysSinces
                .Include(x => x.Events)
                .SingleOrDefaultAsync(x => x.Id == id && x.AccountName == this.User.Identity.Name, cancellationToken);

            if (this.DaysSince == default)
            {
                return this.NotFound();
            }

            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            this.DaysSince.Events.Add(this.Event);
            this.DaysSince.LastOccurrence = this.DaysSince.Events.Max(x => x.EventDate);
            this.DaysSince.ModifiedDate = DateTimeOffset.UtcNow;

            _ = await this.nsContext.SaveChangesAsync(cancellationToken);

            return this.Redirect($"/ui/days-since/{id}");
        }
    }
}
