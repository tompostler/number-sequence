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
    [Route("days-since")]
    public sealed class DaysSincesController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<DaysSincesController> logger;

        public DaysSincesController(
            IServiceProvider serviceProvider,
            ILogger<DaysSincesController> logger)
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
            DaysSince daysSince = await nsContext.DaysSinces
                .Include(x => x.Events)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (daysSince == default)
            {
                return this.NotFound($"Days since [{id}] not found.");
            }

            return this.Ok(daysSince);
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(string id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            id = id.ToLowerInvariant();
            DaysSince daysSince = await nsContext.DaysSinces
                .Include(x => x.Events)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (daysSince == default)
            {
                return this.NotFound($"Days since [{id}] not found.");
            }

            return this.Ok(daysSince.Events);
        }

        [HttpPost, RequiresToken]
        public async Task<IActionResult> CreateAsync([FromBody] DaysSince daysSince, CancellationToken cancellationToken)
        {
            daysSince.Id = daysSince.Id.ToLowerInvariant();

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Check if the days since already exists.
            if (await nsContext.DaysSinces.AnyAsync(x => x.Id == daysSince.Id, cancellationToken))
            {
                return this.Conflict($"Days since with id [{daysSince.Id}] already exists.");
            }

            // Check if we shouldn't make another days since for this account.
            Account account = await nsContext.Accounts.SingleAsync(x => x.Name == this.User.Identity.Name, cancellationToken);
            int countdaysSinceForAccount = await nsContext.DaysSinces.CountAsync(x => x.AccountName == account.Name, cancellationToken);
            if (countdaysSinceForAccount >= TierLimits.DaysSincePerAccount[account.Tier])
            {
                return this.Conflict($"Too many days since already created for account with name [{account.Name}].");
            }

            DaysSince toInsert = new()
            {
                AccountName = account.Name,
                Id = daysSince.Id.ToLowerInvariant(),
                FriendlyName = daysSince.FriendlyName,
                LastOccurrence = DateOnly.FromDateTime(DateTime.UtcNow),
                Value = daysSince.Value.Trim(),
            };

            // Split up the one value across the 4 lines, with at most DaysSince.MaxValueLineWidth char per line.
            // If it's not splittable, then it's a bad request.
            string[] valueComponents = toInsert.Value.Split(' ');
            List<string> chunks = new();
            string currentChunk = string.Empty;
            foreach (string component in valueComponents)
            {
                if ((currentChunk + component).Length < DaysSince.MaxValueLineWidth)
                {
                    currentChunk += (currentChunk.Length == 0 ? string.Empty : ' ') + component;
                }
                else
                {
                    chunks.Add(currentChunk);
                    currentChunk = component;
                }
            }
            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk);
            }

            // Validate.
            // We don't need to validate if there's more than 4, since the max characters is DaysSince.MaxValueLineWidth*4.
            if (chunks.Any(x => x.Length > DaysSince.MaxValueLineWidth))
            {
                return this.BadRequest($"Chunks cannot be longer than {DaysSince.MaxValueLineWidth}char. They were parsed as:\n{string.Join('\n', chunks)}");
            }

            // Store optimized.
            if (chunks.Count >= 1)
            {
                toInsert.ValueLine1 = chunks[0];
            }
            if (chunks.Count >= 2)
            {
                toInsert.ValueLine2 = chunks[1];
            }
            if (chunks.Count >= 3)
            {
                toInsert.ValueLine3 = chunks[2];
            }
            if (chunks.Count >= 4)
            {
                toInsert.ValueLine4 = chunks[3];
            }

            _ = nsContext.DaysSinces.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            DaysSince created = await nsContext.DaysSinces.SingleAsync(x => x.AccountName == account.Name && x.Id == toInsert.Id, cancellationToken);
            this.logger.LogInformation($"Created days since: {created.ToJsonString()}");
            return this.Ok(created);
        }

        [HttpPut, RequiresToken]
        public async Task<IActionResult> UpdateAsync([FromBody] DaysSince daysSince, CancellationToken cancellationToken)
        {
            daysSince.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            DaysSince daysSinceRecord = await nsContext.DaysSinces
                .Include(x => x.Events)
                .SingleOrDefaultAsync(x => x.AccountName == daysSince.AccountName && x.Id == daysSince.Id, cancellationToken);
            if (daysSinceRecord == default)
            {
                return this.NotFound();
            }

            // Allow the user to space apart the 4 lines however they want, as long as they remain equal to the original value.
            string calculatedValue = $"{daysSince.ValueLine1} {daysSince.ValueLine2} {daysSince.ValueLine3} {daysSince.ValueLine4}".Trim();
            if (daysSince.Value != calculatedValue)
            {
                return this.BadRequest($"The calculated value of:\n{calculatedValue}\nDoes not equal the actual value of:\n{daysSince.Value}");
            }

            daysSinceRecord.Value = daysSince.Value;
            daysSinceRecord.ValueLine1 = daysSince.ValueLine1;
            daysSinceRecord.ValueLine2 = daysSince.ValueLine2;
            daysSinceRecord.ValueLine3 = daysSince.ValueLine3;
            daysSinceRecord.ValueLine4 = daysSince.ValueLine4;

            List<DaysSinceEvent> daysSinceEvents = new();
            foreach (DaysSinceEvent daysSinceEvent in daysSince.Events)
            {
                DaysSinceEvent daysSinceEventRecord = daysSinceRecord.Events.SingleOrDefault(x => x.Id == daysSinceEvent.Id);
                if (daysSinceEvent.Id == default)
                {
                    daysSinceEvents.Add(daysSinceEvent);
                }
                else if (daysSinceEventRecord == default)
                {
                    return this.NotFound($"Event id [{daysSinceEvent.Id}] not found.");
                }
                else
                {
                    if (daysSinceEventRecord.Description != daysSinceEvent.Description
                        || daysSinceEventRecord.EventDate != daysSinceEvent.EventDate)
                    {
                        daysSinceEventRecord.ModifiedDate = DateTimeOffset.UtcNow;
                    }

                    daysSinceEventRecord.Description = daysSinceEvent.Description;
                    daysSinceEventRecord.EventDate = daysSinceEvent.EventDate;

                    daysSinceEvents.Add(daysSinceEventRecord);
                }
            }

            daysSinceRecord.Events = daysSinceEvents;
            daysSinceRecord.LastOccurrence = daysSinceRecord.Events.Max(x => x.EventDate);

            daysSinceRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(daysSinceRecord);
        }

        [HttpGet, RequiresToken]
        public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<DaysSince> daysSinces = await nsContext.DaysSinces
                .Include(x => x.Events)
                .Where(x => x.AccountName == this.User.Identity.Name)
                .ToListAsync(cancellationToken);
            return this.Ok(daysSinces);
        }
    }
}
