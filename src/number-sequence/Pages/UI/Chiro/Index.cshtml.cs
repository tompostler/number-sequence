using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Chiro
{
    [RequiresToken(AccountRoles.Chiro)]
    public sealed class IndexModel : PageModel
    {
        public string SubmittedSpecies { get; private set; }
        public string SubmittedRowId { get; private set; }

        public void OnGet(string submitted, string rowId)
        {
            this.SubmittedSpecies = submitted;
            this.SubmittedRowId = rowId;
        }
    }
}
