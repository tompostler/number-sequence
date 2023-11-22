using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("r")]
    public sealed class RedirectsController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<RedirectsController> logger;

        public RedirectsController(
            IServiceProvider serviceProvider,
            ILogger<RedirectsController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            id = id.ToLowerInvariant();
            Redirect redirect = await nsContext.Redirects.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (redirect == default)
            {
                return this.NotFound($"Short url [{id}] not found.");
            }
            else if (redirect.Expiration.HasValue && redirect.Expiration < DateTimeOffset.UtcNow)
            {
                return this.BadRequest($"Short url [{id}] expired.");
            }

            redirect.Hits++;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Redirect(redirect.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] Redirect redirect, CancellationToken cancellationToken)
        {
            redirect.Id = redirect.Id.ToLowerInvariant();

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Check if the redirect already exists.
            if (await nsContext.Redirects.AnyAsync(x => x.AccountName == this.User.Identity.Name && x.Id == redirect.Id, cancellationToken))
            {
                return this.Conflict($"Redirect with name [{redirect.Id}] already exists.");
            }

            // Check if we shouldn't make another redirect for this account.
            Account account = await nsContext.Accounts.SingleAsync(x => x.Name == this.User.Identity.Name, cancellationToken);
            int countRedirectForAccount = await nsContext.Redirects.CountAsync(x => x.AccountName == account.Name, cancellationToken);
            if (countRedirectForAccount >= TierLimits.RedirectsPerAccount[account.Tier])
            {
                return this.Conflict($"Too many redirects already created for account with name [{account.Name}].");
            }

            Redirect toInsert = new()
            {
                AccountName = account.Name,
                Id = redirect.Id,
                Value = redirect.Value,
                Expiration = redirect.Expiration,
            };

            _ = nsContext.Redirects.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            Redirect created = await nsContext.Redirects.SingleAsync(x => x.AccountName == account.Name && x.Id == toInsert.Id, cancellationToken);
            this.logger.LogInformation($"Created redirect: {created.ToJsonString()}");
            return this.Ok(created);
        }

        [HttpGet, RequiresToken]
        public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<Count> counts = await nsContext.Counts.Where(x => x.Account == this.User.Identity.Name).ToListAsync(cancellationToken);
            return this.Ok(counts);
        }
    }
}
