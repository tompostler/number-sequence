using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Controllers
{
    [ApiController, Route("DailySequenceValueConfigs"), Route("DSVCs"), RequiresToken]
    public sealed class DailySequenceValueConfigController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<DailySequenceValueConfigController> logger;

        public DailySequenceValueConfigController(
            IServiceProvider serviceProvider,
            ILogger<DailySequenceValueConfigController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] DailySequenceValueConfig dsvc, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Check if the dsvc already exists.
            if (await nsContext.DailySequenceValueConfigs.AnyAsync(x => x.Account == this.User.Identity.Name && x.Category == dsvc.Category.ToLower(), cancellationToken))
            {
                return this.Conflict($"DSVC with category [{dsvc.Category}] already exists.");
            }

            // Check if we shouldn't make another dsvc for this account or category.
            Account account = await nsContext.Accounts.SingleAsync(x => x.Name == this.User.Identity.Name, cancellationToken);
            int countCategoriesForAccount = await nsContext.DailySequenceValueConfigs.Where(x => x.Account == account.Name).CountAsync(cancellationToken);
            if (countCategoriesForAccount >= TierLimits.DailySequenceValueCategoriesPerAccount[account.Tier])
            {
                return this.Conflict($"Too many DSVC categories already created for account with name [{account.Name}].");
            }

            // Validate it is well-formed.
            string errorMessage = dsvc.Validate();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return this.BadRequest(errorMessage);
            }

            DailySequenceValueConfig toInsert = new()
            {
                Account = account.Name,
                Category = dsvc.Category.ToLower(),
                NegativeDeltaMax = dsvc.NegativeDeltaMax,
                NegativeDeltaMin = dsvc.NegativeDeltaMin,
                PositiveDeltaMax = dsvc.PositiveDeltaMax,
                PositiveDeltaMin = dsvc.PositiveDeltaMin,
            };

            _ = nsContext.DailySequenceValueConfigs.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            DailySequenceValueConfig created = await nsContext.DailySequenceValueConfigs.SingleAsync(x => x.Account == account.Name && x.Category == dsvc.Category.ToLower(), cancellationToken);
            this.logger.LogInformation($"Created DSVC: {created.ToJsonString()}");
            return this.Ok(created);
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<DailySequenceValueConfig> dsvcs = await nsContext.DailySequenceValueConfigs
                                                    .Where(x => x.Account == this.User.Identity.Name)
                                                    .OrderBy(x => x.Category)
                                                    .ToListAsync(cancellationToken);
            return this.Ok(dsvcs);
        }

        [HttpGet("{category}")]
        public async Task<IActionResult> GetAsync(string category, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DailySequenceValueConfig dsvc = await nsContext.DailySequenceValueConfigs
                                                .Where(x => x.Account == this.User.Identity.Name && x.Category == category.ToLower())
                                                .SingleOrDefaultAsync(cancellationToken);
            return dsvc == default
                ? this.NotFound()
                : this.Ok(dsvc);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] DailySequenceValueConfig dsvc, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Validate it is well-formed.
            if (dsvc.Validate() is string validationError)
            {
                return this.BadRequest(validationError);
            }

            DailySequenceValueConfig dsvcFound = await nsContext.DailySequenceValueConfigs.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Category == dsvc.Category.ToLower(), cancellationToken);
            if (dsvcFound == default)
            {
                return this.NotFound($"DSVC with category [{dsvc.Category}] does not exist.");
            }

            dsvcFound.NegativeDeltaMax = dsvc.NegativeDeltaMax;
            dsvcFound.NegativeDeltaMin = dsvc.NegativeDeltaMin;
            dsvcFound.PositiveDeltaMax = dsvc.PositiveDeltaMax;
            dsvcFound.PositiveDeltaMin = dsvc.PositiveDeltaMin;
            dsvcFound.ModifiedDate = DateTimeOffset.UtcNow;

            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(dsvcFound);
        }

        [HttpDelete("{category}")]
        public async Task<IActionResult> DeleteAsync(string category, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DailySequenceValueConfig dsvc = await nsContext.DailySequenceValueConfigs.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Category == category.ToLower(), cancellationToken);
            if (dsvc == default)
            {
                return this.NotFound($"DSVC with category [{dsvc.Category}] does not exist.");
            }

            _ = nsContext.DailySequenceValueConfigs.Remove(dsvc);
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            this.logger.LogInformation($"Deleted DSVC: {dsvc.ToJsonString()}");

            return this.Ok(dsvc);
        }
    }
}
