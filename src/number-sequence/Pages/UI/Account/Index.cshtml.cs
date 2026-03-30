using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Account
{
    [RequiresToken]
    public sealed class IndexModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;
        private readonly NsContext nsContext;

        public IndexModel(NsTcpWtfClient nsClient, NsContext nsContext)
        {
            this.nsClient = nsClient;
            this.nsContext = nsContext;
        }

        public TcpWtf.NumberSequence.Contracts.Account Account { get; private set; }
        public List<Token> Tokens { get; private set; }

        [FromQuery]
        public string Error { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            this.Account = await this.nsClient.Account.GetAsync(this.User.Identity.Name, cancellationToken);
            this.Tokens = await this.nsContext.Tokens
                .Where(x => x.Account == this.User.Identity.Name)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
            foreach (Token token in this.Tokens)
            {
                token.Key = null;
                token.Value = null;
            }
            return this.Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string name, CancellationToken cancellationToken)
        {
            try
            {
                _ = await this.nsClient.Token.DeleteAsync(name, cancellationToken);
            }
            catch (NsTcpWtfClientException ex)
            {
                return this.Redirect($"/ui/account?error={Uri.EscapeDataString(ex.Message)}");
            }
            return this.Redirect("/ui/account");
        }
    }
}
