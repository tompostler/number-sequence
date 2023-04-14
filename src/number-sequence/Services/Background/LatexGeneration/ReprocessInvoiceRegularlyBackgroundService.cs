using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using number_sequence.DataAccess;
using number_sequence.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        protected override TimeSpan? Interval => TimeSpan.FromHours(4);

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DateTimeOffset fourteenDaysAgo = DateTimeOffset.UtcNow.AddDays(-14);
            List<Invoice> invoicesNeedingReprocessing = await nsContext.Invoices
                                                            .Where(x =>
                                                                !x.PaidDate.HasValue
                                                                && x.ReprocessRegularly
                                                                && x.ModifiedDate < fourteenDaysAgo)
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

                invoiceNeedingReprocessing.ModifiedDate = DateTimeOffset.UtcNow;
                _ = await nsContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
