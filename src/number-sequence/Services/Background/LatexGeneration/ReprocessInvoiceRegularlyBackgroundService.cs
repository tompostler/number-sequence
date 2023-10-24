using Cronos;
using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Contracts.Invoicing;

namespace number_sequence.Services.Background.LatexGeneration
{
    public sealed class ReprocessInvoiceRegularlyBackgroundService : SqlSynchronizedBackgroundService
    {
        public ReprocessInvoiceRegularlyBackgroundService(
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ReprocessInvoiceRegularlyBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        { }

        protected override List<CronExpression> Crons => new()
        {
            // 3 minutes past the hour, every 4 hours from 8AM through 8PM, Monday through Friday
            CronExpression.Parse("3 8-20/4 * * MON-FRI"),
        };

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DateTimeOffset fourteenDaysAgo = DateTimeOffset.UtcNow.AddDays(-14);
            List<Invoice> invoicesNeedingReprocessing = await nsContext.Invoices
                                                            .Where(x =>
                                                                !x.PaidDate.HasValue
                                                                && x.ReprocessRegularly
                                                                // It must have been processed at least once to be reprocessed regularly
                                                                && x.ProcessedAt < fourteenDaysAgo)
                                                            .ToListAsync(cancellationToken);

            foreach (Invoice invoiceNeedingReprocessing in invoicesNeedingReprocessing)
            {
                invoiceNeedingReprocessing.ProccessAttempt += 1;
                invoiceNeedingReprocessing.ProcessedAt = default;

                TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
                OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                    typeof(DurableTaskImpl.Orchestrators.InvoicePostlerLatexGenerationOrchestrator),
                    instanceId: $"{invoiceNeedingReprocessing.Id:0000}-{invoiceNeedingReprocessing.ProccessAttempt:00}_{NsStorage.C.LTBP.InvoicePostler}",
                    invoiceNeedingReprocessing.Id);
                this.logger.LogInformation($"Created orchestration {instance.InstanceId} to generate the pdf.");

                // Explicitly not updating the last modified date, so as to keep the data cleaner on when it was last touched compared to last processed
                _ = await nsContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
