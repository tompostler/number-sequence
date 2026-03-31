using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace number_sequence.Pages.UI.Random
{
    public sealed class IndexModel : PageModel
    {
        public IActionResult OnGet() => this.Page();
    }
}
