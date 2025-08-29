using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Controllers
{
    [ApiController, Route("DailySequenceValues"), Route("DSVs"), RequiresToken]
    public sealed class DailySequenceValueController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<DailySequenceValueController> logger;

        public DailySequenceValueController(
            IServiceProvider serviceProvider,
            ILogger<DailySequenceValueController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] DailySequenceValue dsv, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Check if the dsv already exists.
            if (await nsContext.DailySequenceValues.AnyAsync(x => x.Account == this.User.Identity.Name && x.Category == dsv.Category.ToLower() && x.EventDate == dsv.EventDate, cancellationToken))
            {
                return this.Conflict($"DSV with category [{dsv.Category}] and date [{dsv.EventDate}] already exists.");
            }

            // Check if we shouldn't make another dsv for this account or category.
            Account account = await nsContext.Accounts.SingleAsync(x => x.Name == this.User.Identity.Name, cancellationToken);
            int countCategoriesForAccount = await nsContext.DailySequenceValues.Where(x => x.Account == account.Name).GroupBy(x => x.Category).CountAsync(cancellationToken);
            if (countCategoriesForAccount >= TierLimits.DailySequenceValueCategoriesPerAccount[account.Tier])
            {
                return this.Conflict($"Too many DSV categories already created for account with name [{account.Name}].");
            }
            int countForCategoryForAccount = await nsContext.DailySequenceValues.CountAsync(x => x.Account == account.Name && x.Category == dsv.Category.ToLower(), cancellationToken);
            if (countForCategoryForAccount >= TierLimits.DailySequenceValuesPerCategory[account.Tier])
            {
                return this.Conflict($"Too many DSVs already created for account with name [{account.Name}] in category [{dsv.Category.ToLower()}].");
            }

            DailySequenceValue toInsert = new()
            {
                Account = account.Name,
                Category = dsv.Category.ToLower(),
                EventDate = dsv.EventDate,
                Value = dsv.Value
            };

            // Check if there is a config for this category. If so, adjust the value if necessary.
            DailySequenceValueConfig dsvc = await nsContext.DailySequenceValueConfigs.SingleOrDefaultAsync(x => x.Account == account.Name && x.Category == dsv.Category.ToLower(), cancellationToken);
            if (dsvc != default)
            {
                DailySequenceValue previousDsv = await nsContext.DailySequenceValues
                                                                .Where(x => x.Account == account.Name && x.Category == dsv.Category.ToLower() && x.EventDate < dsv.EventDate)
                                                                .OrderByDescending(x => x.EventDate)
                                                                .FirstOrDefaultAsync(cancellationToken);
                if (previousDsv != default)
                {
                    decimal delta = dsv.Value - previousDsv.Value;
                    this.logger.LogInformation($"Applying DSVC {dsvc.ToJsonString()} to DSV {dsv.ToJsonString()} with previous DSV {previousDsv.ToJsonString()} and delta {delta}.");
                    if (delta < 0)
                    {
                        // Negative delta.
                        if (dsvc.NegativeDeltaMin.HasValue && delta > dsvc.NegativeDeltaMin.Value)
                        {
                            toInsert.OriginalValue = toInsert.Value;
                            toInsert.Value = previousDsv.Value + dsvc.NegativeDeltaMin.Value;
                        }
                        else if (dsvc.NegativeDeltaMax.HasValue && delta < dsvc.NegativeDeltaMax.Value)
                        {
                            toInsert.OriginalValue = toInsert.Value;
                            toInsert.Value = previousDsv.Value + dsvc.NegativeDeltaMax.Value;
                        }
                    }
                    else if (delta > 0)
                    {
                        // Positive delta.
                        if (dsvc.PositiveDeltaMin.HasValue && delta < dsvc.PositiveDeltaMin.Value)
                        {
                            toInsert.OriginalValue = toInsert.Value;
                            toInsert.Value = previousDsv.Value + dsvc.PositiveDeltaMin.Value;
                        }
                        else if (dsvc.PositiveDeltaMax.HasValue && delta > dsvc.PositiveDeltaMax.Value)
                        {
                            toInsert.OriginalValue = toInsert.Value;
                            toInsert.Value = previousDsv.Value + dsvc.PositiveDeltaMax.Value;
                        }
                    }
                    else
                    {
                        // No change.
                    }
                }
            }

            _ = nsContext.DailySequenceValues.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            DailySequenceValue created = await nsContext.DailySequenceValues.SingleAsync(x => x.Account == account.Name && x.Category == dsv.Category.ToLower() && x.EventDate == dsv.EventDate, cancellationToken);
            this.logger.LogInformation($"Created DSV: {created.ToJsonString()}");
            return this.Ok(created);
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync(
            CancellationToken cancellationToken,
            [FromQuery] int takeAmount = 20,
            [FromQuery] int daysLookback = 30)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            
            DateOnly daysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-daysLookback));

            List<DailySequenceValue> dsvs = await nsContext.DailySequenceValues
                                                .Where(x => x.Account == this.User.Identity.Name && x.EventDate > daysAgo)
                                                .OrderBy(x => x.Category)
                                                .ThenByDescending(x => x.EventDate)
                                                .Take(takeAmount)
                                                .ToListAsync(cancellationToken);
            return this.Ok(dsvs);
        }

        [HttpGet("{category}")]
        public async Task<IActionResult> GetCategoryAsync(
            string category,
            CancellationToken cancellationToken,
            [FromQuery] int takeAmount = 20,
            [FromQuery] int daysLookback = 30)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            
            DateOnly daysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-daysLookback));

            List<DailySequenceValue> dsvs = await nsContext.DailySequenceValues
                                                .Where(x => x.Account == this.User.Identity.Name && x.Category == category.ToLower() && x.EventDate > daysAgo)
                                                .OrderByDescending(x => x.EventDate)
                                                .Take(takeAmount)
                                                .ToListAsync(cancellationToken);
            return this.Ok(dsvs);
        }

        [HttpGet("{category}/{date}")]
        public async Task<IActionResult> GetAsync(string category, DateOnly date, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DailySequenceValue dsv = await nsContext.DailySequenceValues.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Category == category.ToLower() && x.EventDate == date, cancellationToken);
            if (dsv == default)
            {
                return this.NotFound();
            }

            return this.Ok(dsv);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] DailySequenceValue dsv, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DailySequenceValue dsvFound = await nsContext.DailySequenceValues.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Category == dsv.Category.ToLower() && x.EventDate == dsv.EventDate, cancellationToken);
            if (dsvFound == default)
            {
                return this.NotFound($"DSV with category [{dsv.Category}] and date [{dsv.EventDate}] does not exist.");
            }

            dsvFound.Value = dsv.Value;
            dsvFound.ModifiedDate = DateTimeOffset.UtcNow;

            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(dsvFound);
        }

        [HttpDelete("{category}/{date}")]
        public async Task<IActionResult> DeleteAsync(string category, DateOnly date, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DailySequenceValue dsv = await nsContext.DailySequenceValues.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Category == category.ToLower() && x.EventDate == date, cancellationToken);
            if (dsv == default)
            {
                return this.NotFound($"DSV with category [{category}] and date [{date}] does not exist.");
            }

            _ = nsContext.DailySequenceValues.Remove(dsv);
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            this.logger.LogInformation($"Deleted DSV: {dsv.ToJsonString()}");

            return this.Ok(dsv);
        }
    }
}
