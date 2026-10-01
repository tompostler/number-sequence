using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using number_sequence.DataAccess;
using number_sequence.Extensions;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class AccountsController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<AccountsController> logger;

        public AccountsController(
            IServiceProvider serviceProvider,
            IMemoryCache memoryCache,
            ILogger<AccountsController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetAsync(string name, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Account account = await nsContext.Accounts.FirstOrDefaultAsync(x => x.Name == name.ToLower(), cancellationToken);

            if (account == default)
            {
                return this.NotFound();
            }
            else
            {
                account.CreatedFrom = default;
                account.Key = default;
                return this.Ok(account);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] Account account, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Check if the account already exists
            if (await nsContext.Accounts.AnyAsync(x => x.Name == account.Name.ToLower(), cancellationToken))
            {
                return this.Conflict($"Account with name [{account.Name}] already exists.");
            }

            // Check if we shouldn't make another account for this CreatedFrom
            string createdFrom = this.Request.GetClientIPAddress()?.ToLower();
            int accountCountForCreatedFrom = await nsContext.Accounts.CountAsync(x => x.CreatedFrom == createdFrom, cancellationToken);
            List<AccountTier> tiersForCreatedFrom = await nsContext.Accounts.Where(x => x.CreatedFrom == createdFrom).Select(x => x.Tier).Distinct().ToListAsync(cancellationToken);
            tiersForCreatedFrom.Sort();
            AccountTier smallestAppliedTier = tiersForCreatedFrom.FirstOrDefault();
            if (accountCountForCreatedFrom >= TierLimits.AccountsPerCreatedFrom[smallestAppliedTier])
            {
                return this.Conflict($"Too many accounts already created from [{account.CreatedFrom}].");
            }

            Account toInsert = new()
            {
                CreatedFrom = createdFrom,
                Key = account.Key.ComputeSHA256(),
                Name = account.Name?.ToLower(),
                Tier = AccountTier.Small
            };

            _ = nsContext.Accounts.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            Account createdAccount = await nsContext.Accounts.SingleAsync(x => x.Name == toInsert.Name, cancellationToken);
            createdAccount.Key = default;
            this.logger.LogInformation($"Created account: {createdAccount.ToJsonString()}");
            return this.Ok(createdAccount);
        }

        [HttpGet, RequiresToken(AccountRoles.Admin)]
        public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<Account> accounts = await nsContext.Accounts.OrderBy(x => x.Name).ToListAsync(cancellationToken);
            foreach (Account account in accounts)
            {
                account.Key = default;
            }
            return this.Ok(accounts);
        }

        [HttpPut("{name}/tier/{tier}"), RequiresToken(AccountRoles.Admin)]
        public Task<IActionResult> UpdateTierAsync(string name, AccountTier tier, CancellationToken cancellationToken)
            => this.UpdateAccountAsync(
                name,
                account =>
                {
                    this.logger.LogInformation($"{this.User.Identity.Name} changing tier of {account.Name} from {account.Tier} to {tier}.");
                    account.Tier = tier;
                    return null;
                },
                cancellationToken);

        [HttpPut("{name}/roles/{role}"), RequiresToken(AccountRoles.Admin)]
        public Task<IActionResult> AddRoleAsync(string name, string role, CancellationToken cancellationToken)
            => this.UpdateAccountAsync(
                name,
                account =>
                {
                    // Only known roles can be granted, and they're stored in the constant's casing.
                    string knownRole = AccountRoles.All.FirstOrDefault(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase));
                    if (knownRole == default)
                    {
                        return this.BadRequest($"Role [{role}] is not one of: {string.Join(", ", AccountRoles.All)}.");
                    }

                    List<string> roles = AccountRoles.Parse(account.Roles);
                    if (!roles.Contains(knownRole, StringComparer.OrdinalIgnoreCase))
                    {
                        roles.Add(knownRole);
                    }
                    this.SetRoles(account, roles);
                    return null;
                },
                cancellationToken);

        [HttpDelete("{name}/roles/{role}"), RequiresToken(AccountRoles.Admin)]
        public Task<IActionResult> RemoveRoleAsync(string name, string role, CancellationToken cancellationToken)
            => this.UpdateAccountAsync(
                name,
                account =>
                {
                    // An admin removing their own admin role could leave nobody able to undo it without sql.
                    if (string.Equals(account.Name, this.User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(role, AccountRoles.Admin, StringComparison.OrdinalIgnoreCase))
                    {
                        return this.BadRequest("You cannot remove your own admin role.");
                    }

                    // Unknown roles are allowed here so stale values written by hand can be cleaned up.
                    List<string> roles = AccountRoles.Parse(account.Roles);
                    _ = roles.RemoveAll(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase));
                    this.SetRoles(account, roles);
                    return null;
                },
                cancellationToken);

        /// <summary>
        /// Loads the account, applies <paramref name="update"/> (which returns an error result or null), saves, and evicts the
        /// account's cached token principals so the change applies on the next request instead of whenever the cache ages out.
        /// </summary>
        private async Task<IActionResult> UpdateAccountAsync(string name, Func<Account, IActionResult> update, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            name = name.ToLower();
            Account account = await nsContext.Accounts.SingleOrDefaultAsync(x => x.Name == name, cancellationToken);
            if (account == default)
            {
                return this.NotFound($"Account [{name}] not found.");
            }

            IActionResult error = update(account);
            if (error != default)
            {
                return error;
            }

            account.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            List<string> tokenValues = await nsContext.Tokens.Where(x => x.Account == name).Select(x => x.Value).ToListAsync(cancellationToken);
            foreach (string tokenValue in tokenValues)
            {
                this.memoryCache.Remove(tokenValue);
            }
            this.logger.LogInformation($"Updated account {name} (tier {account.Tier}, roles [{account.Roles}]) and evicted {tokenValues.Count} cached tokens.");

            account.Key = default;
            return this.Ok(account);
        }

        private void SetRoles(Account account, List<string> roles)
        {
            string newRoles = roles.Count == 0 ? null : string.Join(';', roles.OrderBy(x => x));
            this.logger.LogInformation($"{this.User.Identity.Name} changing roles of {account.Name} from [{account.Roles}] to [{newRoles}].");
            account.Roles = newRoles;
        }
    }
}
