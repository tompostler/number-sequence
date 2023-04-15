using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using number_sequence.DataAccess;
using number_sequence.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class AccountsController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<AccountsController> logger;

        public AccountsController(
            IServiceProvider serviceProvider,
            ILogger<AccountsController> logger)
        {
            this.serviceProvider = serviceProvider;
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
    }
}
