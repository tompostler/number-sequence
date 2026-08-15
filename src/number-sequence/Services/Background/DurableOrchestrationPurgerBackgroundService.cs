using Cronos;
using DurableTask.Core;
using Microsoft.ApplicationInsights;
using number_sequence.Utilities;

namespace number_sequence.Services.Background
{
    public sealed class DurableOrchestrationPurgerBackgroundService : SqlSynchronizedBackgroundService
    {
        public DurableOrchestrationPurgerBackgroundService(
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<DurableOrchestrationPurgerBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        { }

        protected override List<CronExpression> Crons => new()
        {
            // Sundays at 2am
            CronExpression.Parse("0 2 * * SUN"),
        };

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
            await taskHubClient.PurgeOrchestrationInstanceHistoryAsync(
                thresholdDateTimeUtc: DateTime.UtcNow.AddDays(-90),
                timeRangeFilterType: OrchestrationStateTimeRangeFilterType.OrchestrationCompletedTimeFilter);
        }
    }
}
