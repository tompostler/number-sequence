using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;

namespace number_sequence.Pages.UI.DaysSince
{
    [RequiresToken]
    public sealed class IndexModel : PageModel
    {
        private readonly NsContext nsContext;

        public IndexModel(NsContext nsContext)
        {
            this.nsContext = nsContext;
        }

        public List<TcpWtf.NumberSequence.Contracts.DaysSince> DaysSinces { get; private set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            this.DaysSinces = await this.nsContext.DaysSinces
                .Where(x => x.AccountName == this.User.Identity.Name)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);

            return this.Page();
        }
    }
}
