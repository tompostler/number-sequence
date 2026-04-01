using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
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
                Value = count.Value,
                OverflowDropsOldestEvents = count.OverflowDropsOldestEvents
            };

            _ = nsContext.Counts.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            Count createdCount = await nsContext.Counts.SingleAsync(x => x.Account == account.Name && x.Name == toInsert.Name, cancellationToken);
            this.logger.LogInformation($"Created count: {createdCount.ToJsonString()}");
            return this.Ok(createdCount);
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<Count> counts = await nsContext.Counts.Where(x => x.Account == this.User.Identity.Name).ToListAsync(cancellationToken);
            return this.Ok(counts);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetAsync(string name, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Count count = await nsContext.Counts.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Name == name, cancellationToken);
            return count == default
                ? (IActionResult)this.NotFound()
                : this.Ok(count);
        }

        [HttpPut("{name}")]
        public async Task<IActionResult> IncrementAsync(string name, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            return await this.IncrementCoreAsync(nsContext, name, 1, cancellationToken);
        }

        [HttpPut("{name}/{amount}")]
        public async Task<IActionResult> IncrementByAmountAsync(string name, ulong amount, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            return await this.IncrementCoreAsync(nsContext, name, amount, cancellationToken);
        }

        private async Task<IActionResult> IncrementCoreAsync(NsContext nsContext, string name, ulong amount, CancellationToken cancellationToken)
        {
            Count count = await nsContext.Counts.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Name == name, cancellationToken);
            if (count == default)
            {
                return this.NotFound($"Count with name [{name}] does not exist.");
            }

            // Check tier limit for events
            Account account = await nsContext.Accounts.SingleAsync(x => x.Name == this.User.Identity.Name, cancellationToken);
            int eventCount = await nsContext.CountEvents.CountAsync(x => x.Account == account.Name && x.CountName == name, cancellationToken);
            int eventLimit = TierLimits.CountEventsPerCount[account.Tier];

            if (eventCount >= eventLimit)
            {
                if (count.OverflowDropsOldestEvents)
                {
                    // Drop oldest events to make room
                    int toRemove = eventCount - eventLimit + 1;
                    List<CountEvent> oldest = await nsContext.CountEvents
                        .Where(x => x.Account == account.Name && x.CountName == name)
                        .OrderBy(x => x.CreatedDate)
                        .Take(toRemove)
                        .ToListAsync(cancellationToken);
                    nsContext.CountEvents.RemoveRange(oldest);
                }
                else
                {
                    return this.Conflict($"Count event history is full ({eventCount}/{eventLimit}). Set {nameof(Count.OverflowDropsOldestEvents)} to true to automatically drop oldest events.");
                }
            }

            count.Value += amount;
            count.ModifiedDate = DateTimeOffset.UtcNow;

            // Record the event
            _ = nsContext.CountEvents.Add(new CountEvent
            {
                Account = account.Name,
                CountName = name,
                Value = count.Value,
                IncrementAmount = amount,
            });

            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(count);
        }

        [HttpGet("{name}/events")]
        public async Task<IActionResult> GetEventsAsync(string name, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            if (from.HasValue && to.HasValue && from.Value >= to.Value)
            {
                return this.BadRequest("The 'from' date must be before the 'to' date.");
            }

            bool exists = await nsContext.Counts.AnyAsync(x => x.Account == this.User.Identity.Name && x.Name == name, cancellationToken);
            if (!exists)
            {
                return this.NotFound($"Count with name [{name}] does not exist.");
            }

            IQueryable<CountEvent> query = nsContext.CountEvents
                .Where(x => x.Account == this.User.Identity.Name && x.CountName == name);
            if (from.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(x => x.CreatedDate <= to.Value);
            }

            List<CountEvent> events = await query
                .OrderBy(x => x.CreatedDate)
                .ToListAsync(cancellationToken);

            return this.Ok(events);
        }

        [HttpGet("{name}/chart")]
        public async Task<IActionResult> GetChartAsync(string name, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] int width = 2560, [FromQuery] int height = 1440, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            bool exists = await nsContext.Counts.AnyAsync(x => x.Account == this.User.Identity.Name && x.Name == name, cancellationToken);
            if (!exists)
            {
                return this.NotFound($"Count with name [{name}] does not exist.");
            }

            IQueryable<CountEvent> query = nsContext.CountEvents
                .Where(x => x.Account == this.User.Identity.Name && x.CountName == name);
            if (from.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(x => x.CreatedDate <= to.Value);
            }

            List<CountEvent> events = await query
                .OrderBy(x => x.CreatedDate)
                .ToListAsync(cancellationToken);

            if (width < 100 || width > 2560 || height < 100 || height > 1440)
            {
                return this.BadRequest($"Width must be 100–2560 and height must be 100–1440.");
            }

            if (events.Count == 0)
            {
                return this.NoContent();
            }

            ScottPlot.Plot plot = new();
            ScottPlot.Plottables.Scatter scatter = plot.Add.Scatter(
                events.Select(x => x.CreatedDate.UtcDateTime).ToList(),
                events.Select(x => x.Value).ToList());
            scatter.LegendText = name;

            _ = plot.Axes.DateTimeTicksBottom();
            _ = plot.HideLegend();
            plot.Axes.TightMargins();
            plot.Axes.SetLimitsY(0, plot.Axes.GetLimits().Top);

            plot.Title($"Count: {name}");
            _ = plot.Add.Annotation($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC", ScottPlot.Alignment.UpperLeft);

            byte[] bytes = plot.GetImage(width, height).GetImageBytes();
            return this.File(bytes, "image/png");
        }

        [HttpPut("{name}/OverflowDropsOldestEvents")]
        public async Task<IActionResult> UpdateOverflowDropsOldestEventsAsync(string name, [FromBody] bool overflowDropsOldestEvents, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Count count = await nsContext.Counts.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Name == name, cancellationToken);
            if (count == default)
            {
                return this.NotFound($"Count with name [{name}] does not exist.");
            }

            count.OverflowDropsOldestEvents = overflowDropsOldestEvents;
            count.ModifiedDate = DateTimeOffset.UtcNow;

            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(count);
        }

        private OkObjectResult Ok(Count count)
        {
            if (this.Request.Query.ContainsKey("bare"))
            {
                return this.Ok(count.Value);
            }
            else if (this.Request.Query.ContainsKey("bases"))
            {
                return this.Ok(CountWithBases.From(count));
            }
            else
            {
                return base.Ok(count);
            }
        }
    }
}
