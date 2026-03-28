using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;

namespace number_sequence.Pages.UI
{
    public sealed class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            this.Response.Cookies.Delete(RequiresTokenFilter.TokenCookieName);
            return this.Redirect("/ui/login");
        }
    }
}
