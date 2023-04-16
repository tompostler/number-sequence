using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using number_sequence.DataAccess;
using number_sequence.Filters;
using System;
using System.Threading;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]"), RequiresToken]
    public sealed class CountsController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<CountsController> logger;

        public CountsController(
            IServiceProvider serviceProvider,
            ILogger<CountsController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] Count count, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Check if the count already exists
            if (await nsContext.Counts.AnyAsync(x => x.Account == this.User.Identity.Name && x.Name == count.Name.ToLower(), cancellationToken))
            {
                return this.Conflict($"Count with name [{count.Name}] already exists.");
            }

            // Check if we shouldn't make another count for this account
            Account account = await nsContext.Accounts.SingleAsync(x => x.Name == this.User.Identity.Name, cancellationToken);
            int countCountForAccount = await nsContext.Counts.CountAsync(x => x.Account == account.Name, cancellationToken);
            if (countCountForAccount >= TierLimits.CountsPerAccount[account.Tier])
            {
                return this.Conflict($"Too many counts already created for account with name [{account.Name}].");
            }

            Count toInsert = new()
            {
                Account = account.Name,
                Name = count.Name.ToLower(),
                Value = count.Value
            };

            _ = nsContext.Counts.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            Count createdCount = await nsContext.Counts.SingleAsync(x => x.Account == account.Name && x.Name == toInsert.Name, cancellationToken);
            this.logger.LogInformation($"Created count: {createdCount.ToJsonString()}");
            return this.Ok(createdCount);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetAsync(string name, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Count count = await nsContext.Counts.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Name == name, cancellationToken);
            return count == default
                ? (IActionResult)this.NotFound()
                : this.Request.Query.ContainsKey("bare")
                    ? this.Ok(count.Value)
                    : this.Ok(count);
        }

        [HttpPut("{name}")]
        public async Task<IActionResult> IncrementAsync(string name, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Count count = await nsContext.Counts.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Name == name, cancellationToken);
            if (count == default)
            {
                return this.NotFound($"Count with name [{name}] does not exist.");
            }

            count.Value++;
            count.ModifiedDate = DateTimeOffset.UtcNow;

            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Request.Query.ContainsKey("bare")
                ? this.Ok(count.Value)
                : this.Ok(count);
        }

        [HttpPut("{name}/{amount}")]
        public async Task<IActionResult> IncrementByAmountAsync(string name, ulong amount, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Count count = await nsContext.Counts.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Name == name, cancellationToken);
            if (count == default)
            {
                return this.NotFound($"Count with name [{name}] does not exist.");
            }

            count.Value += amount;
            count.ModifiedDate = DateTimeOffset.UtcNow;

            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Request.Query.ContainsKey("bare")
                ? this.Ok(count.Value)
                : this.Ok(count);
        }
    }
}
