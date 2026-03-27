using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;

namespace number_sequence.Pages.UI.DaysSince
{
    public sealed class DetailModel : PageModel
    {
        private readonly NsContext nsContext;

        public DetailModel(NsContext nsContext)
        {
            this.nsContext = nsContext;
        }

        public TcpWtf.NumberSequence.Contracts.DaysSince DaysSince { get; private set; }
        public int DayCount { get; private set; }

        public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken)
        {
            id = id.ToLowerInvariant();
            this.DaysSince = await this.nsContext.DaysSinces
                .Include(x => x.Events)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (this.DaysSince == default)
            {
                return this.NotFound();
            }

            this.DaysSince.Value = string.Join(' ', this.DaysSince.ValueLine1, this.DaysSince.ValueLine2, this.DaysSince.ValueLine3, this.DaysSince.ValueLine4).TrimEnd();
            this.DayCount = (int)(DateTime.UtcNow - this.DaysSince.LastOccurrence.ToDateTime(new(), DateTimeKind.Utc)).TotalDays;

            this.Response.Headers.CacheControl = "public, max-age=3600";
            return this.Page();
        }
    }
}
