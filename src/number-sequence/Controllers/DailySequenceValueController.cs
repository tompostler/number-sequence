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
            int countForCategoryForAccount = await nsContext.DailySequenceValues.CountAsync(x=> x.Account == account.Name && x.Category == dsv.Category.ToLower(), cancellationToken);
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

            _ = nsContext.DailySequenceValues.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            DailySequenceValue created = await nsContext.DailySequenceValues.SingleAsync(x => x.Account == account.Name && x.Category == dsv.Category.ToLower() && x.EventDate == dsv.EventDate, cancellationToken);
            this.logger.LogInformation($"Created DSV: {created.ToJsonString()}");
            return this.Ok(created);
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<DailySequenceValue> dsvs = await nsContext.DailySequenceValues
                                                .Where(x => x.Account == this.User.Identity.Name)
                                                .OrderBy(x => x.Category)
                                                .ThenBy(x => x.EventDate)
                                                .ToListAsync(cancellationToken);
            return this.Ok(dsvs);
        }

        [HttpGet("{category}")]
        public async Task<IActionResult> GetCategoryAsync(string category, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<DailySequenceValue> dsvs = await nsContext.DailySequenceValues
                                                .Where(x => x.Account == this.User.Identity.Name && x.Category == category.ToLower())
                                                .OrderBy(x => x.EventDate)
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
            return dsv == default
                ? this.NotFound()
                : this.Ok(dsv);
        }
    }
}
