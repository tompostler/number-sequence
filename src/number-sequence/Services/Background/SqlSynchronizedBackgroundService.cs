using Cronos;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Utilities;

namespace number_sequence.Services.Background
{
    public abstract class SqlSynchronizedBackgroundService : BackgroundService
    {
        // Personal project, align to my time zone
        private static readonly TimeZoneInfo CentralUSTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

        protected readonly IServiceProvider serviceProvider;
        protected readonly Sentinals sentinals;
        protected readonly ILogger logger;

        private readonly TelemetryClient telemetryClient;
        protected IOperationHolder<RequestTelemetry> op;

        public SqlSynchronizedBackgroundService(
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger logger,
            TelemetryClient telemetryClient)
        {
            this.serviceProvider = serviceProvider;
            this.sentinals = sentinals;
            this.logger = logger;
            this.telemetryClient = telemetryClient;
        }

        protected override sealed async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!this.Interval.HasValue && this.Cron == default)
            {
                throw new InvalidOperationException("An Interval or Cron must be set for the background service. Neither were set.");
            }
            if (this.Interval.HasValue && this.Cron!= default)
            {
                throw new InvalidOperationException("An Interval or Cron must be set for the background service. Both were set.");
            }

            await this.sentinals.DBMigration.WaitForCompletionAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // Try to make sure there's only one instance of the service attempting to run at any time
                using (IServiceScope scope = this.serviceProvider.CreateScope())
                using (NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>())
                {
                    Models.SynchronizedBackgroundService lastExecution = await nsContext.SynchronizedBackgroundServices.SingleOrDefaultAsync(x => x.Name == this.GetType().FullName, stoppingToken);
                    DateTimeOffset nextExecution = this.DetermineNextExecution(lastExecution?.LastExecuted);

                    // We have a previous record and we should wait before executing again
                    if (lastExecution != default && DateTimeOffset.UtcNow < nextExecution)
                    {
                        this.logger.LogInformation($"Last execution of {this.GetType().FullName} was {DateTimeOffset.UtcNow - lastExecution.LastExecuted} ago when the interval is [{this.Interval}] and the cron is [{this.Cron}]. Next execution is expected at {nextExecution:u}");
                        TimeSpan timeUntilNextExpectedExecution = nextExecution - DateTimeOffset.UtcNow;
                        var durationToSleep = TimeSpan.FromMinutes(Math.Max(5, timeUntilNextExpectedExecution.TotalMinutes));
                        this.logger.LogInformation($"Determined we should sleep for {durationToSleep}");
                        await Task.Delay(durationToSleep, stoppingToken);
                        continue;
                    }

                    // We've never run or are ready to run again, save off the record of our execution
                    if (lastExecution == default)
                    {
                        lastExecution = new Models.SynchronizedBackgroundService { Name = this.GetType().FullName };
                        _ = nsContext.SynchronizedBackgroundServices.Add(lastExecution);
                    }
                    lastExecution.LastExecuted = DateTimeOffset.UtcNow;
                    lastExecution.CountExecutions++;
                    _ = await nsContext.SaveChangesAsync(stoppingToken);
                }

                // And then actually do the work
                using IOperationHolder<RequestTelemetry> op = this.telemetryClient.StartOperation<RequestTelemetry>(this.GetType().FullName);
                this.op = op;
                try
                {
                    // Limit the amount of execution time to complete before the next interval (but be at least 5 minutes)
                    DateTimeOffset nextExecutionAfterNow = this.DetermineNextExecution(DateTimeOffset.UtcNow);
                    var maximumAmountOfTimeToLetExecute = TimeSpan.FromMinutes(Math.Max(5, nextExecutionAfterNow.Subtract(DateTimeOffset.UtcNow).TotalMinutes));
                    using CancellationTokenSource intervalTokenSource = new(maximumAmountOfTimeToLetExecute);
                    using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, intervalTokenSource.Token);

                    await this.ExecuteOnceAsync(linkedTokenSource.Token);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    this.logger.LogError(ex, "Failed execution.");
                }
            }
        }

        private DateTimeOffset DetermineNextExecution(DateTimeOffset? lastExecution)
        {
            if (!lastExecution.HasValue)
            {
                // Never run before, it should be run immediately
                return DateTimeOffset.MinValue;
            }

            if (this.Interval.HasValue)
            {
                return lastExecution.Value.Add(this.Interval.Value);
            }
            else if (this.Cron != default)
            {
                return this.Cron.GetNextOccurrence(lastExecution.Value, CentralUSTimeZone).Value;
            }
            else
            {
                throw new InvalidOperationException("An Interval or Cron must be set for the background service. Neither were set.");
            }
        }

        protected virtual TimeSpan? Interval { get; }

        protected virtual CronExpression Cron { get; }

        protected abstract Task ExecuteOnceAsync(CancellationToken cancellationToken);
    }
}
