using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Admin
{
    [RequiresToken(AccountRoles.Admin)]
    public sealed class AccountsModel : PageModel
    {
        private readonly NsTcpWtfClient nsClient;

        public AccountsModel(NsTcpWtfClient nsClient)
        {
            this.nsClient = nsClient;
        }

        public List<TcpWtf.NumberSequence.Contracts.Account> Accounts { get; private set; }

        [FromQuery]
        public string Error { get; set; }

        [FromQuery]
        public string Updated { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            this.Accounts = await this.nsClient.Account.ListAsync(cancellationToken);
            return this.Page();
        }

        /// <summary>
        /// The row posts the whole desired state; the api is per-change, so diff against the current account and send only what moved.
        /// </summary>
        public async Task<IActionResult> OnPostUpdateAsync(string name, AccountTier tier, List<string> roles, CancellationToken cancellationToken)
        {
            try
            {
                TcpWtf.NumberSequence.Contracts.Account current = await this.nsClient.Account.GetAsync(name, cancellationToken);
                List<string> currentRoles = AccountRoles.Parse(current.Roles);
                roles ??= [];

                if (current.Tier != tier)
                {
                    _ = await this.nsClient.Account.UpdateTierAsync(name, tier, cancellationToken);
                }
                foreach (string role in roles.Except(currentRoles, StringComparer.OrdinalIgnoreCase))
                {
                    _ = await this.nsClient.Account.AddRoleAsync(name, role, cancellationToken);
                }
                foreach (string role in currentRoles.Except(roles, StringComparer.OrdinalIgnoreCase))
                {
                    _ = await this.nsClient.Account.RemoveRoleAsync(name, role, cancellationToken);
                }
            }
            catch (NsTcpWtfClientException ex)
            {
                return this.Redirect($"/ui/admin/accounts?error={Uri.EscapeDataString(ex.Message)}");
            }
            return this.Redirect($"/ui/admin/accounts?updated={Uri.EscapeDataString(name)}");
        }
    }
}
